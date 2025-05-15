using Microsoft.Extensions.Http;
using Refit;

namespace Comparing_Refit.Configurations
{
    public static class WebClients
    {
        public static IServiceCollection AddApiClientWithPolly(this IServiceCollection services)
        {
            // Get the combined policy wrap from your PollyPolicies class
            var policyWrap = PollyPolicies.GetPolicyWrap();

            // Register HttpClient with base address and policies
            services.AddHttpClient("MyApiClient", client =>
            {
                client.BaseAddress = new Uri("https://api.sampleapis.com/");
            })
            .AddPolicyHandler(policyWrap);

            // Register Refit client using the above HttpClient and policy handler
            services.AddRefitClient<IApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.sampleapis.com/"))
                .AddHttpMessageHandler(() => new PolicyHttpMessageHandler(policyWrap));

            return services;
        }
    }
}
