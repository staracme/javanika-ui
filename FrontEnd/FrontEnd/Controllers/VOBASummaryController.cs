using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using System.Net;
using System.Net.Mail;
namespace FrontEnd.Controllers
{
    public class VOBASummaryController : Controller
    {
        public class VOBAPaymentResponse
        {
            public string orderID { get; set; }
            public decimal amount { get; set; }
            public string status { get; set; }
            public string intent { get; set; }
            public int eventID { get; set; }
        }

        // GET: VOBASummary
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            if (!string.IsNullOrEmpty(Request["r_id"]))
            {
                int r_id = Convert.ToInt32(Request["r_id"]);
                var voba = db.VOBARegistrations.Where(r => r.RegistrationID == r_id).SingleOrDefault();
                return View(voba);
            }
            else
                return Redirect("/Home");
        }

        public JsonResult PlaceOrder(VOBAPaymentResponse response)
        {
            FinalPlaceOrderResponse resp = new FinalPlaceOrderResponse();
            try
            {
                int rID = Convert.ToInt32(Request["r_id"]);
                var order = db.VOBARegistrations.Where(v => v.RegistrationID == rID).SingleOrDefault();
                order.PaypalOrderID = response.orderID;
                order.PaymentStatus = response.status;
                order.Intent = response.intent;
                db.SaveChanges();

                string DataContents = System.IO.File.ReadAllText(Server.MapPath("~/Templates/registration_success.html"));
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

                MailMessage message = new MailMessage(username, order.ParentEmail, "Registration Confirmation", DataContents);
                message.IsBodyHtml = true;
                smtp.Send(message);


                //MailMessage message2 = new MailMessage(username, "javanikabooking@gmail.com", "VOBA event registration", DataContents);
                //message2.IsBodyHtml = true;
                //smtp.Send(message2);

                resp.status = "OK";
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resp.status = ex.InnerException.Message;
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
        }
    }
}