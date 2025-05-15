namespace Comparing_Refit.ApiMappings
{
    public class RedirectMapping : IEndpoint
    {
        public void MapApiEndpoints(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/redirect").WithTags("Redirect").WithOpenApi();
            group.MapGet("/youtube", () => Results.Redirect("https://youtu.be/M766FGsv5do?t=7"))
                .WithSummary("123");
        }
    }
}
