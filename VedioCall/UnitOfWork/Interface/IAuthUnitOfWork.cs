namespace VedioCall;

public interface IAuthUnitOfWork
{
    Task<TokenDto> Register(User user);
    Task<TokenDto?> Login(LoginDto loginDto);
    Task<User> MapFromUserRegistrationDtoToUser(UserRegistrationDto dto);
}
