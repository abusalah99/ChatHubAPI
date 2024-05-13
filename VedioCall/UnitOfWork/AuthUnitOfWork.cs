namespace VedioCall;
public class AuthUnitOfWork : IAuthUnitOfWork
{
    private readonly IRepository<User> _repository;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IJwtProvider _jwtProvider;
    private readonly JwtAccessOptions _jwtAccessOptions;

    public AuthUnitOfWork(IRepository<User> repository, IWebHostEnvironment env,
        IHttpContextAccessor contextAccessor, IJwtProvider jwtProvider, IOptions<JwtAccessOptions> jwtAccessOption)
    {
        _repository = repository;
        _env = env;
        _contextAccessor = contextAccessor;
        _jwtProvider = jwtProvider;
        _jwtAccessOptions = jwtAccessOption.Value;
    }

    public async Task<TokenDto> Register(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        user.Role = RoleEnum.User;

        await _repository.Add(user);

        TokenDto token = new()
        {
            Value = _jwtProvider.GenrateAccessToken(user),
            ExpireAt = DateTime.UtcNow.AddMonths(_jwtAccessOptions.ExpireTimeInMonths),
        };

        _contextAccessor.CreatedCookie("AccessToken", token.Value, token.ExpireAt);

        return token;
    }

    public async Task<TokenDto?> Login(LoginDto dto)
    {
        User? userFromDb = null;

        if (!dto.Email.IsNullOrEmpty())
            userFromDb = await _repository.GetSingleEntityWithSomeCondiition(q => q.Where(u => u.Email == dto.Email));

        if (userFromDb == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, userFromDb.Password))
            return null;

        TokenDto token = new()
        {
            Value = _jwtProvider.GenrateAccessToken(userFromDb),
            ExpireAt = DateTime.UtcNow.AddMonths(_jwtAccessOptions.ExpireTimeInMonths),
        };

        _contextAccessor.CreatedCookie("AccessToken", token.Value, token.ExpireAt);

        return token;
    }

    public async Task<User> MapFromUserRegistrationDtoToUser(UserRegistrationDto dto)
    {
        User user = new()
        {
            Email = dto.Email,
            Name = dto.Name,
            Password = dto.Password
        };

        string? imageName = null;

        if (dto.Image != null)
            imageName = await dto.Image.SaveImageAsync(_env);

        user.ImagePath = imageName.GetFileUrl(_contextAccessor) ?? "";

        return user;
    }
}
