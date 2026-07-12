using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AuthenticationAPI.Common.Annotations;

public partial class PersonNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text)
            return ValidationResult.Success;

        if (NameRegex().IsMatch(text))
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage ?? "O campo deve conter apenas letras.");
    }

    [GeneratedRegex(@"^[\p{L}\s]+$")]
    private static partial Regex NameRegex();
}
