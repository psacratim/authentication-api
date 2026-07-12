namespace AuthenticationAPI.Common.Results.Internal;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string FullName
);