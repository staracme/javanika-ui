using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;


namespace FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        JAVADBEntities db = new JAVADBEntities();

        public ActionResult Index()
        {
            

            //Log.Info("bb");

            //If cookie is set then utilize
            if (Request.Cookies["Session"] != null && Request.Cookies["Session"]["SessionID"] != "expired")
               Response.Cookies["Session"]["SessionID"] = Request.Cookies["Session"]["SessionID"];
            else
               Response.Cookies["Session"]["SessionID"] = Session.SessionID;


            Response.Cookies["Session"].Expires = DateTime.Now.AddDays(3);

            if (Request.Cookies["IsRefreshed"] != null)
                Response.Cookies["IsRefreshed"].Expires = DateTime.Now.AddDays(-1);


            ViewData["PastEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e=>e.EventID).ToList();
            ViewData["UpcomingEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();
            ViewData["CurrentEvents"] = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList();


            return View();
        }
    }
}