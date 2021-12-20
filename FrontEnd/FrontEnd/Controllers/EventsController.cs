using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using FrontEnd.Utility;
using SA.Caching.Helpers;

namespace FrontEnd.Controllers
{
    public class EventsController : Controller
    {
        // GET: Events
        JAVADBEntities db = new JAVADBEntities();
        private readonly int _cacheTimeInHours;
        private bool IsCacheToReferesh;
        public EventsController()
        {
            IsCacheToReferesh = false;
            if (Session != null)
            {
                IsCacheToReferesh = (Convert.ToString(Session["IsCacheToReferesh"]) == null) ? false : (bool)Session["IsCacheToReferesh"];
            }
            _cacheTimeInHours = Convert.ToInt32(ConfigurationManager.AppSettings["cacheTimeInHours"]);
        }

        public ActionResult Index()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            var events = getEventsFromCache(eventID, IsCacheToReferesh); //db.tblEvents.Where(t => t.EventID == eventID).SingleOrDefault();
            var artists = getEventArtistsFromCache(eventID); //db.tblEventArtists.Where(a => a.EventID == eventID).ToList();
            ViewData["Artists"] = artists;
            return View(events);
        }

        #region Caching Functions
        private object getEventsFromCache(int eventID, bool IsCacheToReferesh)
        {
            string ckKey = "cvEvent_" + eventID.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object oData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                oData = db.tblEvents.Where(t => t.EventID == eventID).SingleOrDefault(); ;
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, oData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                oData = cvKey;
            }
            return oData;
        }
        private object getEventArtistsFromCache(int eventID)
        {
            string ckKey = "cvEventArtists_" + eventID.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object oData = null;
            if (MemoryCacher.GetValue(ckKey) == null)
            {
                oData = db.tblEventArtists.Where(a => a.EventID == eventID).ToList();
                MemoryCacher.Add(ckKey, oData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                oData = cvKey;
            }
            return oData;
        }
        #endregion
    }
}