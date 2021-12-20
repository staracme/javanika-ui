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
    public class HomeController : Controller
    {
        // GET: Home
        JAVADBEntities db = new JAVADBEntities();
        private readonly int _cacheTimeInHours;
        public HomeController()
        {
            _cacheTimeInHours = Convert.ToInt32(ConfigurationManager.AppSettings["cacheTimeInHours"]);
        }

        public ActionResult Index()
        {
            //If cookie is set then utilize
            if (Request.Cookies["Session"] != null && Request.Cookies["Session"]["SessionID"] != "expired")
               Response.Cookies["Session"]["SessionID"] = Request.Cookies["Session"]["SessionID"];
            else
               Response.Cookies["Session"]["SessionID"] = Session.SessionID;

            Response.Cookies["Session"].Expires = DateTime.Now.AddDays(3);

            if (Request.Cookies["IsRefreshed"] != null)
                Response.Cookies["IsRefreshed"].Expires = DateTime.Now.AddDays(-1);

            var IsCacheToReferesh = db.tblConfigs.Where(x => x.ConfigID == 1).SingleOrDefault().IsCacheToReferesh;
            if ((bool)IsCacheToReferesh)
            {
                var config = db.tblConfigs.Where(x => x.ConfigID == 1).SingleOrDefault();
                config.IsCacheToReferesh = false;
                db.SaveChanges();
            }
            Session["IsCacheToReferesh"] = IsCacheToReferesh;

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
                if (IsCacheToReferesh) {
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