using System;
using System.Diagnostics.CodeAnalysis;
using FM.Application.Mail;
using FM.Domain.Common;
using FM.Infrastructure.Mail;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddMailKit(this IServiceCollection services) => services
            .AddConfig<EmailConfig.OutgoingConfig>()
            .AddConfig<RetryConfig>(cfg =>
            {
                if (cfg.Max == default) cfg.Max = RetryConfig.Default.Max;
                if (cfg.Delay == TimeSpan.Zero) cfg.Delay = TimeSpan.FromSeconds(RetryConfig.Default.Delay);
            })
            .AddTransient<ISmtpClient, SmtpClient>()
            .AddTransient<IPop3Client, Pop3Client>()
            .AddTransient<IImapClient, ImapClient>()
            .AddTransient<IMailSender, MailKitMailSender>()
            .AddTransient<IMailReader, MailKitPop3Reader>()
            .AddTransient<IMailReader, MailKitImapReader>();

        private static IServiceCollection AddConfig<TConfig>(this IServiceCollection services,
            Action<TConfig>? configure = null) where TConfig : class =>
            services.AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                string key = typeof(TConfig).GetConfigName();
                var config = configuration.GetSection(key).Get<TConfig>();
                configure?.Invoke(config);
                return config;
            });
    }
}
