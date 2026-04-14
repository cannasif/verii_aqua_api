using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Shared.Host.WebApi.Hubs
{
    [Authorize]
    public class AuthHub : Hub
    {
        private readonly AquaDbContext _context;
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, string> _connectionUsers = new();

        public AuthHub(AquaDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = Context.User?.FindFirst("sid")?.Value;

            if (userId != null && sessionId != null)
            {
                // Mevcut aktif oturumu kontrol et
                var activeSession = await _context.Set<UserSession>()
                    .FirstOrDefaultAsync(us => us.UserId == long.Parse(userId) && us.RevokedAt == null);

                if (activeSession != null)
                {
                    // Eğer yeni bir oturumdan giriş yapıldıysa, eski bağlantıyı kapat
                    if (activeSession.SessionId.ToString() != sessionId)
                    {
                        if (_userConnections.TryGetValue(userId, out var oldConnectionId))
                        {
                            await Clients.Client(oldConnectionId).SendAsync("ForceLogout", "Başka bir cihazdan giriş yapıldı.");
                            _userConnections.Remove(userId);
                            _connectionUsers.Remove(oldConnectionId);
                        }
                    }
                }
                else
                {
                    // Geçersiz oturum, bağlantıyı reddet
                    Context.Abort();
                    return;
                }

                // Yeni bağlantıyı kaydet
                _userConnections[userId] = Context.ConnectionId;
                _connectionUsers[Context.ConnectionId] = userId;
            }
            else
            {
                // Geçersiz kullanıcı bilgisi, bağlantıyı reddet
                Context.Abort();
                return;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionUsers.TryGetValue(Context.ConnectionId, out var userId))
            {
                _userConnections.Remove(userId);
                _connectionUsers.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Belirli bir kullanıcıyı sistemden çıkmaya zorla
        public static async Task ForceLogoutUser(IHubContext<AuthHub> hubContext, string userId)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await hubContext.Clients.Client(connectionId).SendAsync("ForceLogout", "Oturumunuz sonlandırıldı.");
                _userConnections.Remove(userId);
                _connectionUsers.Remove(connectionId);
            }
        }

        // Kullanıcıya bildirim gönder
        public static async Task SendNotificationToUser(IHubContext<AuthHub> hubContext, string userId, string message, string type = "info")
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", new
                {
                    message,
                    type,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // Tüm kullanıcılara bildirim gönder
        public static async Task SendNotificationToAll(IHubContext<AuthHub> hubContext, string message, string type = "info")
        {
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                message,
                type,
                timestamp = DateTime.UtcNow
            });
        }

        // Aktif kullanıcı sayısını al
        public static int GetActiveUserCount()
        {
            return _userConnections.Count;
        }

        // Belirli bir kullanıcının online olup olmadığını kontrol et
        public static bool IsUserOnline(string userId)
        {
            return _userConnections.ContainsKey(userId);
        }
    }
}
