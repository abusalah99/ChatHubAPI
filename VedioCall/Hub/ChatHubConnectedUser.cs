namespace VedioCall;

public record ChatHubConnectedUser
{
    public List<HubConnectedUser> HubConnectedUsers { get; set; } = new();
}

