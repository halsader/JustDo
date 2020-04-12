using System;
using System.IO;

using JustDo.Infrastructure;
using JustDo.Infrastructure.Db.Entity;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using Serilog;

namespace JustDo {
    public class Program {
        public static readonly string Namespace = typeof(Program).Namespace;
        //public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

        public static int Main(string[] args) {
            var configuration = GetConfiguration();
            var logger = CreateSerilogLogger(configuration);
            Log.Logger = logger;

            try {
                Log.Information("Configuring web host ({ApplicationContext})...", Namespace);
                var host = CreateWebHostBuilder(args).Build();

                Log.Information("Applying migrations ({ApplicationContext})...", Namespace);

                host.MigrateDbContext<TodoContext>((context, services) => {
                });

                Log.Information("Starting web host ({ApplicationContext})...", Namespace);
                host.Run();

                return 0;
            } catch (Exception ex) {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Namespace);
                return 1;
            } finally {
                Log.CloseAndFlush();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var configuration = GetConfiguration();
            return WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(false)
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(configuration)
                .UseSerilog();
        }

        private static ILogger CreateSerilogLogger(IConfiguration configuration) {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", Namespace)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("./log.txt")
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static IConfiguration GetConfiguration() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}