using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using QRCoder;
using System.Drawing;
using FrontEnd.Utility;
using SA.LA;
using System.Threading.Tasks;

namespace FrontEnd.Controllers
{
    public class FinalPlaceOrderResponse
    {
        public string status { get; set; }
        public string orderID { get; set; }
    }

   
    public class OrderSummaryController : Controller
    {
        // GET: OrderSummary
        JAVADBEntities db = new JAVADBEntities();

        public ActionResult Index()
        {
            int orderID = Convert.ToInt32(Session["OrderID"]);
            var order = db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault();
            var evt = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();

            decimal final_amount = Convert.ToDecimal(order.Amount);

            if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
            {
                decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                decimal process_amount = ((final_amount * process_fees_perc / 100));
                final_amount = (final_amount + process_amount);

                ViewData["ProcessingPercentage"] = process_fees_perc;
                ViewData["ProcessingFee"] = process_amount;
            }

            ViewData["Amount"] = final_amount;            
            return View(order);
        }

        public JsonResult PlaceOrder(PaymentResponse response)
        {
            FinalPlaceOrderResponse resp = new FinalPlaceOrderResponse();
            SMTPEmailRequest smtpRequest = null;
            try
            {
                smtpRequest = new SMTPEmailRequest();
                int orderID = Convert.ToInt32(response.sys_orderID);

                var order = SaveTicketOrder(response) as tblTicketOrder;

                GenerateQRCode(order, orderID);
                SetEmailContent(order.OrderID);
                SetReceiptEmail(order.OrderID);
                var evt = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();
                //deduct number of tickets from the stock
                //if (evt.EventID == 3039)
                //{
                if (order.NoOfTickets <= evt.TicketsAvailable)
                    {
                        //Recheck this logic with omkar
                        evt.TicketsAvailable = (evt.TicketsAvailable - order.NoOfTickets);
                        db.SaveChanges();
                    }
                //}
                resp.orderID = order.OrderID.ToString();
                resp.status = "OK";
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Order Summary Controller",
                    Source = "PlaceOrder()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
                //resp.status = ex.Message;
                resp.status = "Error occurred";
            }
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        private object SaveTicketOrder(PaymentResponse response)
        {
            dynamic order = null;
            try
            {
                int orderID = Convert.ToInt32(response.sys_orderID);
                var orders = db.tblTicketOrders.Where(o => o.Status == "SUCCESS" && o.PaymentStatus == "COMPLETED").ToList();
                int serialNo = (orders.Count() > 0 ? Convert.ToInt32((orders.OrderByDescending(o => o.OrderID).Take(1).SingleOrDefault().OrderNo) + 1) : 1);

                order = db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault();
                order.OrderNo = serialNo;
                order.Status = "SUCCESS";
                order.PaypalOrderID = response.orderID;
                order.PaymentStatus = response.status;
                order.Intent = response.intent;
                db.SaveChanges();
               
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Order Summary Controller",
                    Source = "SaveTicketOrder()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }
            return order;
        }
        private void GenerateQRCode(dynamic order, int requestOrderID)
        {
            try
            {
                string QRCodePath = System.Configuration.ConfigurationManager.AppSettings["QRCodePath"].ToString();
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(order.OrderID.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                qrCodeImage.Save(QRCodePath + requestOrderID.ToString() + ".png");
                order.QRCode = requestOrderID.ToString() + ".png";
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Order Summary Controller",
                    Source = "GenerateQRCode()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }
        }

        private void SetEmailContent(int orderID)
        {
            SMTPEmailRequest smtpRequest;
            try
            {
                smtpRequest = new SMTPEmailRequest();
                var order = db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault();
                var evt = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();

                smtpRequest.OrderID = order.OrderNo.ToString();
                smtpRequest.EventName = evt.EventName.ToString();
                smtpRequest.Tickets = order.NoOfTickets.ToString();
                smtpRequest.Address = "Centreville Junior High School, 37720 Fremonth, BLVD, Fremont, CA 94536";
                smtpRequest.Date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime;
                smtpRequest.QRCodeImage = order.QRCode;
                smtpRequest.CustomerEmail = order.Email;

                smtpRequest.TemplatePath = Server.MapPath("~/Templates/ticket.html");
                string dataContents = System.IO.File.ReadAllText(smtpRequest.TemplatePath);
                dataContents = dataContents.Replace("[srno]", smtpRequest.OrderID);
                dataContents = dataContents.Replace("[event_name]", smtpRequest.EventName);
                dataContents = dataContents.Replace("[tickets]", smtpRequest.Tickets);
                dataContents = dataContents.Replace("[address]", smtpRequest.Address);
                dataContents = dataContents.Replace("[date]", smtpRequest.Date);
                dataContents = dataContents.Replace("[image]", smtpRequest.QRCodeImage);
                smtpRequest.MailBody = dataContents;
                smtpRequest.IsBodyHtml = true;
                smtpRequest.IsBodyHtml = true;
                Task.Run(() => SendEmail.SMTPEmail(smtpRequest));
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Order Summary Controller",
                    Source = "SetEmailContent()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }
            
        }

        private void SetReceiptEmail(int orderID)
        {
            SMTPEmailRequest smtpRequest;
            try
            {
                smtpRequest = new SMTPEmailRequest();
                //string username = System.Configuration.ConfigurationManager.AppSettings["SMTPUsername"].ToString();

                var order = db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault();
                var evt = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();

                string receiptTemplatePath = Server.MapPath("~/Templates/receipt.html");
                string receipt = System.IO.File.ReadAllText(receiptTemplatePath);
                string type = "";

                receipt = receipt.Replace("[name]", order.Name.ToString());
                receipt = receipt.Replace("[order_no]", order.OrderID.ToString());

                decimal final_amount = Convert.ToDecimal(order.Amount);

                if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                {
                    decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                    decimal process_amount = ((final_amount * process_fees_perc / 100));
                    receipt = receipt.Replace("[service_cost]", Decimal.Round(process_amount, 2).ToString());
                    receipt = receipt.Replace("[service_perc]", process_fees_perc.ToString() + "%");
                    final_amount = (final_amount + process_amount);
                }

                receipt = receipt.Replace("[sr_no]", order.OrderNo.ToString());
                receipt = receipt.Replace("[type]", type);
                receipt = receipt.Replace("[date]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + ", " + evt.ShowTime + " onwards");
                receipt = receipt.Replace("[individual_price]", order.AmountPerTicket.ToString());
                receipt = receipt.Replace("[sub_total]", Decimal.Round(Convert.ToDecimal(order.Amount), 2).ToString());
                receipt = receipt.Replace("[total_amount]", Decimal.Round(Convert.ToDecimal(final_amount), 2).ToString());
                receipt = receipt.Replace("[event_name]", evt.EventName.ToString());
                receipt = receipt.Replace("[no_of_tickets]", order.NoOfTickets.ToString());
                receipt = receipt.Replace("[address]", "Centreville Junior High School, 37720 Fremonth, BLVD, Fremont, CA 94536");
                receipt = receipt.Replace("[date_time]", Convert.ToDateTime(order.CreatedDate).ToString("dddd, dd MMMM yyyy hh:mm tt"));
                receipt = receipt.Replace("[event_date_time]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                receipt = receipt.Replace("[image]", order.QRCode);

                //Receipt
                smtpRequest.CustomerName = order.Name;
                smtpRequest.CustomerEmail = order.Email;
                smtpRequest.MailTitle = "Your Receipt";
                smtpRequest.MailBody = receipt;
                smtpRequest.IsBodyHtml = true;
                Task.Run(() => SendEmail.SMTPEmail(smtpRequest));

                //New Order
                SMTPEmailRequest smtpRequest1 = new SMTPEmailRequest();
                smtpRequest1.CustomerName = order.Name;
                smtpRequest1.CustomerEmail = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["AdminEmail"]); //"javanikabooking@gmail.com";
                smtpRequest1.MailTitle = "New Order";
                smtpRequest1.MailBody = receipt;
                smtpRequest1.IsBodyHtml = true;
                Task.Run(() => SendEmail.SMTPEmail(smtpRequest1));
            }
            catch (Exception ex)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Order Summary Controller",
                    Source = "SetReceiptEmail()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }

        }
    }
}