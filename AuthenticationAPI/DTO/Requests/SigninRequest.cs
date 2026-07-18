using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.DTO.Requests;

public record SigninRequest
{

    [EmailAddress]
    [MaxLength(80)]
    [Required]
    public required string Email { get => _email; set => _email = value.Trim().ToLowerInvariant(); }

    [MaxLength(256)]
    public string? Password { get; set; }

    // Normalized values
    private string _email = string.Empty;
}
