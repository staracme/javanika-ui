using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class BookingSuccessController : Controller
    {
        // GET: OrderSuccess
        FrontEnd.Models.JAVADBEntities db = new Models.JAVADBEntities();
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            if (Request.Cookies["IsRefreshed"] != null)
                Response.Cookies["IsRefreshed"].Expires = DateTime.Now.AddDays(-1);

            int ticketID = Convert.ToInt32(Request["tID"]);

            var ticket = db.tblTicketOrders.Where(t => t.OrderID == ticketID).SingleOrDefault();
            var evt = db.tblEvents.Where(t => t.EventID == ticket.EventID).SingleOrDefault();
            var seats = db.tblSeatSelections.Where(s => s.OrderID == ticket.OrderID).ToList();

            ViewData["Event"] = evt;
            ViewData["Seats"] = seats;
            return View(ticket);
        }
    }
}