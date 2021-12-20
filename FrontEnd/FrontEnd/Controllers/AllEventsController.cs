using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FrontEnd.Models;
using SA.Caching.Helpers;

namespace FrontEnd.Controllers
{
    public class AllEventsController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();
        private readonly int _cacheTimeInHours;
        private bool IsCacheToReferesh;
        public AllEventsController()
        {
            IsCacheToReferesh = false;
            if (Session != null)
            {
                IsCacheToReferesh = (Convert.ToString(Session["IsCacheToReferesh"]) == null) ? false : (bool)Session["IsCacheToReferesh"];
            }
            _cacheTimeInHours = Convert.ToInt32(ConfigurationManager.AppSettings["cacheTimeInHours"]);
        }
        // GET: AllEvents
        public ActionResult Index()
        {
            //ViewData["PastEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            //ViewData["UpcomingEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            //ViewData["CurrentEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();

            ViewData["PastEvents"] = getPastEventsFromCache((bool)IsCacheToReferesh);// db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e=>e.EventID).ToList();
            ViewData["UpcomingEvents"] = getUpcomingEventsFromCache((bool)IsCacheToReferesh); //db.tblEvents.Where(t => t.Status == "a" && t.EventDate >= System.DateTime.Today).OrderByDescending(e => e.EventID).ToList();
            ViewData["CurrentEvents"] = getCurrentEventsFromCache((bool)IsCacheToReferesh); //db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();

            return View();
        }

        #region Caching Functions
        private object getPastEventsFromCache(bool IsCacheToReferesh)
        {
            string ckKey = "cvPastEvents_" + System.DateTime.Today.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object pastEventsData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                pastEventsData = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, pastEventsData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                pastEventsData = cvKey;
            }
            return pastEventsData;
        }
        private object getUpcomingEventsFromCache(bool IsCacheToReferesh)
        {
            string ckKey = "cvUpcomingEvents_" + System.DateTime.Today.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object upcomingEventsData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                upcomingEventsData = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, upcomingEventsData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                upcomingEventsData = cvKey;
            }
            return upcomingEventsData;
        }
        private object getCurrentEventsFromCache(bool IsCacheToReferesh)
        {
            string ckKey = "cvCurrentEvents_" + System.DateTime.Today.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object upcomingEventsData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                //query should be checked for currentevents
                //upcomingEventsData = db.tblEvents.Where(t => t.Status == "a" && t.EventDate >= System.DateTime.Today.AddDays(15)).OrderByDescending(e => e.EventID).ToList();
                upcomingEventsData = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, upcomingEventsData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                upcomingEventsData = cvKey;
            }
            return upcomingEventsData;
        }
        #endregion
    }
}