namespace SklepCSManager;

public enum LoggingLevelData
{
    None = 0,
    PurchaseSuccess = 1 << 0,
    PurchaseErrors = 1 << 1,
    WebApiErrors = 1 << 2,

    All = ~None
}