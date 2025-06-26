using Google.Protobuf;

using Grpc.Core;
using Grpc.Net.Client;

using Pocco.CDN.Protos;

namespace CDNGrpcServer.Test;

public class UnitTest1 {
  private static readonly GrpcChannel _channel = GrpcChannel.ForAddress("http://localhost:5261");
  private static readonly FileGrpcStream.FileGrpcStreamClient _client = new FileGrpcStream.FileGrpcStreamClient(_channel);

  [Fact]
  public async Task FileUploadTest() {
    using var call = _client.UploadFile();

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    var filename = "test.png";
    using var fs = File.OpenRead(filename);
    bool isFirstRead = true;
    int bytesRead;

    while ((bytesRead = await fs.ReadAsync(buffer, 0, bufferSize)) > 0) {
      var chunk = new UploadFileRequest {
        FileName = isFirstRead ? filename : "",
        FileData = ByteString.CopyFrom(buffer, 0, bytesRead)
      };

      await call.RequestStream.WriteAsync(chunk);
      isFirstRead = false;
    }

    await call.RequestStream.CompleteAsync();
    var response = await call.ResponseAsync;

    Console.WriteLine($"File uploaded: {response.FileName}");
    fs.Close();
    call.Dispose();
  }

  [Fact]
  public async Task FileDownloadTest() {
    using var call = _client.DownloadFile(new DownloadFileRequest {
      FileName = "test.png"
    });
    using var fs = File.OpenWrite("test_downloaded.png");

    await foreach (var chunk in call.ResponseStream.ReadAllAsync()) {
      await fs.WriteAsync(chunk.FileData.ToByteArray());
    }

    Console.WriteLine("File downloaded");
    fs.Close();
    call.Dispose();
  }
}
