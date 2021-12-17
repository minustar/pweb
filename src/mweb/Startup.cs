namespace Minustar.Website;

public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void COnfigureServices(IServiceCollection services)
    {
        var connStr = GetConnectionString();
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connStr));

        services.AddDefaultIdentity<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = XForwardedFor | XForwardedProto;
        });

        services.AddRazorPages()
            //.AddRazorRuntimeCompilation()
            ;
    }

    private string GetConnectionString()
    {
        return configuration.GetConnectionString("Default");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapAreaControllerRoute(
                name: "API_Route",
                areaName: "API",
                pattern: "api/{controller}/{action}/{id?}");
            endpoints.MapDefaultControllerRoute();
            endpoints.MapRazorPages();
        });
    }
}
