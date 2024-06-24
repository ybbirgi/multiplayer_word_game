namespace WordGame.Dtos.Accounts;

public class LoginResultDto
{
    public bool AuthenticationRequired { get; set; }
    public PasswordLoginResult PasswordLoginResult { get; set; }
    public TokenDto Token { get; set; }
}