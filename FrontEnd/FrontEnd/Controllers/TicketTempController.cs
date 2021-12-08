using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class TicketTempController : Controller
    {
        // GET: TicketTemp
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            int ticketID = Convert.ToInt32(Request["tID"]);

            var ticket = db.tblTickets.Where(t => t.TicketID == ticketID).SingleOrDefault();
            var evt = db.tblEvents.Where(t => t.EventID == ticket.EventID).SingleOrDefault();
            var seats = db.tblSeatSelections.Where(s => s.OrderID == ticket.TicketID).ToList();

            ViewData["Event"] = evt;
            ViewData["Seats"] = seats;
            return View();
        }
    }
}