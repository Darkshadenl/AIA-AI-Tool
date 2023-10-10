namespace aia_api.Routes.DTO;

public class ReplicateResultDTO
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string CreatedAt { get; set; }
    public string StartedAt { get; set; }
    public string CompletedAt { get; set; }
    public string Status { get; set; }
    public ReplicateInput ReplicateInput { get; set; }
    public object Output { get; set; }
    public object Error { get; set; }
    public object Logs { get; set; }
    public object Metrics { get; set; }
}

public class ReplicateInput
{
    public string Text { get; set; }
}
