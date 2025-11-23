namespace ActivityProvider.Models
{
    public sealed record AnalyticsResponse(string InveniraStdID, ConfigParams[] QualAnalytics, ConfigParams[] QuantAnalytics);
}
