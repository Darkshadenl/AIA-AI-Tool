using System.IO.Compression;
using System.Text.RegularExpressions;

namespace aia_api.Application.Helpers;

public class CommentChecker
{
    private readonly ILogger<CommentChecker> _logger;
    private readonly List<string> _logs;

    public CommentChecker(ILogger<CommentChecker> logger)
    {
        _logger = logger;
        _logs = new List<string>();
    }

    public void LogLogsAndClear()
    {
        var logs = string.Join("\n", _logs);
        _logger.LogInformation(logs);
        _logs.Clear();
    }

    public bool HasComments(ZipArchiveEntry zipArchiveEntry, string fileExtension)
    {
        switch (fileExtension)
        {
            case ".ts":
                var detectCommentsPattern = @"((?<=\s|^)\/\/[^\n]*|\/\*[\s\S]*?\*\/|\/\*\*[\s\S]*?\*\/)";
                var detectEslintCommentsPattern =
                    @"(\/\/.*eslint-.*|\/\*[\s\S]*?eslint-[\s\S]*?\*\/|\/\*\*[\s\S]*?eslint-[\s\S]*?\*\/)";
                return FileHasComments(zipArchiveEntry, detectCommentsPattern, detectEslintCommentsPattern);
            default:
                throw new ArgumentException("File extension not supported.");
        }
    }

    private bool FileHasComments(ZipArchiveEntry file, string allCommentPattern, string eslintCommentPattern)
    {
        var hasComments = false;
        using var reader = new StreamReader(file.Open());
        var fileContent = reader.ReadToEnd();

        var allCommentsMatches = Regex.Matches(fileContent, allCommentPattern, RegexOptions.Multiline);
        var eslintCommentsMatches = Regex.Matches(fileContent, eslintCommentPattern, RegexOptions.Multiline);
        reader.Close();

        if (allCommentsMatches.Count > 0)
        {
            if (allCommentsMatches.Count <= eslintCommentsMatches.Count)
            {
                return false;
            }
            _logs.Add($"Found comments that are not eslint comments in {file.FullName}.");
            hasComments = true;
            foreach (Match match in allCommentsMatches)
            {
                _logs.Add($"Found comment: {match.Value}");
            }
        }
        else
        {
            _logs.Add($"{file.Name} does not contain comments.");
        }

        return hasComments;
    }
}
