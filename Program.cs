using Serilog;

namespace BPLSWebSSOApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(
                (hostContext, services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(hostContext.Configuration);
                })
                .ConfigureWebHostDefaults(config =>
                {
                    config.UseStartup<Startup>();
                }
                );
    }
}

