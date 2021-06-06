using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace FM.Infrastructure.Mail.MailKit.Tests.Smtp4Dev
{
    public interface ISmtp4DevClient
    {
        [Get("/api/Messages")]
        Task<IEnumerable<Message>> GetMessagesAsync();

        [Get("/api/Messages/{id}/html")]
        Task<string> GetHtmlBodyAsync(Guid id);
    }
}
