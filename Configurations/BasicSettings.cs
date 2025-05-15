using Comparing_Refit.ApiMappings;
using Comparing_Refit.Transformer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

namespace Comparing_Refit.Configurations
{
    public static class BasicSettings
    {
        public static IServiceCollection AddBasicSettings(this IServiceCollection services)
        {
            // Authentication Configuration
            services.AddAuthentication(options =>
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
            services.AddOpenApi("v1", options => { options.AddDocumentTransformer<MultiSecuritySchemeTransformer>(); });

            services.AddApiClientWithPolly();

            services.AddEndpointsApiExplorer();

            return services;
        }

        public static WebApplication MapEndpoints(this WebApplication app, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var referencedAssemblies = entryAssembly?.GetReferencedAssemblies()
                    .Select(Assembly.Load)
                    .ToList() ?? new List<Assembly>();

                referencedAssemblies.Add(entryAssembly);
                assemblies = referencedAssemblies.ToArray();
            }

            var endpointTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) &&
                            !t.IsInterface &&
                            !t.IsAbstract)
                .ToList();

            foreach (var endpointType in endpointTypes)
            {
                var endpoint = Activator.CreateInstance(endpointType) as IEndpoint;
                endpoint?.MapApiEndpoints(app);

                app.Logger.LogInformation("Registered endpoints from {EndpointType}", endpointType.Name);
            }

            return app;
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var referencedAssemblies = entryAssembly?.GetReferencedAssemblies()
                    .Select(Assembly.Load)
                    .ToList() ?? new List<Assembly>();

                referencedAssemblies.Add(entryAssembly);
                assemblies = referencedAssemblies.ToArray();
            }

            var endpointTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) &&
                            !t.IsInterface &&
                            !t.IsAbstract)
                .ToList();

            foreach (var endpointType in endpointTypes)
            {
                services.AddTransient(typeof(IEndpoint), endpointType);
            }

            return services;
        }
    }
}
