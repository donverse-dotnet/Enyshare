#pragma warning disable CS1998, IDE0130
using Microsoft.AspNetCore.Mvc;

namespace Pocco.CDN.Controllers;

[ApiController]
public class CDNController : ControllerBase {
  private readonly ILogger<CDNController> _logger;
  private readonly string _rootPath;

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

  public CDNController(ILogger<CDNController> logger) {
    _logger = logger;
    _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cdn");

    if (Directory.Exists(_rootPath) is false) {
      Directory.CreateDirectory(_rootPath);
    }
  }

  [HttpPost("{user_id}")]
  public async Task<IActionResult> UploadFile(string user_id, IFormFile file) {
    if (file is null || file.Length is 0) {
      _logger.LogWarning("No file uploaded");
      return BadRequest("No file uploaded");
    }

    if (Directory.Exists(Path.Combine(_rootPath, user_id)) is false) {
      Directory.CreateDirectory(Path.Combine(_rootPath, user_id));
    }

    var fileExtension = Path.GetExtension(file.FileName);

    if (file.FileName.Contains("..") || file.FileName.Contains('/') || file.FileName.Contains('\\')) {
      _logger.LogWarning("Invalid file name");
      return BadRequest("Invalid file name");
    }
    if (file.FileName.Length > 255) {
      _logger.LogWarning("File name is too long");
      return BadRequest("File name is too long");
    }
    if (file.FileName.Length < 3) {
      _logger.LogInformation("File name is too short");
      return BadRequest("File name is too short");
    }
    if (file.FileName.Contains(' ')) {
      _logger.LogWarning("File name contains spaces");
      return BadRequest("File name contains spaces");
    }
    if (fileExtension is null || _allowedFileExtensions.Contains(fileExtension.ToLower()) is false) {
      _logger.LogWarning("Invalid file extension");
      return BadRequest("Invalid file extension");
    }

    var filePath = Path.Combine(_rootPath, user_id, $"{Guid.NewGuid()}{fileExtension}");

    using (var stream = new FileStream(filePath, FileMode.Create)) {
      await file.CopyToAsync(stream);
    }

    _logger.LogInformation("File uploaded successfully to {filePath}", filePath);
    return Ok(new { filePath });
  }

  [HttpGet("{user_id}/{file_name}")]
  public async Task<IActionResult> DownloadFile(string user_id, string file_name) {
    var filePath = Path.Combine(_rootPath, user_id, file_name);

    if (!System.IO.File.Exists(filePath)) {
      _logger.LogWarning("File not found at {filePath}", filePath);
      return NotFound("File not found");
    }

    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    _logger.LogInformation("File downloaded successfully from {filePath}", filePath);
    return File(fileStream, "application/octet-stream", file_name);
  }

  [HttpDelete("{user_id}/{file_name}")]
  public IActionResult DeleteFile(string user_id, string file_name) {
    var filePath = Path.Combine(_rootPath, user_id, file_name);

    if (!System.IO.File.Exists(filePath)) {
      _logger.LogWarning("File not found at {filePath}", filePath);
      return NotFound("File not found");
    }

    System.IO.File.Delete(filePath);
    _logger.LogInformation("File deleted successfully from {filePath}", filePath);
    return Ok("File deleted successfully");
  }
}
