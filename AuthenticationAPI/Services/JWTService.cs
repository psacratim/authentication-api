using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationAPI.Common;
using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Database.Entities;
using AuthenticationAPI.DTO.Responses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationAPI.Services;

public class JWTService(
    IOptions<JwtOptions> _jwtOptions
) : IJWTService
{
    private readonly JwtOptions jwtOptions = _jwtOptions.Value;

    public Result<JWTResponse> GenerateNewJWT(Account account, string deviceName)
    {
        var claims = new List<Claim>{
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, account.Name ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, account.Email ?? string.Empty)
        };

        var issuerSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey));
        var credentials = new SigningCredentials(issuerSigninKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );
        var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return Result<JWTResponse>.Success(new JWTResponse
        {
            AccessToken = accessToken,
            RefreshToken = Guid.NewGuid()
        });
    }
}
