namespace VedioCall;
public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public RoleEnum Role { get; set; } = RoleEnum.User;
    public string ImagePath { get; set; } = null!;
}

