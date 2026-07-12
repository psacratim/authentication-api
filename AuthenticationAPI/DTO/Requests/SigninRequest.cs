using System.ComponentModel.DataAnnotations;
using AuthenticationAPI.Common.Enums;

namespace AuthenticationAPI.DTO.Requests;

public record SigninRequest
{
    [Required]
    [EnumDataType(typeof(AuthMethod))]
    public required AuthMethod AuthMethod { get; set; }

    [EmailAddress]
    [MaxLength(80)]
    [Required]
    public required string Email { get => _email; set => _email = value.Trim().ToLowerInvariant(); }

    [MaxLength(256)]
    public string? Password { get; set; }

    // Normalized values
    private string _email = string.Empty;
}
