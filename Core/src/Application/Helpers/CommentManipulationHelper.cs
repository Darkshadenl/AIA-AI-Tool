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

    public string ReplaceCommentInCode(string generatedComment, string inputCode)
    {
        MatchCollection commentMatches = GetComments(generatedComment, @"\[RETURN\](.*?)\[\/RETURN\]");

        if (commentMatches.Count <= 0)
        {
            _logger.LogInformation("No blocks of [RETURN][/RETURN] found in the LLM response, skipping comment replacement for file.");
            return inputCode;
        }

        string codeWithoutComments = RemoveExisingComments(commentMatches, inputCode);
        List<string> updatedCodeLines = InsertCommentsIntoCode(commentMatches, codeWithoutComments);

        return string.Join(Environment.NewLine, updatedCodeLines);
    }

    private string RemoveExisingComments(MatchCollection commentMatches, string inputCode)
    {
        foreach (Match match in commentMatches)
        {
            string comment = match.Groups[1].Value.Trim(Environment.NewLine.ToCharArray());
            List<string> commentLines = comment.Split(Environment.NewLine).ToList();
            string methodSignature = commentLines.Last();

            string specificCommentPattern = 
                $@"[^\S\r\n]*((?<=\s|^)\/\/[^\n]*|\/\*(?:(?!\*\/)[\s\S])*\*\/|\/\*\*(?:(?!\*\/)[\s\S])*\*\/)\n*\s*(?:@.*\n*\W*)*{Regex.Escape(methodSignature)}";
            inputCode = Regex.Replace(inputCode, specificCommentPattern, methodSignature);
        }

        return inputCode;
    }

    private List<string> InsertCommentsIntoCode(MatchCollection commentMatches, string inputCode)
    {
        List<string> inputCodeLines = inputCode.Split(Environment.NewLine).ToList();
        
        foreach (Match match in commentMatches)
        {
            string comment = match.Groups[1].Value.Trim(Environment.NewLine.ToCharArray());
            Console.WriteLine(comment);
            List<string> commentLines = comment.Split(Environment.NewLine).ToList();
            
            int insertIndex = GetMethodLineIndex(commentLines, inputCodeLines);
            if (insertIndex < 0) return inputCodeLines;
            commentLines.Remove(commentLines.Last()); 
            inputCodeLines.InsertRange(insertIndex, commentLines);
        }

        return inputCodeLines;
    }

    /// <summary>
    /// Gets the index of the line that contains the method signature.
    /// </summary>
    /// <param name="commentLines">List of lines of the newly generated comment by the LLM</param>
    /// <param name="inputCodeLines">List of lines of the original input code</param>
    /// <returns>The index of the line that contains the method signature</returns>
    private int GetMethodLineIndex(List<string> commentLines, List<string> inputCodeLines)
    {
        int index = inputCodeLines.FindIndex(line => line.Trim() == commentLines.Last().Trim());
        if (index < 0)
        {
            _logger.LogDebug("Could not find the location to place the comment in. Comment: {comment}, Code: {code}", 
                string.Join("", commentLines), string.Join("", inputCodeLines));
        }

        return index;
    }

    private MatchCollection GetComments(string content, string pattern)
    {
        return Regex.Matches(content, pattern, RegexOptions.Singleline);
    }
}