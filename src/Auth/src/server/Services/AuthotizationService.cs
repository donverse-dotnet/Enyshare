using Grpc.Core;

public class AuthorizationService : AuthService.AuthServiceBase
{
    public override Task<AuthResponse> Authorize(AuthRequest request, ServerCallContext context)
    {
        // 仮の認可ロジック
        // 例: トークンが有効であり、リソースとアクションが一致する場合認可する
        bool isAuthorized = ValidateToken(request.Token) && CheckPermission(request.Resource, request.Action);

        return Task.FromResult(new AuthResponse
        {
            Authorized = isAuthorized,
            ErrorMessage = isAuthorized ? null : "Access denied: insufficient permissions or invalid token"
        });
    }

    // ヘルパーメソッド: トークンの検証
    private bool ValidateToken(string token)
    {
        // トークンがデータベースに存在し、まだ有効かを確認
        Console.WriteLine($"Validating token: {token}");
        return !string.IsNullOrEmpty(token) && token.Length > 10; // 仮条件
    }

    // ヘルパーメソッド: アクセス権限の確認
    private bool CheckPermission(string resource, string action)
    {
        // 仮のリソースとアクションのチェック
        Console.WriteLine($"Checking permissions for resource: {resource}, action: {action}");
        return resource == "exampleResource" && action == "read";
    }
}
