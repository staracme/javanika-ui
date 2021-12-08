using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using System.Net.Mail;
namespace FrontEnd.Controllers
{
    public class TestMailController : Controller
    {
        // GET: TestMail
        public ActionResult Index()
        {
            string username = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
            string password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();


            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // smtp server address here…
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(username, password),
                Timeout = 30000,
            };


            MailMessage message = new MailMessage(username, "pran1990@gmail.com", "Your Tickets", "Test Mail");
            message.IsBodyHtml = true;
            smtp.Send(message);

            return View();
        }
    }
}