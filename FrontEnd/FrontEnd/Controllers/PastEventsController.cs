using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class PastEventModel
    {
        public string EventName { get; set; }
    }

    public class PastEventsController : Controller
    {
        // GET: PastEvents
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            return View(db.tblPastEvents.OrderByDescending(s=>s.PastEventID).ToList());
        }
    }
}