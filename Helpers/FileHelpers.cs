using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using MvcMovie.Settings;

namespace MvcMovie.Helpers;

public class FileHelpers
{

    public static bool IsValidExcelFile(IFormFile file)
    {
        // Check if the file is not null and has a valid extension
        return file != null && FileSettings.ValidExcelExtensions.Any(ext => file.FileName.Contains(ext, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValidType(IFormFile file, string validExtensions)
    {
        // Check if the file is not null and has a valid extension
        return file != null && validExtensions.Any(ext => file.FileName.Contains(ext, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValidSize(IFormFile file, int validSizeMegaByte)

    {
        // Check if the file is not null and has a valid extension
        return file != null && file.Length <= validSizeMegaByte * 1024 * 1024;
    }

    public static (string, string) GetFileUniqueName(IFormFile file)
    {
        // Generate a unique file name using the current timestamp and the original file name
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), FileSettings.UploadsFolder);
        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
        var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, fileName);
        return (fileName, filePath);
    }


}
