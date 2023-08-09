namespace OliverBooth.Middleware;

internal static class RssEndpointExtensions
{
    public static IEndpointConventionBuilder MapRssFeed(this IEndpointRouteBuilder endpoints, string pattern)
    {
        RequestDelegate pipeline = endpoints.CreateApplicationBuilder()
            .UseMiddleware<RssMiddleware>()
            .Build();

        return endpoints.Map(pattern, pipeline).WithDisplayName("RSS Feed");
    }
}
