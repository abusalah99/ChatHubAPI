namespace VedioCall;

public class BaseEntity
{
    public Guid Id { get; set; }
    [JsonIgnore]
    public bool IsDeleted { get; set; } = false;
}
