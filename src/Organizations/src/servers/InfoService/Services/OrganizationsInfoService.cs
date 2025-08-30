using Grpc.Core;

using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

using Pocco.Libs.Protobufs.Services;

using System.ComponentModel;
using System.Threading.Tasks;

namespace InfoService.Services;

public class OrganizationsInfoServiceImpl : V0OrganizationInfoService.V0OrganizationInfoServiceBase {
    private readonly IOrganizationRepository _repository;

    public OrganizationsInfoServiceImpl(IOrganizationRepository repository) {
        _repository = repository;
    }

    public override async Task<CreateOrganizationReply> Create(CreateOrganizationRequest request, ServerCallContext context) {
        var entity = new OrganizationEntity {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = null
        };

        var saved = await _repository.CreateAsync(entity);

        return new CreateOrganizationReply {
            Organization = new VOInfoModel {

                Id = saved.Id,
                Name = saved.Name,
                Description = saved.Description,
                CreatedBy = saved.CreatedBy,
                CreatedAt = Timestamp.FromDateTime(saved.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(saved.UpdatedAt.ToUniversalTime()),
                DeletedAt = saved.DeletedAt.HasValue
                    ? Timestamp.FromDateTime(saved.DeletedAt.Value.ToUniversalTime())
                    : null
            }
        };
    }

    public override async Task<DeleteOrganizationReply> Update(DeleteOrganizationRequest request, ServerCallContext context) {
        var success = await _repository.LogicalDeleteAsync(request.Id);

        return new DeleteOrganizationReply {
            Success = success,
            Message = success ? "Deleted successfully." : "Organization not found or already deleted."
        };
    }

    public override async Task<Empty> CreateFromModel(VOInfoModel model, ServerCallContext context) {
        var entity = new OrganizationEntity {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            CreatedBy = model.CreatedBy,
            CreatedAt = model.CreatedAt.ToDateTime(),
            UpdatedAt = model.UpdatedAt.ToDateTime(),
            DeletedAt = model.DeletedAt?.ToDateTime()
        };

        await _repository.CreateAsync(entity);
        return new Empty();
    }

    public override async Task<VOInfoModel> GetInfo(GetOrganizationInfoRequest request, ServerCallContext context) {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));

        return new VOInfoModel {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedBy = entity.CreatedBy,
            CreatedAt = Timestamp.FromDateTime(entity.CreatedAt.ToUniversalTime()),
            UpdatedAt = Timestamp.FromDateTime(entity.UpdatedAt.ToUniversalTime()),
            DeletedAt = entity.DeletedAt.HasValue
                ? Timestamp.FromDateTime(entity.DeletedAt.Value.ToUniversalTime())
                : null
        };
    }
}
