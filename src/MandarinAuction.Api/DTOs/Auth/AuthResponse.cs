namespace MandarinAuction.Api.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public AuthResponse() { }

    public AuthResponse(string token, string userName, string email)
    {
        Token = token;
        UserName = userName;
        Email = email;
    }
}
