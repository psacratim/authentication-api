using System.ComponentModel.DataAnnotations;
using AuthenticationAPI.Common.Annotations;

namespace AuthenticationAPI.DTO.Requests;

public record SignupRequest
{
    [MaxLength(128)]
    [Required]
    [PersonName]
    public required string FullName { get; set; }

    [MaxLength(80)]
    [Required]
    [EmailAddress]
    public required string Email { get => _email; set => _email = value.Trim().ToLowerInvariant(); }

    [MaxLength(256)]
    public string? Password { get; set; }

    // Normalized values
    private string _email = string.Empty;
}