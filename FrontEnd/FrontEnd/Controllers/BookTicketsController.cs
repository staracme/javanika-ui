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
    public class OrderInput
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int NoOfTickets { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string TicketType { get; set; }
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

        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Index()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            //var evt = db.tblEvents.SqlQuery("SELECT EVENTID,TicketStock FROM TBLEVENTS WHERE EVENTID = " + eventID + "").SingleOrDefault();
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            TempData["minTicketsAllowed"] = evt.EBMinTicketOrder;
            TempData["maxTicketsAllowed"] = evt.EBMaxTicketOrder;
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

        public JsonResult CheckSeatsAvailability()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int no_of_tickets = Convert.ToInt32(Request["no_of_tickets"]);
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

            BookResponse response = new BookResponse();
            //check if stock is available
            var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                                && e.Status == "SUCCESS"
                                                && e.PaymentStatus == "COMPLETED").ToList();

            int totalSeatsAvailable = (int)(evt.TicketStock - soldTicketsCount.Count);
            //to check current no of tickets does not exceed thre remaining stock of tickets
            bool isTicketCountValid = totalSeatsAvailable > no_of_tickets;

            if (isTicketCountValid)
            {
                response.status = "YES";
                response.ticket_stock = Convert.ToInt32(totalSeatsAvailable);
            }
            else
            {
                response.status = "NO";
                response.ticket_stock = Convert.ToInt32(totalSeatsAvailable);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BookTickets()
        {
            //Session id logic should come
            //Early bird logic should come
            string name = Request["txtFirstName"];
            string email = Request["txtEmail"];
            int eventID = Convert.ToInt32(Request["eventID"]);
            int i = 0;
            
            BookResponse response = new BookResponse();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(Request["txtNoOfTickets"]) && int.TryParse(Request["txtNoOfTickets"], out i))
            {
                //if (Session["OrderID"] == null)
                //{
                    int noOfTickets = Convert.ToInt32(Request["txtNoOfTickets"]);
                    eventID = Convert.ToInt32(Request["eventID"]);
                    //decimal price = Convert.ToDecimal(40);
                    decimal price = Convert.ToDecimal(0.01);

                    var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
                    price = (decimal)evt.TicketPrice;
                    
                    var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                                && e.Status == "SUCCESS"
                                                && e.PaymentStatus == "COMPLETED").ToList();

                    decimal totalAmount = (noOfTickets * price);

                    var input = new OrderInput
                    {
                        CustomerName = name,
                        CustomerEmail = email,
                        NoOfTickets = noOfTickets,
                        Price = price,
                        TotalAmount = totalAmount
                    };
                    //Logic to check EArly Bird ticket allocation base on configurtion 
                    //datewise ot seatwise
                    if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.DATEWISE.ToString())
                    {
                        DateTime currentDate = System.DateTime.Today;
                        if (currentDate >= evt.StartDate && currentDate <= evt.EndDate)
                        {
                            input.TicketType = TicketType.EARLYBIRD.ToString();
                            input.Price = (decimal)evt.EBTicketPrice;
                        }
                        else
                        {
                            input.TicketType = TicketType.GENERAL.ToString();
                            input.Price = (decimal)evt.TicketPrice;
                        }
                    }
                    else if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.SEATWISE.ToString())
                    {
                        int percentEBSeats = (int)evt.PercentEBSeats;
                        int totalEBSeatsAllowed = (int)((evt.TicketStock * percentEBSeats) / 100);

                        if (totalEBSeatsAllowed >= noOfTickets && evt.TicketsAvailable >= noOfTickets)
                        {
                            input.TicketType = TicketType.EARLYBIRD.ToString();
                            input.Price = (decimal)evt.EBTicketPrice;
                        }
                        else
                        {
                            input.TicketType = TicketType.GENERAL.ToString();
                            input.Price = (decimal)evt.TicketPrice;
                        }
                    }
                    input.TotalAmount = (input.NoOfTickets * input.Price);
                    //removing hard coded logic
                    #region MyRegion
                    //if (eventID == 3039)
                    //{
                    //    //check if stock is available
                    //    if (noOfTickets <= evt.TicketStock)
                    //    {
                    //        response.orderID = SaveTicketOrder(input);
                    //        response.status = "OK";
                    //        Session["OrderID"] = Convert.ToInt32(response.orderID);
                    //    }
                    //    else
                    //    {
                    //        response.ticket_stock = Convert.ToInt32(evt.TicketStock);
                    //        response.status = "OUT_OF_STOCK";
                    //    }
                    //}
                    //else
                    //{ 
                    #endregion

                    int totalSeatsAvailable = (int)(evt.TicketsAvailable - soldTicketsCount.Count);
                    //to check current no of tickets does not exceed thre remaining stock of tickets
                    bool isTicketCountValid = totalSeatsAvailable > noOfTickets;
                    if (isTicketCountValid)
                    {
                        if (Session["OrderID"] == null)
                        {
                            response.orderID = SaveTicketOrder(input);
                        }
                        else
                        {
                            response.orderID = UpdateTicketOrder(input);
                        }
                        response.status = "OK";
                        Session["OrderID"] = Convert.ToInt32(response.orderID);
                    }
                    else
                    {
                        //response.ticket_stock = Convert.ToInt32(evt.TicketStock);
                        //it should display this instead of above date
                        response.ticket_stock = Convert.ToInt32((evt.TicketStock - soldTicketsCount.Count));
                        response.status = "OUT_OF_STOCK";
                    }
                    return Json(response, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return GetExistingTicketOrderDetails(OrderID);
                //}
                
            }
            else
            {
                response.status = "INVALID DATA";
                response.orderID = 0;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        private JsonResult GetExistingTicketOrderDetails(int OrderID)
        {
            BookResponse response = new BookResponse();
            response.orderID = OrderID;
            response.status = "OK";
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        private int SaveTicketOrder(OrderInput input)
        {

            tblTicketOrder order = new tblTicketOrder();
            order.EventID = Convert.ToInt32(Request["eventID"]);
            order.Name = input.CustomerName;
            order.Email = input.CustomerEmail;
            order.NoOfTickets = input.NoOfTickets;
            order.AmountPerTicket = input.Price;
            order.Amount = input.TotalAmount;
            order.Status = "PENDING";
            order.PaymentStatus = "PENDING";
            order.TicketType = input.TicketType;
            order.CreatedDate = DateTime.Now;
            db.tblTicketOrders.Add(order);
            db.SaveChanges();

            return order.OrderID;
        }

        private int UpdateTicketOrder(OrderInput input)
        {
            int orderID = Convert.ToInt32(Session["OrderID"]);
            var result = db.tblTicketOrders.Where(e => e.OrderID == orderID).SingleOrDefault();
            result.EventID = Convert.ToInt32(Request["eventID"]);
            result.Name = input.CustomerName;
            result.Email = input.CustomerEmail;
            result.NoOfTickets = input.NoOfTickets;
            result.AmountPerTicket = input.Price;
            result.Amount = input.TotalAmount;
            //result.Status = "PENDING";
            //result.PaymentStatus = "PENDING";
            result.TicketType = input.TicketType;
            //result.CreatedDate = DateTime.Now;
            //db.tblTicketOrders.Add(order);
            db.SaveChanges();
            return result.OrderID;
        }
    }
}