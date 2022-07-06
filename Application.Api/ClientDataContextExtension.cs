using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.PermissionHelpers;
using Microsoft.EntityFrameworkCore;

namespace Application.Api
{
    public static class ClientDataContextExtensions
    {
        public static void AddClientDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(provider =>
            {
                var httpContext = provider.GetService<IHttpContextAccessor>();
                var hasReferer = httpContext.HttpContext.Request.Headers.TryGetValue("Referer", out var referer);
                if (hasReferer)
                {
                    string subdomain = new System.Uri(referer.First()).Host.Split('.')[0];
                    var index= subdomain.IndexOf("pos");
                    var connString = configuration.GetConnectionString("TemplateConnection").Replace("__DBNAME__", index==-1?subdomain: subdomain.Substring(0, index));
                    var opts = new DbContextOptionsBuilder<GHP_POS_DBContext>();
                    opts.UseSqlServer(connString);
                   // opts.EnableSensitiveDataLogging();
                    return new GHP_POS_DBContext(opts.Options);
                }
                else
                {
                    throw new Exception("Referer not present in request header");
                }
                
            });
        }
    }
}
