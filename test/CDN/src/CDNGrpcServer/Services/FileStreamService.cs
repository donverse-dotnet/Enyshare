using Grpc.Core;

using Pocco.CDN.Protos;

namespace Pocco.CDN.Services;

public class FileStreamService : FileGrpcStream.FileGrpcStreamBase {
  public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context) {
    var fileInfo = new UploadFileResponse();
    var fileData = new List<byte>();

    var file_name = string.Empty;
    var isFirstRead = true;

    await foreach (var request in requestStream.ReadAllAsync()) {
      if (isFirstRead) {
        file_name = request.FileName;
        isFirstRead = false;
      }

      fileData.AddRange(request.FileData);
    }

    fileInfo.FileName = file_name;

    return fileInfo;
  }

  public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context) {
    var file_name = request.FileName;
    var file_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cdn", file_name);

    if (File.Exists(file_path) is false) {
      throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
    }

    var file_data = await File.ReadAllBytesAsync(file_path);

    await responseStream.WriteAsync(new DownloadFileResponse { FileData = Google.Protobuf.ByteString.CopyFrom(file_data) });
  }

  public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context) {
    var file_name = request.FileName;
    var file_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cdn", file_name);

    if (File.Exists(file_path) is false) {
      throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
    }

    File.Delete(file_path);

    return new DeleteFileResponse { FileName = file_name };
  }
}
