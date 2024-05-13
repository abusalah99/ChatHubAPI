namespace VedioCall;

public interface IJwtProvider
{
    string GenrateAccessToken(User user);

}
