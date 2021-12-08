using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd.Models
{
    public class SMTPEmailRequest
    {
        public string TemplatePath { get; set; }
        public string OrderID { get; set; }
        public string SrNo { get; set; }
        public string EventName { get; set; }
        public string Tickets { get; set; }
        public string Address { get; set; }
        public string Date { get; set; }
        public string QRCodeImage { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string MailTitle { get; set; }
        public string MailBody { get; set; }
        public bool IsBodyHtml { get; set; }
        
    }
}