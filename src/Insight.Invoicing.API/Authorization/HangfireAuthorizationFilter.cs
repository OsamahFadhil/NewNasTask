using Hangfire.Dashboard;

namespace Insight.Invoicing.API.Authorization;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.IsInRole("Administrator");
        }

        return false;
    }
}


