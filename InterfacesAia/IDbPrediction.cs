using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterfacesAia;

public interface IDbPrediction
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    int Id { get; }

    string FileName { get; set; }
    string FileExtension { get; set; }
    string Prompt { get; set; }
    string? PredictionResponseText { get; set; }
}
