using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public record JwtDescriptorConfig
{
    public TimeSpan? Expires { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string SigningKey { get; set; } = null!;

    public SecurityKey GetKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
    }
}