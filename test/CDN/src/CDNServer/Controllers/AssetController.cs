#pragma warning disable CS1998, IDE0130
using Microsoft.AspNetCore.Mvc;

namespace Pocco.CDN.Controllers;

[ApiController, Route("sys-content")]
public class AssetController : ControllerBase {
  private readonly ILogger<AssetController> _logger;
  private readonly string _rootPath;

  public AssetController(ILogger<AssetController> logger) {
    _logger = logger;
    _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cdn", "assets");

    if (Directory.Exists(_rootPath) is false) {
      Directory.CreateDirectory(_rootPath);
    }

    _logger.LogInformation("AssetController initialized");
  }

  [HttpGet("{**file_name}")]
  public async Task<IActionResult> DownloadFile(string file_name) {
    var fileString = file_name.Replace("%2F", "/");
    var filePath = Path.Combine(_rootPath, fileString);

    if (!System.IO.File.Exists(filePath)) {
      _logger.LogWarning("File not found at {filePath}", filePath);
      return NotFound("File not found");
    }

    // Get file extension
    var fileExtension = Path.GetExtension(filePath);

    // Set content type based on file extension
    string contentType = fileExtension.ToLower() switch {
      ".js" => "application/javascript",
      ".css" => "text/css",
      ".html" => "text/html",
      ".png" => "image/png",
      ".jpg" => "image/jpeg",
      ".jpeg" => "image/jpeg",
      ".gif" => "image/gif",
      ".webp" => "image/webp",
      ".mp4" => "video/mp4",
      ".mp3" => "audio/mpeg",
      ".wav" => "audio/wav",
      ".ogg" => "audio/ogg",
      ".webm" => "video/webm",
      _ => "application/octet-stream",
    };

    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    _logger.LogInformation("File downloaded successfully from {filePath}", fileString);
    return File(fileStream, contentType, fileString);
  }
}
