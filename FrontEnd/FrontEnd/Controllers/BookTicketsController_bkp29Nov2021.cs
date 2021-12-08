using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace FrontEnd.Controllers
{
    public class BookResponse
    {
        public string status { get; set; }
        public int orderID { get; set; }
        public int no_of_tickets { get; set; }
        public int ticket_stock { get; set; }

    }


    public class PaymentResponse
    {
        public string orderID { get; set; }
        public string sys_orderID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public decimal amount { get; set; }
        public int no_of_tickets { get; set; }
        public string status { get; set; }
        public string intent { get; set; }
        public int eventID { get; set; }
    }

    public class tblTempEvent
    {
        public int EventID { get; set; }
        public int TicketStock { get; set; }
    }

    public class BookTicketsController : Controller
    {
        // GET: BookTickets
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            //int eventID = Convert.ToInt32(Request["eventID"]);
            //var evt = db.tblEvents.SqlQuery("SELECT EVENTID,TicketStock FROM TBLEVENTS WHERE EVENTID = " + eventID + "").SingleOrDefault();
            return View();
        }


        public JsonResult CheckTicketStock()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int no_of_tickets = Convert.ToInt32(Request["no_of_tickets"]);
            var evt = db.tblEvents.Where(e=>e.EventID == eventID).SingleOrDefault();

            BookResponse response = new BookResponse();

            //check if stock is available
            if (no_of_tickets <= evt.TicketStock)
            {
                response.status = "YES";
                response.ticket_stock = Convert.ToInt32(evt.TicketStock);
            }
            else
            {
                response.status = "NO";
                response.ticket_stock = Convert.ToInt32(evt.TicketStock);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BookTickets()
        {
            if(Session["OrderID"] != null)
            {

            }
            //Session id logic should come
            //Early bird logic should come
            string name = Request["txtFirstName"];
            string email = Request["txtEmail"];
            int i = 0;
            
            BookResponse response = new BookResponse();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(Request["txtNoOfTickets"]) && int.TryParse(Request["txtNoOfTickets"], out i))
            {
                int noOfTickets = Convert.ToInt32(Request["txtNoOfTickets"]);
                int eventID = Convert.ToInt32(Request["eventID"]);
                //decimal price = Convert.ToDecimal(40);
                decimal price = Convert.ToDecimal(0.01);

                if (eventID == 3039)
                    price = 50;

                var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();


                //if (noOfTickets >= 1 && noOfTickets <= 9)
                //    price = 30;
                //else if (noOfTickets >= 10 && noOfTickets <= 14)
                //    price = 25;
                //else if (noOfTickets >= 15 && noOfTickets <= 19)
                //    price = 22;
                //else if (noOfTickets >= 20)
                //    price = 20;

                decimal totalAmount = (noOfTickets * price);

                if(eventID == 3039)
                {
                    //check if stock is available
                    if (noOfTickets <= evt.TicketStock)
                    {
                        tblTicketOrder order = new tblTicketOrder();
                        order.EventID = Convert.ToInt32(Request["eventID"]);
                        order.Name = name;
                        order.Email = email;
                        order.NoOfTickets = noOfTickets;
                        order.AmountPerTicket = price;
                        order.Amount = totalAmount;
                        order.Status = "PENDING";
                        order.PaymentStatus = "PENDING";
                        order.CreatedDate = DateTime.Now;
                        db.tblTicketOrders.Add(order);
                        db.SaveChanges();


                        response.status = "OK";
                        response.orderID = order.OrderID;
                        Session["OrderID"] = Convert.ToInt32(order.OrderID);
                    }
                    else
                    {
                        response.ticket_stock = Convert.ToInt32(evt.TicketStock);
                        response.status = "OUT_OF_STOCK";
                    }
                }
                else
                {
                    tblTicketOrder order = new tblTicketOrder();
                    order.EventID = Convert.ToInt32(Request["eventID"]);
                    order.Name = name;
                    order.Email = email;
                    order.NoOfTickets = noOfTickets;
                    order.AmountPerTicket = price;
                    order.Amount = totalAmount;
                    order.Status = "PENDING";
                    order.PaymentStatus = "PENDING";
                    order.CreatedDate = DateTime.Now;
                    db.tblTicketOrders.Add(order);
                    db.SaveChanges();


                    response.status = "OK";
                    response.orderID = order.OrderID;

                    Session["OrderID"] = Convert.ToInt32(order.OrderID);
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                response.status = "INVALID DATA";
                response.orderID = 0;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}