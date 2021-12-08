using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FrontEnd.Controllers
{
    public class TestReceiptController : Controller
    {
        // GET: TestReceipt
        public ActionResult Index()
        {
            string Receipt = System.IO.File.ReadAllText(Server.MapPath("~/Templates/receiptseats.html"));
            Receipt = Receipt.Replace("[display_type]","style=" + "visibility:hidden");


            ViewData["Html"] = Receipt;
            return View();
        }
    }
}