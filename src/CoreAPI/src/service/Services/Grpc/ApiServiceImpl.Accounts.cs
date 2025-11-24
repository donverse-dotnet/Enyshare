using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Accounts.Types;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  // Account service methods
  private readonly V0AccountService.V0AccountServiceClient _accountServiceClient;

  // Register
  public override async Task<Empty> Register(V0AccountRegisterRequest request, ServerCallContext context) {
    var rrd = new V0RegisterAccountRequest {
      Email = request.Email,
      Password = request.Password
    };

    try {
      var response = await _accountServiceClient.RegisterAsync(rrd, cancellationToken: context.CancellationToken);
    } catch (RpcException rpcEx) { // TODO: 詳細なエラーハンドリング → エラーモデルなどを利用してAPIエラーを返す
      _logger.LogError("RPC Error in Register: {Message}", rpcEx.Message);
      throw;
    } catch (Exception ex) {
      _logger.LogError("Unexpected Error in Register: {Message}", ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
    }

    return new Empty();
  }

  // GetProfile
  public override async Task<V0BaseAccount> GetProfile(V0AccountGetProfileRequest request, ServerCallContext context) {
    var gprd = new V0GetAccountRequest {
      Id = request.UserId
    };

    var response = await _accountServiceClient.GetAsync(gprd, cancellationToken: context.CancellationToken);

    var status = new V0AccountStatus {
      Type = response.Account.Status.Status switch {
        Libs.Protobufs.Accounts.Enums.V0AccountStatus.V0Offline => V0AccountStatusTypes.Offline,
        Libs.Protobufs.Accounts.Enums.V0AccountStatus.V0Online => V0AccountStatusTypes.Online,
        Libs.Protobufs.Accounts.Enums.V0AccountStatus.V0Idle => V0AccountStatusTypes.Idle,
        Libs.Protobufs.Accounts.Enums.V0AccountStatus.V0Meeting => V0AccountStatusTypes.Busy,
        _ => V0AccountStatusTypes.Offline,
      },
      CustomMessage = response.Account.Status.Message
    };
    var profile = new V0BaseAccount {
      Id = response.Account.Id,
      Username = response.Account.Username,
      AvatarUrl = response.Account.AvatarUrl,
      Status = status,
      CreatedAt = response.Account.CreatedAt,
      // TODO: UpdatedAt
    };
    profile.OrganizationIds.AddRange(response.Account.OrganizationIds);

    return profile;
  }
  // UpdateProfile
  public override async Task<V0BaseAccount> UpdateProfile(V0AccountUpdateProfileRequest request, ServerCallContext context) {
    var uprd = new V0UpdateAccountRequest {
      NewAccount = new V0AccountModel {
        Id = request.UserId,
        Username = request.Username,
        AvatarUrl = request.AvatarUrl,
        Notifications = new Libs.Protobufs.Accounts.Types.V0AccountNotificationSettings {
          Email = request.NotificationSettings.Email,
          Push = request.NotificationSettings.Push,
          ShowBadge = request.NotificationSettings.Badge
        }
      }
    };

    var response = await _accountServiceClient.UpdateAsync(uprd, cancellationToken: context.CancellationToken);

    var updatedProfile = new V0BaseAccount {
      Id = response.Account.Id,
      Username = response.Account.Username,
      AvatarUrl = response.Account.AvatarUrl,
    };

    return updatedProfile;
  }
  // DeleteAccount
  public override async Task<Empty> DeleteAccount(Empty request, ServerCallContext context) {
    var userId = context.GetHttpContext().Request.Headers["x-account-id"].ToString();
    if (string.IsNullOrEmpty(userId)) {
      _logger.LogWarning("DeleteAccount called without x-account-id header");
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing account ID"));
    }

    var dard = new V0DeleteAccountRequest {
      Id = userId
    };

    try {
      var response = await _accountServiceClient.DeleteAsync(dard, cancellationToken: context.CancellationToken);
    } catch (RpcException rpcEx) { // TODO: 詳細なエラーハンドリング → エラーモデルなどを利用してAPIエラーを返す
      _logger.LogError("RPC Error in DeleteAccount: {Message}", rpcEx.Message);
      throw;
    } catch (Exception ex) {
      _logger.LogError("Unexpected Error in DeleteAccount: {Message}", ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
    }

    return new Empty();
  }
}
