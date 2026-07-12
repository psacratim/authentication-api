using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Database.Entities;
using AuthenticationAPI.DTO.Responses;

namespace AuthenticationAPI.Common.Interfaces;

public interface IJWTService
{
    public Result<JWTResponse> GenerateNewJWT(Account account);
}
