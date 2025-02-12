namespace API.Options;

public class CookieSettings
{
    public const string Section = "COOKIE";
    
    public TimeSpan Expires { get; set; }
}