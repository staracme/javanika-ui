using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class VOBARegisterResponse
    {
        public string status { get; set; }
        public string error_message { get; set; }
        public int r_id { get; set; }
    }

    public class VOBAController : Controller
    {
        // GET: VOBA
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Rules()
        {
            return View();
        }

        public JsonResult Register()
        {
            VOBARegisterResponse response = new VOBARegisterResponse();
            
            try
            {
                VOBARegistration registration = new VOBARegistration();
                registration.ChildDOB = Request["txtDOB"];
                registration.ParentEmail = Request["txtParentEmail"];
                registration.ParentMobile = Request["txtParentMobile"];
                registration.ChildName = Request["txtChildName"];
                registration.ParentName = Request["txtParentName"];
                registration.TermsAccepted = true;
                registration.PaymentStatus = "PENDING";
                registration.CreatedDate = DateTime.Now;
                db.VOBARegistrations.Add(registration);
                db.SaveChanges();

                response.r_id = registration.RegistrationID;
                response.status = "OK";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                response.status = "ERROR";
                response.error_message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}