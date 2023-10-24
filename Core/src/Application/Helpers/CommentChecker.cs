using System.IO.Compression;
using System.Text.RegularExpressions;

namespace aia_api.Application.Helpers;

public class CommentChecker
{
    private readonly ILogger<CommentChecker> _logger;

    public CommentChecker(ILogger<CommentChecker> logger)
    {
        _logger = logger;
    }

    public bool HasComments(ZipArchiveEntry zipArchiveEntry, string fileExtension)
    {
        switch (fileExtension)
        {
            case ".ts":
                string pattern = @"((?<=\s|^)\/\/[^\n]*|\/\*[\s\S]*?\*\/|\/\*\*[\s\S]*?\*\/)";
                return FileHasComments(zipArchiveEntry, pattern);
            default:
                throw new ArgumentException("File extension not supported.");
        }
    }

    private bool FileHasComments(ZipArchiveEntry file, string pattern)
    {
        var hasComments = false;
        using var reader = new StreamReader(file.Open());
        var fileContent = reader.ReadToEnd();

        var matches = Regex.Matches(fileContent, pattern, RegexOptions.Multiline);
        reader.Close();

        if (matches.Count > 0)
        {
            hasComments = true;
            _logger.LogInformation("{Name} contains comments.", file.FullName);

            foreach (Match match in matches)
            {
                _logger.LogInformation("Found comment: {Value}", match.Value);
            }
        }
        _logger.LogInformation("{Name} does not contain comments.", file.Name);

        return hasComments;
    }
}
