using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Attributes;

public class AllowedExtentionAttribute : ValidationAttribute
{
    public string _allowedExtensions { get; set; }

    public AllowedExtentionAttribute(string allowedExtensions)
    {
        _allowedExtensions = allowedExtensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var fileName = file.FileName;
            var ext = Path.GetExtension(fileName);
            if (!_allowedExtensions.Split(',').Any(x => x.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult($"The file extension {ext} is not allowed. Allowed extensions are: {_allowedExtensions}");
            }
        }
        return ValidationResult.Success;
    }


}
