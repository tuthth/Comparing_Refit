using nClam;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Comparing_Refit.ApiMappings
{
    public class ClamAVMapping : IEndpoint
    {
        private static ClamClient? _clamClient;

        public static void Initialize(IConfiguration configuration)
        {
            var clamAVHost = configuration["ClamAV:Host"];
            var clamAVPort = int.Parse(configuration["ClamAV:Port"]);
            _clamClient = new ClamClient(clamAVHost, clamAVPort);
        }

        public void MapApiEndpoints(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/clamav").WithTags("ClamAV").WithOpenApi().DisableAntiforgery();

            group.MapPost("/scan/file", async (IFormFile file) =>
            {
                if (_clamClient == null)
                    return Results.Problem("ClamAV client not initialized.");

                if (file == null || file.Length == 0)
                    return Results.BadRequest("No file uploaded.");

                using var stream = file.OpenReadStream();
                var result = await _clamClient.SendAndScanFileAsync(stream);

                return Results.Json(new
                {
                    Result = result.Result.ToString(),
                    InfectedFiles = result.InfectedFiles,
                    RawResult = result.RawResult
                });
            }).WithSummary("Scan uploaded file with ClamAV");

            group.MapPost("/scan/html", async (string htmlContent) =>
            {
                if (_clamClient == null)
                    return Results.Problem("ClamAV client not initialized.");

                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
                var result = await _clamClient.SendAndScanFileAsync(memoryStream);

                return Results.Json(new
                {
                    Result = result.Result.ToString(),
                    InfectedFiles = result.InfectedFiles,
                    RawResult = result.RawResult
                });
            }).WithSummary("Scan HTML content with ClamAV");
        }
    }
}