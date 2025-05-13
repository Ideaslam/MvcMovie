using System.ComponentModel.DataAnnotations;
using MvcMovie.Settings;

namespace MvcMovie.Attributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private int _allowedFileSize { get; set; }
    public MaxFileSizeAttribute(int allowedFileSize)
    {
        _allowedFileSize = allowedFileSize;
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            
            var isAllowed = file.Length <= _allowedFileSize * 1024 * 1024;
           
            if (!isAllowed)
            {
                return new ValidationResult($"The file size exceeds the allowed limit of {_allowedFileSize} MB.");
            }
        }
        return ValidationResult.Success;
    }


}
