﻿
using Microsoft.Extensions.Caching.Distributed;

namespace Platform
{
    public class SumEndpoint
    {
        public async Task Endpoint(HttpContext context, IDistributedCache cache)
        {
            int count;
            int.TryParse((string?)context.Request.RouteValues["count"], out count);

            string cacheKey = $"sum_{count}";

            string totalString = cache.GetString(cacheKey);

            if (totalString == null)
            {

                long total = 0;
                for (int i = 1; i <= count; i++)
                {
                    total += i;
                }
                 totalString = $"({DateTime.Now.ToLongTimeString()}) {total}";

                await cache.SetStringAsync(cacheKey, totalString, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            }

            await context.Response.WriteAsync($"({DateTime.Now.ToLongTimeString()}) Total for {count}"
                    + $" values:\n{totalString}\n");
        }
    }
}
