namespace MailgunEventRecorder
{
    using System.Net.Http;
    using MailgunEventRecorder.Contracts;
    using MailgunEventRecorder.Infrastructure;
    using MailgunEventRecorder.Services;
    using MailgunEventRecorder.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<AppSettings>()
                .AddSingleton<ICheckpointStorage, CheckpointStorage>()
                .AddSingleton<IEventStorage, EventStorage>();

            services
                .AddHttpClient<IMailgunEventRetriever, MailgunEventRetriever>()
                .AddPolicyHandler(HttpPolicyMaker.GetHttpPolicy());

            services
                .AddHostedService<EventRecorder>();
        }
    }
}
