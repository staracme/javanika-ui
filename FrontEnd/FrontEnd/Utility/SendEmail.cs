using FrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using SA.LA;

namespace FrontEnd.Utility
{
    public static class SendEmail
    {
        static readonly string _userName;
        static readonly string _passCode;
        static readonly string _host;
        static readonly int _port;
        static readonly int _timeOut;

        static SendEmail()
        {
            _userName = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPUsername"]);
            _passCode = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPPassword"]);
            _host = "smtp.gmail.com";
            _port = 587;
            _timeOut = 30000;
        }
        

        static public void SMTPEmail(SMTPEmailRequest smtpRequest)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = _host, // smtp server address here…
                    Port = _port,
                    EnableSsl = true,
                    //DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(_userName, _passCode),
                    Timeout = _timeOut,
                };

                MailMessage message = new MailMessage(_userName, smtpRequest.CustomerEmail
                , smtpRequest.MailTitle, smtpRequest.MailBody);
                message.IsBodyHtml = smtpRequest.IsBodyHtml;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Send Email",
                    Source = "SMTPEmail()",
                    Message = ex.Message,
                    Exception = ex
                };
                ExceptionLogger.ExceptionHandler(exceptionEntity);
            }
        }
    }
}