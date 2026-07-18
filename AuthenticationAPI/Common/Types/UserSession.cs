namespace AuthenticationAPI.Common.Types;

public sealed record UserSession(
    string AccessToken,
    Guid RefreshToken,
    string DeviceName
);