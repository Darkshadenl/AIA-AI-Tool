using aia_api.Application.Replicate;

namespace aia_api.Routes.DTO;

public class ReplicateResultDTO
{
    public string id { get; set; }
    public string version { get; set; }
    public PredictionInputDTO InputDto { get; set; }
    public string logs { get; set; }
    public string[] output { get; set; }

    public string error { get; set; }
    public string status { get; set; }
    public string created_at { get; set; }
    public string started_at { get; set; }
    public string completed_at { get; set; }
    public Metrics metrics { get; set; }
}

public class Metrics
{
    public double PredictTime { get; set; }
}
