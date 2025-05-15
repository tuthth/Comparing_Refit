using Refit;
using System.Diagnostics;

namespace Comparing_Refit.ApiMappings
{
    public class ApiClientMapping : IEndpoint
    {
        public void MapApiEndpoints(IEndpointRouteBuilder app)
        {
            app.Map("/", () => Results.Redirect("/scalar/v1")).WithDisplayName("Return home");
            var group = app.MapGroup("/api").WithTags("ApiClient").WithOpenApi();

            group.MapGet("/hello", () => Results.Ok("Hello Scalar"))
                .WithSummary("Hello Scalar")
                .RequireAuthorization();

            group.MapGet("/colors/httpclient", async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                using var client = new HttpClient { BaseAddress = new Uri("https://api.sampleapis.com") };
                var response = await client.GetFromJsonAsync<List<CSSColorName>>("/csscolornames/colors");

                stopwatch.Stop();

                return Results.Json(new { Time = stopwatch.ElapsedMilliseconds + " ms", Colors = response });
            }).WithSummary("Get with HttpClient");

            group.MapGet("/colors/refit", async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var apiService = RestService.For<IApiService>("https://api.sampleapis.com");
                var response = await apiService.GetAll();

                stopwatch.Stop();

                return Results.Json(new { Time = stopwatch.ElapsedMilliseconds + " ms", Colors = response });
            }).WithSummary("Get with Refit");
        }
    }
}
