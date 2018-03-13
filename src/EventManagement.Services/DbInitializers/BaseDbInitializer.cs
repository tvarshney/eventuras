using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;

namespace losol.EventManagement.Services.DbInitializers
{
    public abstract class BaseDbInitializer : IDbInitializer
    {
        protected readonly ApplicationDbContext _db;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _config;

		public BaseDbInitializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager,  UserManager<ApplicationUser> userManager, IConfiguration config)
		{
			_db = db;
			_config = config;
			_roleManager = roleManager;
			_userManager = userManager;
		}

        public virtual async Task SeedAsync()
        {
            // Add administrator role if it does not exist
			string[] roleNames = { "Admin", "SuperAdmin" };
			IdentityResult roleResult;
			foreach (var roleName in roleNames)
			{
				var roleExist = await _roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}

			// Add super-admin if none exists
			if (!_userManager.GetUsersInRoleAsync("SuperAdmin").Result.Any())
			{

				if (string.IsNullOrEmpty(_config.GetSection("SuperAdmin")["Email"]))
				{
					throw new System.ArgumentException("SuperAdmin email not set. Please check install documentation");
				}

				if (string.IsNullOrEmpty(_config.GetSection("SuperAdmin")["Password"]))
				{
					throw new System.ArgumentException("SuperAdmin password not set. Please check install documentation");
				}

				var _user = await _userManager.FindByEmailAsync(_config.GetSection("SuperAdmin")["Email"]);

				if (_user == null)
				{
					var superadmin = new ApplicationUser
                    {
						UserName = _config.GetSection("SuperAdmin")["Email"],
						Email = _config.GetSection("SuperAdmin")["Email"],
						EmailConfirmed = true
					};
					string UserPassword = _config.GetSection("SuperAdmin")["Password"];
					var createSuperAdmin = await _userManager.CreateAsync(superadmin, UserPassword);
					if (createSuperAdmin.Succeeded)
					{
						await _userManager.AddToRoleAsync(superadmin, "SuperAdmin");
					}
				}

			}

			// Seed payment methods
			if (!_db.PaymentMethods.Any())
			{
				var paymentMethods = new PaymentMethod[] {
					new PaymentMethod {Code="Card", Name="Kortbetaling", Active=false},
					new PaymentMethod {Code="Email_invoice", Name="E-postfaktura", Active=true},
					new PaymentMethod {Code="EHF_invoice", Name="EHF-faktura", Active=true}
				};

				foreach (var item in paymentMethods)
				{
					await _db.PaymentMethods.AddAsync(item);
				}

				await _db.SaveChangesAsync();
			}

			// Seed test events if no events exist. 
			if (!_db.EventInfos.Any())
			{
				var eventInfos = new EventInfo[]
				{
					new EventInfo{Title="Test event 01", Code="Test01", Description="A test event."},
					new EventInfo{Title="Test event 02", Code="Test02", Description="Another test event."}
				};

				foreach (var item in eventInfos)
				{
					await _db.EventInfos.AddAsync(item);
				}

				await _db.SaveChangesAsync();
			}
        }
    }
}