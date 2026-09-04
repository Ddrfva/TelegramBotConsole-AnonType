using Core.DataAccess;
using Core.Services;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace TelegramBot_31.WebAdmin
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING not found in .env");

            services.AddSingleton<IDataContextFactory<ToDoDataContext>>(new DataContextFactory(connectionString));

            services.AddScoped<IUserRepository>(sp =>
            {
                var factory = sp.GetRequiredService<IDataContextFactory<ToDoDataContext>>();
                return new SqlUserRepository(factory);
            });
            services.AddScoped<IToDoRepository>(sp =>
            {
                var factory = sp.GetRequiredService<IDataContextFactory<ToDoDataContext>>();
                return new SqlToDoRepository(factory);
            });
            services.AddScoped<IToDoListRepository>(sp =>
            {
                var factory = sp.GetRequiredService<IDataContextFactory<ToDoDataContext>>();
                return new SqlToDoListRepository(factory);
            });
            services.AddScoped<INotificationService>(sp =>
            {
                var factory = sp.GetRequiredService<IDataContextFactory<ToDoDataContext>>();
                return new NotificationService(factory);
            });

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await next();
            });

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=Index}/{id?}");
            });
        }
    }
}