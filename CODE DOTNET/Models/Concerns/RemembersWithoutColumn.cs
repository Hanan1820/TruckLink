
using System.Security.Cryptography;
using System.Text;

namespace TruckLink.Models.Concerns;








public interface IRemembersWithoutColumn
{
    string AuthPassword { get; }
    int AuthIdentifier { get; }

    
    string GetRememberToken(string appKey)
    {
        var data = Encoding.UTF8.GetBytes($"{GetType().FullName}|{AuthIdentifier}|{AuthPassword}");
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(appKey));
        return Convert.ToHexString(h.ComputeHash(data)).ToLowerInvariant();
    }

    
    void SetRememberToken(string? value) { }

    
    string GetRememberTokenName() => "";
}
