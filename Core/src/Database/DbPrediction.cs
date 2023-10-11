using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aia_api.Database;

public class DbPrediction
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; }

    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public string Prompt { get; set; }
    public string? PredictionResponseText { get; set; }
}
