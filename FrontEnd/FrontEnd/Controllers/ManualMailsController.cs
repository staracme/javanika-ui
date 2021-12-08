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
    public class ManualMailsController : Controller
    {
        // GET: ManualMails
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            try
            {
                var orders = db.tblTicketOrders.SqlQuery("select * from tblTicketOrders where status = 'SUCCESS' and PaymentStatus='COMPLETED' and Email not in ('omkargoforit@gmail.com', 'pranav@staracme.com', 'omkar@staracme.com', 'pran1990@gmail.com','niljag@yahoo.com')  AND EventID = 3039").ToList();

                string username = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
                string password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

                foreach (var order in orders)
                {
                    var isMailSent = db.tblMailLogs.Where(o => o.OrderID == order.OrderID).Any();

                    if (!isMailSent)
                    {
                        int orderID = order.OrderID;

                        int eventID = Convert.ToInt32(order.EventID);

                        string DataContents = System.IO.File.ReadAllText(Server.MapPath("~/Templates/ticket.html"));

                        var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

                        var booked_seats = db.tblSeatSelections.Where(s => s.OrderID == order.OrderID).ToList();

                        var o = db.tblTicketOrders.Where(os => os.OrderID == orderID).SingleOrDefault();

                        DataContents = DataContents.Replace("[srno]", order.OrderNo.ToString());
                        DataContents = DataContents.Replace("[event_name]", evt.EventName.ToString());
                        DataContents = DataContents.Replace("[tickets]", order.NoOfTickets.ToString());
                        DataContents = DataContents.Replace("[address]", "Centreville Junior High School, 37720 Fremonth, BLVD, Fremont, CA 94536");
                        DataContents = DataContents.Replace("[date]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        DataContents = DataContents.Replace("[image]", order.QRCode);


                        SmtpClient smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com", // smtp server address here…
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new System.Net.NetworkCredential(username, password),
                            Timeout = 30000,
                        };


                        MailMessage message = new MailMessage(username, order.Email, "Your Tickets", DataContents);
                        message.IsBodyHtml = true;
                        smtp.Send(message);


                        tblMailLog log = new tblMailLog();
                        log.OrderID = order.OrderID;
                        log.Email = order.Email;
                        log.SentDate = DateTime.Now;
                        log.MailContent = DataContents;
                        db.tblMailLogs.Add(log);
                        db.SaveChanges();

                        //string Receipt = System.IO.File.ReadAllText(Server.MapPath("~/Templates/receipt.html"));
                        //string type = "";

                        //Receipt = Receipt.Replace("[name]", order.Name.ToString());
                        //Receipt = Receipt.Replace("[order_no]", order.OrderID.ToString());


                        //decimal final_amount = Convert.ToDecimal(order.Amount);

                        //if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                        //{
                        //    decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        //    decimal process_amount = ((final_amount * process_fees_perc / 100));
                        //    Receipt = Receipt.Replace("[service_cost]", Decimal.Round(process_amount, 2).ToString());
                        //    Receipt = Receipt.Replace("[service_perc]", process_fees_perc.ToString() + "%");
                        //    final_amount = (final_amount + process_amount);
                        //}


                        //Receipt = Receipt.Replace("[sr_no]", order.OrderNo.ToString());
                        //Receipt = Receipt.Replace("[type]", type);
                        //Receipt = Receipt.Replace("[date]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + ", " + evt.ShowTime + " onwards");
                        //Receipt = Receipt.Replace("[individual_price]", order.AmountPerTicket.ToString());
                        //Receipt = Receipt.Replace("[sub_total]", Decimal.Round(Convert.ToDecimal(order.Amount), 2).ToString());
                        //Receipt = Receipt.Replace("[total_amount]", Decimal.Round(Convert.ToDecimal(final_amount), 2).ToString());
                        //Receipt = Receipt.Replace("[event_name]", evt.EventName.ToString());
                        //Receipt = Receipt.Replace("[no_of_tickets]", order.NoOfTickets.ToString());
                        //Receipt = Receipt.Replace("[address]", "Centreville Junior High School, 37720 Fremonth, BLVD, Fremont, CA 94536");
                        //Receipt = Receipt.Replace("[date_time]", Convert.ToDateTime(order.CreatedDate).ToString("dddd, dd MMMM yyyy hh:mm tt"));
                        //Receipt = Receipt.Replace("[event_date_time]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        //Receipt = Receipt.Replace("[image]", order.QRCode);

                        //MailMessage message1 = new MailMessage(username, order.Email, "Your Receipt", Receipt);
                        //message1.IsBodyHtml = true;
                        //smtp.Send(message1);
                    }
                }
                Response.Write("OK");
            }
            catch(Exception ex)
            {
                Response.Write("ERROR");
            }
            return View();
        }
    }
}