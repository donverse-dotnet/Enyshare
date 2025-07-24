using Grpc.Core;
using auth;
using System;

public class AuthenticationService : AuthService.AuthServiceBase
{
    // サインイン処理
    public override Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        // 仮の認証ロジック
        bool isAuthenticated = (request.Email == "test@example.com" && request.Password == "password123");
        string token = isAuthenticated ? Guid.NewGuid().ToString() : null;

        // トークンを保存する処理を追加（例: データベースなど）
        if (isAuthenticated)
        {
            SaveTokenToDatabase(request.Email, token);
        }

        return Task.FromResult(new SignInResponse
        {
            Success = isAuthenticated,
            Token = token,
            ErrorMessage = isAuthenticated ? null : "Invalid email or password"
        });
    }

    // サインアウト処理
    public override Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context)
    {
        // トークンの無効化ロジック
        bool isTokenValid = RevokeTokenInDatabase(request.Token);

        return Task.FromResult(new SignOutResponse
        {
            Success = isTokenValid,
            ErrorMessage = isTokenValid ? null : "Invalid token"
        });
    }

    // アカウントの更新処理
    public override Task<UpdateAccountResponse> UpdateAccount(UpdateAccountRequest request, ServerCallContext context)
    {
        // 仮のアカウント更新ロジック
        bool isUpdated = UpdateUserDetailsInDatabase(request.Token, request.Email, request.Password);

        return Task.FromResult(new UpdateAccountResponse
        {
            Success = isUpdated,
            ErrorMessage = isUpdated ? null : "Failed to update account"
        });
    }

    // セッション保持処理
    public override Task<HoldSessionResponse> HoldSession(HoldSessionRequest request, ServerCallContext context)
    {
        // 仮のセッション更新ロジック
        bool isHeld = ExtendSessionInDatabase(request.Token, request.Duration);

        return Task.FromResult(new HoldSessionResponse
        {
            Success = isHeld,
            ErrorMessage = isHeld ? null : "Failed to hold session"
        });
    }

    // ヘルパーメソッドの例
    private void SaveTokenToDatabase(string email, string token)
    {
        // トークン保存処理（仮実装）
        Console.WriteLine($"Token saved for {email}: {token}");
    }

    private bool RevokeTokenInDatabase(string token)
    {
        // トークン無効化処理（仮実装）
        return !string.IsNullOrEmpty(token);
    }

    private bool UpdateUserDetailsInDatabase(string token, string email, string password)
    {
        // ユーザー情報更新処理（仮実装）
        Console.WriteLine($"Updating user details. Token: {token}, Email: {email}, Password: {password}");
        return true;
    }

    private bool ExtendSessionInDatabase(string token, int duration)
    {
        // セッション拡張処理（仮実装）
        Console.WriteLine($"Extending session for Token: {token}, Duration: {duration} seconds");
        return true;
    }
}
