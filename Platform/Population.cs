namespace Platform
{
    public class Population
    {
        public static async Task Endpoint(HttpContext context, ILogger<Population> logger)
        {
            logger.LogDebug($"Started processing for {context.Request.Path}");
            string city = context.Request.RouteValues["city"] as string ?? "london";
            int? pop = null;
            switch (city.ToLower())
            {
                case "london":
                    pop = 8136000;
                    break;
                case "paris":
                    pop = 2_141_000;
                    break;
                case "monaco":
                    pop = 39000;
                    break;
            }
            if (pop.HasValue)
            {
                await context.Response
                .WriteAsync($"City: {city}, Population: {pop}");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            logger.LogDebug($"Finished processing for {context.Request.Path}");
        }
    }
}