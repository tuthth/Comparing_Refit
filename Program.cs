using Comparing_Refit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;
using Scalar.AspNetCore;
using System.Diagnostics;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // Add your validation parameters here
                    ValidIssuer = "your_issuer",
                    ValidAudience = "your_audience",
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your_secret_key"))
                };
        });

        builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        builder.Services.AddRefitClient<IApiService>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://api.sampleapis.com/");
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.WithTheme(ScalarTheme.Default).WithTitle("Scalar API Reference").WithDarkModeToggle(true);
            options.WithCdnUrl("https://cdn.jsdelivr.net/npm/@scalar/api-reference");
        });

        app.Map("/", () => Results.Redirect("/scalar/v1"));

        app.MapGet("/hello", () => Results.Ok("Hello Scalar")).WithTags("Hello Scalar").RequireAuthorization().WithOpenApi();

        app.MapGet("/api/colors/httpclient", async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            using var client = new HttpClient { BaseAddress = new Uri("https://api.sampleapis.com") };
            var response = await client.GetFromJsonAsync<List<CSSColorName>>("/csscolornames/colors");
            stopwatch.Stop();

            return Results.Json(new { Time = stopwatch.ElapsedMilliseconds + " ms", Colors = response });
        }).WithTags("Get with HttpClient");

        app.MapGet("/api/colors/refit", async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            var apiService = RestService.For<IApiService>("https://api.sampleapis.com");
            var response = await apiService.GetAll();
            stopwatch.Stop();
            return Results.Json(new { Time = stopwatch.ElapsedMilliseconds + " ms", Colors = response });
        }).WithTags("Get with Refit");

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

internal sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}