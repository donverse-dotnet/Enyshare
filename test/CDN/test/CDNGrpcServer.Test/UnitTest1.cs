using Google.Protobuf;

using Grpc.Core;
using Grpc.Net.Client;

using Pocco.CDN.Protos;

namespace CDNGrpcServer.Test;

public class UnitTest1 {
  [Fact]
  public async Task FileUploadTest() {
    using var channel = GrpcChannel.ForAddress("http://localhost:5261");
    var client = new FileGrpcStream.FileGrpcStreamClient(channel);
    using var call = client.UploadFile();

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    var filename = "test.txt";
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
  }

  [Fact]
  public async Task FileDownloadTest() {
    using var channel = GrpcChannel.ForAddress("http://localhost:5261");
    var client = new FileGrpcStream.FileGrpcStreamClient(channel);

    using var call = client.DownloadFile(new DownloadFileRequest {
      FileName = "test.txt"
    });
    using var fs = File.OpenWrite("test_downloaded.txt");

    await foreach (var chunk in call.ResponseStream.ReadAllAsync()) {
      await fs.WriteAsync(chunk.FileData.ToByteArray());
    }

    Console.WriteLine("File downloaded");
    fs.Close();
  }
}
