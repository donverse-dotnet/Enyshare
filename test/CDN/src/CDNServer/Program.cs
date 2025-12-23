#pragma warning disable IDE0130
namespace Pocco.CDN;

public class Server {
  public static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCors(options => {
      options.AddPolicy("AllowUIRenderer", policy => {
        policy.WithOrigins("http://localhost:5099")
              .AllowAnyHeader()
              .AllowAnyMethod();
      });
    });

    builder.Services.AddOpenApi();
    builder.Services.AddControllers();

    var app = builder.Build();

    app.UseCors("AllowUIRenderer");

    if (app.Environment.IsDevelopment()) {
      app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.Run();
  }
}
