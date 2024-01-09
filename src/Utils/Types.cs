namespace DxSignOut.Utils;

internal record HistoryData(DateTime DateTime, SignOutStatus Status, TimeSpan SpendTime);

internal enum SignOutStatus
{
    Succeeded,
    MayFailed,
    Failed
}