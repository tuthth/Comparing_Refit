using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Comparing_Refit.Transformer
{
    public sealed class MultiSecuritySchemeTransformer : IOpenApiDocumentTransformer
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
}
