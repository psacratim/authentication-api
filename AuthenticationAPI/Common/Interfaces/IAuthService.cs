using AuthenticationAPI.Common.Results;
using AuthenticationAPI.DTO.Requests;
using AuthenticationAPI.DTO.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Common.Interfaces;

public interface IAuthService
{
    public Task<Result<JWTResponse>> Signup(SignupRequest request, CancellationToken cancellationToken = default);
    public Task<Result<JWTResponse>> Signin(SigninRequest request, CancellationToken cancellationToken = default);
    public Task<Result> Logout(Guid refreshToken, Guid userId, CancellationToken cancellationToken = default);
    public Task<Result<JWTResponse>> Refresh(Guid refreshToken, CancellationToken cancellationToken = default);
}
