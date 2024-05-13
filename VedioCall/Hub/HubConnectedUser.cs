namespace VedioCall;

public record HubConnectedUser
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    [JsonIgnore]
    public string ConnectionId { get; set; } = null!;
    [JsonIgnore]
    public RoleEnum Role { get; set; }
}
