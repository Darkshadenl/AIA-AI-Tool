using System.IO.Compression;
using System.Text.RegularExpressions;

namespace aia_api.Application.Helpers;

public class CommentChecker
{
    private readonly ILogger<CommentChecker> _logger;
    private readonly List<string> _logs;
    private ZipArchiveEntry _file;
    private MatchCollection _notInlineComments;
    private MatchCollection _eslintComments;
    private MatchCollection _inlineComments;

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
        _file = zipArchiveEntry;
        switch (fileExtension)
        {
            case ".ts":
                var detectCommentsPattern = @"((?<=\s|^)\/\/[^\n]*|\/\*[\s\S]*?\*\/|\/\*\*[\s\S]*?\*\/)";
                var detectEslintCommentsPattern =
                    @"(\/\/.*eslint-.*|\/\*[\s\S]*?eslint-[\s\S]*?\*\/|\/\*\*[\s\S]*?eslint-[\s\S]*?\*\/)";
                var detectInlineCommentsPattern =
                    @"(?<!\S)\/\/[^\n]*|(?<=\S{1,3}\s{0,2})\/\*[\s\S]*?\*\/|(?<=\S{1,3}\s{0,2})\/\*\*[\s\S]*?\*\/";
                return FileHasComments(detectCommentsPattern, detectEslintCommentsPattern,
                    detectInlineCommentsPattern);
            default:
                _logger.LogDebug("File extension {extension} not supported.", fileExtension);
                return false;
        }
    }

    private bool FileHasComments(string allCommentPattern, string eslintCommentPattern,
        string detectInlineCommentPattern)
    {
        var fileContent = ReadFileContent(_file);
        _notInlineComments = FindMatches(fileContent, allCommentPattern);
        _eslintComments = FindMatches(fileContent, eslintCommentPattern);
        _inlineComments = FindMatches(fileContent, detectInlineCommentPattern);

        return AnalyzeComments();
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

    private bool AnalyzeComments()
    {
        if (_notInlineComments.Count + _inlineComments.Count == 0)
        {
            Log($"{_file.Name} does not contain comments.");
            return false;
        }

        var nonEssentialCommentCount = _eslintComments.Count;

        if (nonEssentialCommentCount == _notInlineComments.Count + _inlineComments.Count)
        {
            Log($"{_file.Name} contains only eslint. Skipping.");
            return false;
        }

        if (IsOnlyTypeOfComment(_eslintComments, "eslint"))
            return false;

        LogComments(_notInlineComments);
        return true;
    }

    private bool IsOnlyTypeOfComment(MatchCollection specificComments, string commentType)
    {
        if (_notInlineComments.Count + _inlineComments.Count <= specificComments.Count)
        {
            Log($"Found only {commentType} comment in {_file.FullName}. Skipping.");
            return true;
        }

        return false;
    }

    private void LogComments(MatchCollection comments)
    {
        Log($"Found comments in {_file.FullName}.");
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
