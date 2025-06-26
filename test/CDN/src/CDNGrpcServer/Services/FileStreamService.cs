using Grpc.Core;

using Pocco.CDN.Protos;

namespace Pocco.CDN.Services;

public class FileStreamService : FileGrpcStream.FileGrpcStreamBase {
  private readonly ILogger<FileStreamService> _logger;
  private readonly string _cdnRootPath;
  private readonly string[] _allowedFileExtensions = [
    ".png",
    ".jpg",
    ".jpeg",
    ".gif",
    ".webp",
    ".mp4",
    ".mp3",
    ".wav",
    ".ogg",
    ".webm"
  ];

  public FileStreamService(ILogger<FileStreamService> logger) {
    _logger = logger;
    _cdnRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cdn");

    if (Directory.Exists(_cdnRootPath) is false) {
      Directory.CreateDirectory(_cdnRootPath);
    }
  }

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

    // 許可されていないファイル拡張子はアップロードできない
    if (_allowedFileExtensions.Contains(Path.GetExtension(file_name)) is false) {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid file extension"));
    }

    // 5MB 以上のファイルはアップロードできない
    if (fileData.Count > 5 * 1024 * 1024) {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "File size exceeds 5MB"));
    }

    // ファイル名が空の場合はアップロードできない
    if (string.IsNullOrEmpty(file_name)) {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "File name is empty"));
    }

    fileInfo.FileName = file_name;

    var file_path = Path.Combine(_cdnRootPath, file_name);
    await File.WriteAllBytesAsync(file_path, fileData.ToArray());

    return fileInfo;
  }

  public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context) {
    var file_name = request.FileName;
    var file_path = Path.Combine(_cdnRootPath, file_name);

    if (File.Exists(file_path) is false) {
      throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
    }

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    using var fs = File.OpenRead(file_path);
    bool isFirstRead = true;
    int bytesRead;

    while ((bytesRead = await fs.ReadAsync(buffer, 0, bufferSize)) > 0) {
      var chunk = new DownloadFileResponse {
        FileName = isFirstRead ? file_name : "",
        FileData = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
      };

      await responseStream.WriteAsync(chunk);
      isFirstRead = false;
    }
  }

  public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context) {
    var file_name = request.FileName;
    var file_path = Path.Combine(_cdnRootPath, file_name);

    if (File.Exists(file_path) is false) {
      throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
    }

    File.Delete(file_path);

    return new DeleteFileResponse { FileName = file_name };
  }
}
