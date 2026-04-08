
public class JwtOptions
{
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpireMinutes { get; init; } = 30;
    // Default set to no tolarance for safety. But fetches from appsettings.sjon
    public int ClockSkewSeconds {get; set;} = 0;
}