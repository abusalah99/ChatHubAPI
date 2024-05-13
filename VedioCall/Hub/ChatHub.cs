
namespace VedioCall;
[Authorize]
public class ChatHub : Hub<IChatHubClients>
{
    private readonly ChatHubConnectedUser _conecctedUsers;

    private Dictionary<Guid, Guid> activeCalls = new();

    public ChatHub(ChatHubConnectedUser conecctedUsers) => _conecctedUsers = conecctedUsers;

    public override async Task OnConnectedAsync()
    {
        string userId = Context?.User?.FindFirstValue("Id") ?? "";
        string role = Context?.User?.FindFirstValue(ClaimTypes.Role) ?? "";
        string name = Context?.User?.FindFirstValue(ClaimTypes.Name) ?? "";

        Guid userIdGuid = Guid.Parse(userId);

        if (!_conecctedUsers.HubConnectedUsers.Any(u => u.Id == userIdGuid))
        {
            var connectedUser = new HubConnectedUser
            {
                Id = userIdGuid,
                ConnectionId = Context!.ConnectionId,
                Role = (RoleEnum)Enum.Parse(typeof(RoleEnum), role),
                Name = name
            }; 

            _conecctedUsers.HubConnectedUsers.Add(connectedUser);
        }

        foreach (var user in _conecctedUsers.HubConnectedUsers)
        {
            List<HubConnectedUser> otherUsers = _conecctedUsers.HubConnectedUsers
                                                               .Where(e => e.Id != user.Id)
                                                               .ToList();

            await Clients.Client(user.ConnectionId).UserConnect(otherUsers);
        }

        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _conecctedUsers.HubConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
        if (user != null)
        {
            _conecctedUsers.HubConnectedUsers.Remove(user);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task StartCall(Guid targetUserId)
    {
        string userId = Context?.User?.FindFirstValue("Id") ?? "";

        Guid callerId = Guid.Parse(userId);

        string? calleeId = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == targetUserId).Select(e => e.ConnectionId).FirstOrDefault();

        if (activeCalls.ContainsValue(targetUserId))
        {
            await Clients.Caller.Busy();
            return;
        }

        if (calleeId != null)
            await Clients.User(calleeId).ReceiveCall(callerId);
    }
    public async Task AnswerCall(Guid callerId)
    {
        string? caller = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == callerId).Select(e => e.ConnectionId).FirstOrDefault();
        if (caller != null)
        {
            string userId = Context?.User?.FindFirstValue("Id") ?? "";

            Guid calleeId = Guid.Parse(userId);

            activeCalls[callerId] = calleeId;

            await Clients.User(caller).CallAccepted();
        }
    }

    public async Task RejectCall(Guid callerId)
    {
        string? caller = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == callerId).Select(e => e.ConnectionId).FirstOrDefault();

        if (caller != null)
            await Clients.User(caller).CallRejected();
    }

    public async Task HangUp()
    {
        Guid userId = Guid.Parse(Context?.User?.FindFirstValue("Id") ?? "");

        if (activeCalls.ContainsKey(userId))
        {
            Guid partnerId = activeCalls[userId];

            string user = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == userId).Select(e => e.ConnectionId).FirstOrDefault()!;

            string partner = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == userId).Select(e => e.ConnectionId).FirstOrDefault()!;

            await Clients.User(user).CallEnded();
            await Clients.User(partner).CallEnded();

            activeCalls.Remove(userId);
            activeCalls.Remove(partnerId);
        }
    }
    public async Task SendIceCandidate(Guid targetUserId, string candidate)
    {
        Guid callerId = Guid.Parse(Context?.User?.FindFirstValue("Id") ?? "");

        string? calleeId = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == targetUserId).Select(e => e.ConnectionId).FirstOrDefault();

        if(calleeId != null)
            await Clients.User(calleeId).ReceiveIceCandidate(callerId, candidate);
    }

    public async Task SendSDP(Guid targetUserId, string sdp)
    {
        Guid callerId = Guid.Parse(Context?.User?.FindFirstValue("Id") ?? "");

        string? calleeId = _conecctedUsers.HubConnectedUsers.Where(u => u.Id == targetUserId).Select(e => e.ConnectionId).FirstOrDefault();

        if (calleeId != null)
            await Clients.User(calleeId).ReceiveSDP(callerId, sdp);
    }
}