namespace DxSignOut.Utils;

internal record HistoryData(DateTime DateTime, SignOutStatus Status);

internal enum SignOutStatus
{
    Succeeded,
    MayFailed,
    Failed
}