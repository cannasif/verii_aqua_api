using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/OpeningImport")]
    [Authorize]
    public class OpeningImportController : ControllerBase
    {
        private readonly IOpeningImportService _service;

        public OpeningImportController(IOpeningImportService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<OpeningImportPreviewResponseDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("preview")]
        public async Task<ActionResult<ApiResponse<OpeningImportPreviewResponseDto>>> Preview([FromBody] OpeningImportPreviewRequestDto dto)
        {
            var result = await _service.PreviewAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:long}/commit")]
        public async Task<ActionResult<ApiResponse<OpeningImportCommitResultDto>>> Commit(long id)
        {
            var result = await _service.CommitAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:long}/cleanup-soft-deleted")]
        public async Task<ActionResult<ApiResponse<OpeningImportCleanupSoftDeletedResultDto>>> CleanupSoftDeletedReferences(long id)
        {
            var result = await _service.CleanupSoftDeletedReferencesAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:long}/reset-existing-data")]
        public async Task<ActionResult<ApiResponse<OpeningImportResetExistingDataResultDto>>> ResetExistingData(long id)
        {
            var result = await _service.ResetExistingDataAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
