namespace WesNews.Infrastructure.Configuration;

public class DigestEmailOptions
{
    public string FromEmail { get; init; } = string.Empty;
    public string ToEmail { get; init; } = string.Empty;
    public string CronExpression { get; init; } = "0 0 7 * * ?";
}
