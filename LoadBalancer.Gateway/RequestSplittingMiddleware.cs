namespace LoadBalancer.Gateway
{
    public class RequestSplittingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;


        public RequestSplittingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var serviceEndpoints =  _configuration.GetSection("ReverseProxy:Clusters:service-cluster:Destinations")
                .GetChildren()
                .Select(x => x["Address"])
                .ToList();

            int totalEmployees = 300;
            int employeesPerService = totalEmployees / serviceEndpoints.Count;

            // Split the request into multiple sub-requests and forward them
            for (int i = 0; i < serviceEndpoints.Count; i++)
            {
                using (var httpClient = new HttpClient())
                {
                    using(var subRequest = new HttpRequestMessage(HttpMethod.Post, serviceEndpoints[i]))
                    {
                        subRequest.Content = new StringContent($"Request for employees {i * employeesPerService + 1} to {(i + 1) * employeesPerService}");
                    
                    
                        var response = await httpClient.SendAsync(subRequest).ConfigureAwait(false);

                        // Process the response if needed
                        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    
                    //
                    // Combine or aggregate results if necessary
                    //
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }

    public static class RequestSplittingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestSplittingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestSplittingMiddleware>();
        }
    }
}