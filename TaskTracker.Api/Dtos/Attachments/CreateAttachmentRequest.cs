namespace TaskTracker.Api.Dtos.Attachments;

public class CreateAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}