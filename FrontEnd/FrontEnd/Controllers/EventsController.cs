using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using FrontEnd.Utility;

namespace FrontEnd.Controllers
{
    public class EventsController : Controller
    {
        // GET: Events
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            var events = db.tblEvents.Where(t => t.EventID == eventID).SingleOrDefault();
            var artists = db.tblEventArtists.Where(a => a.EventID == eventID).ToList();

            ViewData["Artists"] = artists;
            //Log.Info("Fetching events started...");
           
            return View(events);
        }
    }
}