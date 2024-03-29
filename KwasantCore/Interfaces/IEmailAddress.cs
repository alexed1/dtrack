using System;
using System.Collections.Generic;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Interfaces
{
    public interface IEmailAddress
    {
        EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address);
        List<ParsedEmailAddress> ExtractFromString(String textToSearch);
        List<EmailAddressDO> GetEmailAddresses(IUnitOfWork uow, params string[] textToSearch);
    }
}