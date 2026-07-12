namespace AuthenticationAPI.DTO.Responses;

public record JWTResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public Guid RefreshToken { get; set; }
}
