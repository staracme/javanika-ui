using FrontEnd.Models;
using QRCoder;
using SA.LA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FrontEnd
{
    public class Utilities
    {
        public static string GenerateQRCode(dynamic order, int requestOrderID)
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
                return order.QRCode;
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
            return string.Empty;
        }

        public static void SetEmailContent(dynamic order, object evt, int orderID)
        {
            SMTPEmailRequest smtpRequest;
            try
            {
                //smtpRequest = new SMTPEmailRequest();

                //smtpRequest.OrderID = order.OrderNo.ToString();
                //smtpRequest.EventName = evt.EventName.ToString();
                //smtpRequest.Tickets = order.NoOfTickets.ToString();
                //smtpRequest.Address = "Centreville Junior High School, 37720 Fremonth, BLVD, Fremont, CA 94536";
                //smtpRequest.Date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime;
                //smtpRequest.QRCodeImage = order.QRCode;
                //smtpRequest.CustomerEmail = order.Email;

                //smtpRequest.TemplatePath = Server.MapPath("~/Templates/ticket.html");
                //string dataContents = System.IO.File.ReadAllText(smtpRequest.TemplatePath);
                //dataContents = dataContents.Replace("[srno]", smtpRequest.OrderID);
                //dataContents = dataContents.Replace("[event_name]", smtpRequest.EventName);
                //dataContents = dataContents.Replace("[tickets]", smtpRequest.Tickets);
                //dataContents = dataContents.Replace("[address]", smtpRequest.Address);
                //dataContents = dataContents.Replace("[date]", smtpRequest.Date);
                //dataContents = dataContents.Replace("[image]", smtpRequest.QRCodeImage);
                //smtpRequest.MailBody = dataContents;
                //smtpRequest.IsBodyHtml = true;
                //smtpRequest.IsBodyHtml = true;
                //Task.Run(() => SendEmail.SMTPEmail(smtpRequest));
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
    }
}