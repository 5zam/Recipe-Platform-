using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            //
            //dependency injection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            var app = builder.Build();

            //
            await SeedAdminUserAsync(app);
            //


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //
            app.MapRazorPages();


            await app.RunAsync();


        }
        private static async Task SeedAdminUserAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Roles
            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));
            }

            // Admin user
            const string adminEmail = "admin@tastebud.com";
            const string adminPassword = "Admin@123";

            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var res = await userMgr.CreateAsync(admin, adminPassword);
                if (res.Succeeded)
                    await userMgr.AddToRoleAsync(admin, "Admin");
            }
            else if (!await userMgr.IsInRoleAsync(admin, "Admin"))
            {
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
    