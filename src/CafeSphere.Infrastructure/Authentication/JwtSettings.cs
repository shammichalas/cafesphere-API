namespace CafeSphere.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = "CafeSphere_Super_Secret_Key_For_JWT_Authentication_2026_Production!";
    public string Issuer { get; set; } = "CafeSphereAPI";
    public string Audience { get; set; } = "CafeSphereApp";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
