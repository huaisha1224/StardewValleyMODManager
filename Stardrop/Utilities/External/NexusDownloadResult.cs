namespace Stardrop.Utilities.External
{
    public enum DownloadResultKind
    {
        Failed,
        UserCanceled,
        Success
    }

    public record struct NexusDownloadResult(DownloadResultKind ResultKind, string? DownloadedModFilePath);
}
