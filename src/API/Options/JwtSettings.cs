using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Options;

public class JwtSettings
{
    public const string Section = "JWT";
    public TimeSpan Expires { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;

    public SymmetricSecurityKey GetKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
}