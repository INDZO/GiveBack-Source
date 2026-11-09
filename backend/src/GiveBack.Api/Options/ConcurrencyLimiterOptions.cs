namespace GiveBack.Api.Options;

public sealed class ConcurrencyLimiterOptions
{
    public int GlobalLimit { get; set; } = 200;
    public int QueueTimeoutSeconds { get; set; } = 10;
    public int RetryAfterSeconds { get; set; } = 5;
    public List<EndpointLimitOptions> EndpointLimits { get; set; } = new();
}

public sealed class EndpointLimitOptions
{
    public string Method { get; set; } = string.Empty;
    public string PathPattern { get; set; } = string.Empty;
    public int Limit { get; set; }
}
