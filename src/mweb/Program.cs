namespace Minustar.Website;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = BuildHost(args);
        await host.RunAsync();
    }

    private static IHost BuildHost(params string[] args)
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(c =>
            {
                c.UseStartup<Startup>();
            })
            .Build();
    }
}
