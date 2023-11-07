using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace aia_api.Application.Helpers;

public class FindFileDifferenceHelper
{
    private readonly InlineDiffBuilder _inlineDiffBuilder;
    
    public FindFileDifferenceHelper()
    {
        var differ = new Differ();
        _inlineDiffBuilder = new InlineDiffBuilder(differ);
    }

    public void FindDifferences(string oldCode, string newCode)
    {
        var diffResult = _inlineDiffBuilder.BuildDiffModel(oldCode, newCode);

        foreach (var line in diffResult.Lines)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    Console.WriteLine("+ " + line.Text); // Nieuwe code
                    break;
                case ChangeType.Deleted:
                    Console.WriteLine("- " + line.Text); // Verwijderde code
                    break;
                case ChangeType.Modified:
                    Console.WriteLine("* " + line.Text); // Gewijzigde code
                    break;
                case ChangeType.Unchanged:
                    Console.WriteLine("  " + line.Text); // Ongewijzigde code
                    break;
            }
        }
    }
}