using System.Text;
using System.Text.RegularExpressions;

namespace aia_api.Application.Helpers;

public class CommentManipulationHelper
{
    private readonly ILogger<CommentManipulationHelper> _logger;

    public CommentManipulationHelper(ILogger<CommentManipulationHelper> logger)
    {
        _logger = logger;
    }

    public string CombineTokens(string[] tokens)
    {
        var stringBuilder = new StringBuilder();
        foreach (string token in tokens)
            stringBuilder.Append(token);
        return stringBuilder.ToString().Trim();
    }

    public string ReplaceCommentInCode(string generatedComment, string inputCode)
    {
        //Remove old comments
        inputCode = Regex.Replace(inputCode, @"\n((?<=\s|^)\/\/[^\n]*|\/\*[\s\S]*?\*\/|\/\*\*[\s\S]*?\*\/)", "").Trim();
        
        //Insert new comments
        MatchCollection commentMatches = GetComments(generatedComment, @"\[RETURN\](.*?)\[\/RETURN\]");
        List<string> contentLines = InsertCommentsIntoCode(commentMatches, inputCode);
        
        return string.Join(Environment.NewLine, contentLines);
    }

    private List<string> InsertCommentsIntoCode(MatchCollection commentMatches, string inputCode)
    {
        List<string> contentLines = inputCode.Split(Environment.NewLine, StringSplitOptions.TrimEntries).ToList();

        foreach (Match match in commentMatches)
        {
            string comment = match.Groups[1].Value.Trim();
            List<string> commentLines = comment.Split(Environment.NewLine, StringSplitOptions.TrimEntries).ToList();
            
            int insertIndex = GetMethodLineIndex(commentLines, contentLines);
            if (insertIndex < 0) return contentLines;
            commentLines.Remove(commentLines.Last());
            contentLines.InsertRange(insertIndex, commentLines);
        }

        return contentLines;
    }

    /// <summary>
    /// Gets the index of the line that contains the method signature.
    /// </summary>
    /// <param name="commentLines">List of lines of the newly generated comment by the LLM</param>
    /// <param name="contentLines">List of lines of the original input code</param>
    /// <returns>The index of the line that contains the method signature</returns>
    private int GetMethodLineIndex(List<string> commentLines, List<string> contentLines)
    {
        int index = contentLines.IndexOf(commentLines.Last());
        if (index < 0)
        {
            _logger.LogDebug("Could not find the location to place the comment in. Comment: {comment}, Code: {code}", 
                string.Join("", commentLines), string.Join("", contentLines));
        }

        return index;
    }

    private MatchCollection GetComments(string content, string pattern)
    {
        return Regex.Matches(content, pattern, RegexOptions.Singleline);
    }
}