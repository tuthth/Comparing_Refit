using Comparing_Refit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
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

        // Authentication Configuration
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "your_issuer",
                ValidAudience = "your_audience",
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your_secret_key"))
            };
        })
        .AddOAuth("OAuth2", options =>
        {
            options.ClientId = "your_client_id";
            options.ClientSecret = "your_client_secret";
            options.AuthorizationEndpoint = "https://authorization-server.com/auth";
            options.TokenEndpoint = "https://authorization-server.com/token";
            options.CallbackPath = "/signin-oauth";
        });

        // OpenAPI and Scalar UI with Dual Security Schemes
        builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<MultiSecuritySchemeTransformer>(); });

        builder.Services.AddRefitClient<IApiService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.sampleapis.com/"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.WithTheme(ScalarTheme.Default).WithTitle("Scalar API Reference").WithDarkModeToggle(true);
        });

        app.MapGet("/hello", () => Results.Ok("Hello Scalar")).RequireAuthorization().WithOpenApi();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

internal sealed class MultiSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT"
            },
            ["OAuth2"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://authorization-server.com/auth"),
                        TokenUrl = new Uri("https://authorization-server.com/token")
                    }
                }
            }
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
        {
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = new List<string>(),
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "OAuth2", Type = ReferenceType.SecurityScheme } }] = new List<string>()
            });
        }

        return Task.CompletedTask;
    }
}
