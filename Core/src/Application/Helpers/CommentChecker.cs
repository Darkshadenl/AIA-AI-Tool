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
                var detectInlineCommentsPattern =
                    @"(?<=\S{1,3}\s{0,2})\/\/[^\n]*|(?<=\S{1,3}\s{0,2})\/\*[\s\S]*?\*\/|(?<=\S{1,3}\s{0,2})\/\*\*[\s\S]*?\*\/";
                return FileHasComments(zipArchiveEntry, detectCommentsPattern, detectEslintCommentsPattern,
                    detectInlineCommentsPattern);
            default:
                throw new ArgumentException("File extension not supported.");
        }
    }

    private bool FileHasComments(ZipArchiveEntry file, string allCommentPattern, string eslintCommentPattern,
        string detectInlineCommentPattern)
    {
        var fileContent = ReadFileContent(file);
        var allComments = FindMatches(fileContent, allCommentPattern);
        var eslintComments = FindMatches(fileContent, eslintCommentPattern);
        var inlineComments = FindMatches(fileContent, detectInlineCommentPattern);

        return AnalyzeComments(file, allComments, eslintComments, inlineComments);
    }

    private string ReadFileContent(ZipArchiveEntry file)
    {
        using var reader = new StreamReader(file.Open());
        return reader.ReadToEnd();
    }

    private MatchCollection FindMatches(string text, string pattern)
    {
        return Regex.Matches(text, pattern, RegexOptions.Multiline);
    }

    private bool AnalyzeComments(ZipArchiveEntry file, MatchCollection allComments, MatchCollection eslintComments,
        MatchCollection inlineComments)
    {
        if (allComments.Count == 0)
        {
            Log($"{file.Name} does not contain comments.");
            return false;
        }

        var nonEssentialCommentCount = eslintComments.Count + inlineComments.Count;

        if (nonEssentialCommentCount == allComments.Count)
        {
            Log($"{file.Name} contains only eslint or inline comments. Skipping.");
            return false;
        }

        if (IsOnlyTypeOfComment(allComments, eslintComments, file, "eslint"))
            return false;

        if (IsOnlyTypeOfComment(allComments, inlineComments, file, "inline"))
            return false;

        LogComments(file, allComments);
        return true;
    }

    private bool IsOnlyTypeOfComment(MatchCollection allComments, MatchCollection specificComments,
        ZipArchiveEntry file, string commentType)
    {
        if (allComments.Count <= specificComments.Count)
        {
            Log($"Found only {commentType} comment in {file.FullName}. Skipping.");
            return true;
        }

        return false;
    }

    private void LogComments(ZipArchiveEntry file, MatchCollection comments)
    {
        Log($"Found comments in {file.FullName}.");
        for (var index = 0; index < comments.Count; index++)
        {
            var match = comments[index];
            Log($"Comment {index}: {match.Value}");
        }
    }

    private void Log(string message)
    {
        _logs.Add(message);
    }
}
