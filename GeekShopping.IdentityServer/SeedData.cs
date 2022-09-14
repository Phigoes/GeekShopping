using GeekShopping.IdentityServer.Initializer;

namespace GeekShopping.IdentityServer
{
    public class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                db.Initialize();
            }
        }
    }
}
