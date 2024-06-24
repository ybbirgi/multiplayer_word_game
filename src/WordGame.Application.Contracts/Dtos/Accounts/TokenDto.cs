namespace WordGame.Dtos.Accounts;

public class TokenDto
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
}