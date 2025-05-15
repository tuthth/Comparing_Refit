using Comparing_Refit;
using Comparing_Refit.ApiMappings;
using Comparing_Refit.Configurations;
using Comparing_Refit.Transformer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
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

        builder.Services.AddBasicSettings();

        builder.Services.AddEndpoints();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.WithTheme(ScalarTheme.Default).WithTitle("Scalar API Reference").WithDarkModeToggle(true);
        });

        app.MapEndpoints();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
