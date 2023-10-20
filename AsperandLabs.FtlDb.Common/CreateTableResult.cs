namespace AsperandLabs.FtlDb.Common;

public class CreateTableResult
{
    public List<string> Errors { get; set; } = new List<string>();
    public bool IsSuccessful => !Errors.Any();
}