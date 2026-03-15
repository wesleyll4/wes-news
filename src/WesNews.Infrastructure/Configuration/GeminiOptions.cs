namespace WesNews.Infrastructure.Configuration;

public class GeminiOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-2.5-flash";
    public int CandidateLookbackHours { get; init; } = 48;
    public int CandidateLimit { get; init; } = 30;
    public int TopPicksPerCategory { get; init; } = 3;
    public int DelayBetweenCategoriesSeconds { get; init; } = 15;
    public string MorningCron { get; init; } = "0 0 6 * * ?";
    public string AfternoonCron { get; init; } = "0 0 14 * * ?";
}
