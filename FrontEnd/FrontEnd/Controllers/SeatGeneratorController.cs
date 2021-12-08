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
    public class SeatGeneratorController : Controller
    {
        // GET: SeatGenerator
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult AddSeats()
        {
            
            //int noOfSeats = Convert.ToInt32(Request["txtNoOfSeats"]);
            //int seatRowID = Convert.ToInt32(Request["txtSeatRowID"]);
            //string alphabet = Request["txtAlphabet"];

            BookResponse response = new BookResponse();

            //for(int i=0; i<= noOfSeats; i++)
            //{
            //    tblSeat seat = new tblSeat();
            //    seat.SeatNumber = alphabet + "" + i;
            //    seat.SeatRowID = seatRowID;
            //    seat.Status = ""
            //}

            
            //    tbl order = new tblTicketOrder();
            //    order.EventID = Convert.ToInt32(Request["eventID"]);
            //    order.Name = name;
            //    order.Email = email;
            //    order.NoOfTickets = noOfTickets;
            //    order.AmountPerTicket = price;
            //    order.Amount = totalAmount;
            //    order.Status = "PENDING";
            //    order.PaymentStatus = "PENDING";
            //    order.CreatedDate = DateTime.Now;
            //    db.tblTicketOrders.Add(order);
            //    db.SaveChanges();

            //    response.status = "OK";
            //    response.orderID = order.OrderID;

            
                return Json(response, JsonRequestBehavior.AllowGet);
            
        }
    }
}