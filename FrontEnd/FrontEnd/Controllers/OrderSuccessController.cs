using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FrontEnd.Controllers
{
    public class OrderSuccessController : Controller
    {
        // GET: OrderSuccess
        FrontEnd.Models.JAVADBEntities db = new Models.JAVADBEntities();
        public ActionResult Index()
        {
            int orderID = Convert.ToInt32(Request["orderID"]);

            return View(db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault());
        }
    }
}