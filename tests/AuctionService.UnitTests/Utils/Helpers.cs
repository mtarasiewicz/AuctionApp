using System.Security.Claims;

namespace AuctionService.UnitTests;

public class Helpers
{
    public const string Username = "test";
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        var claims = new List<Claim>{
            new Claim(ClaimTypes.Name, Username)
        };
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
