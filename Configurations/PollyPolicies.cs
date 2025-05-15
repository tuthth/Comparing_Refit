using Polly;
using Polly.Extensions.Http;

namespace Comparing_Refit.Configurations
{
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(15),
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine($"Circuit broken for {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    },
                    onReset: () => Console.WriteLine("Circuit reset"),
                    onHalfOpen: () => Console.WriteLine("Circuit half-open, next call is a trial")
                );
        }

        public static IAsyncPolicy<HttpResponseMessage> GetPolicyWrap()
        {
            return Policy.WrapAsync(GetRetryPolicy(), GetCircuitBreakerPolicy());
        }
    }
}
