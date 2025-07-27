
namespace Pocco.Svc.Accounts.Helpers;

public static class PasswordHelper {
    public static string Hash(string plain) {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plain);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}