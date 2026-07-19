using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Results.Internal;
using AuthenticationAPI.Common.Types;
using AuthenticationAPI.DTO.Requests;
using AuthenticationAPI.DTO.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Common.Interfaces;

public interface IAuthService
{
    public Task<Result<UserSession>> Signup(SignupRequest request, CancellationToken cancellationToken = default);
    public Task<Result<UserSession>> Signin(SigninRequest request, AuthenticatedUser? user, CancellationToken cancellationToken = default);
    public Task<Result> Logout(Guid refreshToken, Guid userId, CancellationToken cancellationToken = default);
    public Task<Result<UserSession>> Refresh(Guid refreshToken, CancellationToken cancellationToken = default);
}
