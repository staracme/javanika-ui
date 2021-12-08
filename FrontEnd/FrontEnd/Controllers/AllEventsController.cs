using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class AllEventsController : Controller
    {
        // GET: AllEvents
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            ViewData["PastEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            ViewData["UpcomingEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            ViewData["CurrentEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            return View();
        }
    }
}