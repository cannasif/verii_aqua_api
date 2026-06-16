using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using aqua_api.Modules.Integrations.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace aqua_api.Modules.Integrations.Application.Services;

public sealed class NetsisItemSlipService : INetsisItemSlipService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly INetsisRestClient _netsisRestClient;
    private readonly IOptions<NetsisOptions> _netsisOptions;
    private readonly ILogger<NetsisItemSlipService> _logger;

    public NetsisItemSlipService(
        INetsisRestClient netsisRestClient,
        IOptions<NetsisOptions> netsisOptions,
        ILogger<NetsisItemSlipService> logger)
    {
        _netsisRestClient = netsisRestClient;
        _netsisOptions = netsisOptions;
        _logger = logger;
    }

    public Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferOutAsync(
        NetsisItemSlipCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var configuredDocumentType = _netsisOptions.Value.Rest.WarehouseTransferOutDocumentType;
        var documentType = (NetsisItemSlipDocumentType)(configuredDocumentType > 0 ? configuredDocumentType : 9);
        return CreateDocumentAsync(request, documentType, cancellationToken);
    }

    public Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferInAsync(
        NetsisItemSlipCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var configuredDocumentType = _netsisOptions.Value.Rest.WarehouseTransferInDocumentType;
        var documentType = (NetsisItemSlipDocumentType)(configuredDocumentType > 0 ? configuredDocumentType : 8);
        return CreateDocumentAsync(request, documentType, cancellationToken);
    }

    public async Task<NetsisItemSlipCreateResponseDto> CreateDocumentAsync(
        NetsisItemSlipCreateDto request,
        NetsisItemSlipDocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request, documentType);

        var options = _netsisOptions.Value;
        if (!options.Enabled)
        {
            throw new NetsisIntegrationValidationException("Netsis entegrasyonu kapalı. Sunucu entegrasyon ayarlarını kontrol edin.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ResolveItemSlipsPath(options))
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        try
        {
            using var response = await _netsisRestClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var result = ParseResponse(responseBody);
            result.RawResponse = responseBody;

            if (!response.IsSuccessStatusCode || !IsSuccessful(result))
            {
                var failure = ResolveFailureMessage(response, result, responseBody);
                throw new NetsisIntegrationValidationException(ResolveUserFriendlyFailureMessage(result, failure));
            }

            _logger.LogInformation(
                "Netsis ItemSlip created. DocumentType: {DocumentType}, FatirsNo: {FatirsNo}, FisNo: {FisNo}, KayitNo: {KayitNo}.",
                documentType,
                request.FatUst.FatirsNo,
                result.Data?.FisNo,
                result.Data?.KayitNo);

            return result;
        }
        catch (Exception ex) when (ex is not NetsisIntegrationValidationException and not InvalidOperationException)
        {
            _logger.LogError(
                ex,
                "Netsis ItemSlip creation failed. DocumentType: {DocumentType}, FatirsNo: {FatirsNo}, LineCount: {LineCount}.",
                documentType,
                request.FatUst.FatirsNo,
                request.Kalems.Count);
            throw;
        }
    }

    private static void ValidateRequest(NetsisItemSlipCreateDto request, NetsisItemSlipDocumentType documentType)
    {
        request.FatUst.Tip = documentType;
        request.FatUst.Tipi ??= documentType is NetsisItemSlipDocumentType.WarehouseTransferIn or NetsisItemSlipDocumentType.WarehouseTransferOut
            ? NetsisItemSlipInvoiceType.Miscellaneous
            : NetsisItemSlipInvoiceType.DomesticClosed;

        if (documentType is NetsisItemSlipDocumentType.WarehouseTransferIn or NetsisItemSlipDocumentType.WarehouseTransferOut)
        {
            request.FatUst.WarehouseMovementType ??= NetsisWarehouseMovementType.Production;
            request.FatUst.IssuePlace ??= NetsisWarehouseIssuePlace.CostCenter;
        }

        if (request.Kalems.Count == 0)
        {
            throw new NetsisIntegrationValidationException("Netsis ambar fişi için en az bir kalem olmalıdır.");
        }

        var invalidLine = request.Kalems.FirstOrDefault(line =>
            string.IsNullOrWhiteSpace(line.StokKodu) ||
            line.Miktar <= 0 ||
            line.DepoKodu is null or <= 0);
        if (invalidLine != null)
        {
            throw new NetsisIntegrationValidationException("Netsis ambar fişi için tüm kalemlerde stok kodu, depo kodu ve sıfırdan büyük miktar olmalıdır.");
        }
    }

    private static string ResolveItemSlipsPath(NetsisOptions options)
    {
        return string.IsNullOrWhiteSpace(options.Rest.ItemSlipsPath)
            ? "/api/v2/ItemSlips"
            : options.Rest.ItemSlipsPath;
    }

    private static NetsisItemSlipCreateResponseDto ParseResponse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new NetsisItemSlipCreateResponseDto { IsSuccessful = false };
        }

        try
        {
            var response = JsonSerializer.Deserialize<NetsisItemSlipCreateResponseDto>(responseBody, JsonOptions);
            response ??= new NetsisItemSlipCreateResponseDto { IsSuccessful = false };
            HydrateReferenceFieldsFromRawResponse(response, responseBody);
            return response;
        }
        catch
        {
            return new NetsisItemSlipCreateResponseDto
            {
                IsSuccessful = false,
                ErrorDesc = responseBody,
                RawResponse = responseBody
            };
        }
    }

    private static void HydrateReferenceFieldsFromRawResponse(NetsisItemSlipCreateResponseDto response, string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            CollectReferenceCandidates(document.RootElement, values);

            response.Data ??= new NetsisItemSlipResponseDataDto();
            response.Data.FisNo ??= FirstValue(values, "FisNo", "FISNO", "Fis_No", "FIS_NO", "FATIRS_NO", "FatirsNo");
            response.Data.BelgeNo ??= FirstValue(values, "BelgeNo", "BELGE_NO", "Belge_No", "BelgeNumarasi", "BelgeNumarası");
            response.Data.KayitNo ??= FirstValue(values, "KayitNo", "KAYIT_NO", "Kayit_No", "KayıtNo", "KayitNumarasi");
            response.Data.ReferenceNumber ??= FirstValue(values, "ReferenceNumber", "REFERENCE_NUMBER", "ReferansNo", "ReferansKodu", "RefNo");
        }
        catch
        {
            // Netsis can return non-standard payloads; keep RawResponse for the caller/header integration state.
        }
    }

    private static void CollectReferenceCandidates(JsonElement element, IDictionary<string, string?> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
                    {
                        values.TryAdd(property.Name, property.Value.ToString());
                    }

                    CollectReferenceCandidates(property.Value, values);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectReferenceCandidates(item, values);
                }
                break;
        }
    }

    private static string? FirstValue(IReadOnlyDictionary<string, string?> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool IsSuccessful(NetsisItemSlipCreateResponseDto result)
    {
        return (result.IsSuccessful || result.IsSuccessStatusCode == true)
            && string.IsNullOrWhiteSpace(result.ErrorDesc)
            && string.IsNullOrWhiteSpace(result.ErrorDescription);
    }

    private static string ResolveFailureMessage(
        HttpResponseMessage response,
        NetsisItemSlipCreateResponseDto result,
        string responseBody)
    {
        if (!string.IsNullOrWhiteSpace(result.ErrorDesc))
        {
            return $"Netsis ItemSlips request failed ({(int)response.StatusCode}). {result.ErrorDesc}";
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorDescription))
        {
            return $"Netsis ItemSlips request failed ({(int)response.StatusCode}). {result.ErrorDescription}";
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorCode))
        {
            return $"Netsis ItemSlips request failed ({(int)response.StatusCode}). ErrorCode: {result.ErrorCode}";
        }

        return $"Netsis ItemSlips request failed ({(int)response.StatusCode}). Body: {responseBody}";
    }

    private static string ResolveUserFriendlyFailureMessage(NetsisItemSlipCreateResponseDto result, string fallback)
    {
        var rawMessage = FirstNonEmpty(result.ErrorDesc, result.ErrorDescription, fallback);
        var cleanMessage = CleanNetsisMessage(rawMessage);
        var lowerMessage = cleanMessage.ToLowerInvariant();

        if (lowerMessage.Contains("stok") && lowerMessage.Contains("bulunamad"))
        {
            return "Netsis stok kodu bulunamadı. ERP stok kaydını ve Aqua stok eşlemesini kontrol edin.";
        }

        if (lowerMessage.Contains("depo") && lowerMessage.Contains("bulunamad"))
        {
            return "Netsis depo kodu bulunamadı. Kafes/depo ERP depo eşlemesini kontrol edin.";
        }

        if (string.IsNullOrWhiteSpace(cleanMessage))
        {
            return "Netsis ambar fişi oluşturulamadı. Entegrasyon ayarlarını ve hareket bilgilerini kontrol edin.";
        }

        return $"Netsis ambar fişi oluşturulamadı: {cleanMessage}";
    }

    private static string CleanNetsisMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        var withoutXmlTags = Regex.Replace(message, "<[^>]+>", " ");
        var withoutPrefix = Regex.Replace(withoutXmlTags, @"Netsis ItemSlips request failed \(\d+\)\.?", string.Empty, RegexOptions.IgnoreCase);
        return Regex.Replace(withoutPrefix, @"\s+", " ").Trim();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
