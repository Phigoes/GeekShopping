using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;

        public DbInitializer(MySQLContext context, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role)
        {
            _context = context;
            _user = user;
            _role = role;
        }

        public void Initialize()
        {
            if (_role.FindByNameAsync(Config.Admin).Result != null) return;
            _role.CreateAsync(new IdentityRole(Config.Admin)).GetAwaiter().GetResult();
            _role.CreateAsync(new IdentityRole(Config.Client)).GetAwaiter().GetResult();

            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "Philipp-Admin",
                Email = "phi_admin@teste.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (11) 12345-6789",
                FirstName = "Philipp",
                LastName = "Admin"
            };

            _user.CreateAsync(admin, "Senha@123").GetAwaiter().GetResult();
            _user.AddToRoleAsync(admin, Config.Admin).GetAwaiter().GetResult();

            var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                new Claim(JwtClaimTypes.Role, Config.Admin)
            }).Result;

            ApplicationUser client = new ApplicationUser()
            {
                UserName = "Philipp-Client",
                Email = "phi_client@teste.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (11) 12345-6789",
                FirstName = "Philipp",
                LastName = "Client"
            };

            _user.CreateAsync(client, "Senha@123").GetAwaiter().GetResult();
            _user.AddToRoleAsync(client, Config.Client).GetAwaiter().GetResult();

            var clientClaims = _user.AddClaimsAsync(client, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName, client.FirstName),
                new Claim(JwtClaimTypes.FamilyName, client.LastName),
                new Claim(JwtClaimTypes.Role, Config.Client)
            }).Result;
        }
    }
}
