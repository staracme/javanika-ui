using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class RegisterResponse
    {
        public string status { get; set; }
        public bool? mobile { get; set; }
        public bool? email { get; set; }
    }

    public class RegisterController : Controller
    {
        // GET: Register
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Register()
        {
            RegisterResponse response = new RegisterResponse();

            try
            {
                string req_mobile = Request["txtMobile"];
                string req_email = Request["txtEmail"];

                bool mobile_exists = db.tblCustomers.Where(t => t.Mobile == req_mobile).Any();
                bool email_exists = db.tblCustomers.Where(t => t.Email == req_email).Any();
                
                if (mobile_exists == false && email_exists == false)
                {
                    tblCustomer customer = new tblCustomer();
                    customer.FirstName = Request["txtFirstName"];
                    customer.LastName = Request["txtLastName"];
                    customer.Mobile = Request["txtMobile"];
                    customer.Password = Request["txtRegPassword"];
                    customer.Email = Request["txtEmail"];
                    customer.CreatedDate = DateTime.Now;
                    db.tblCustomers.Add(customer);
                    db.SaveChanges();

                    response.mobile = false;
                    response.email = false;
                    response.status = "OK";
                }
                else
                {
                    response.mobile = mobile_exists;
                    response.email = email_exists;
                    response.status = "EXISTS";
                }
            }
            catch (Exception ex)
            {
                response.mobile = null;
                response.email = null;
                response.status = "ERROR";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsAccountExists()
        {
            string req_mobile = Request["mobile"];
            string req_email = Request["email"];

            bool mobile = db.tblCustomers.Where(t => t.Mobile == req_mobile).Any();
            bool email = db.tblCustomers.Where(t => t.Email == req_email).Any();

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mobile", mobile.ToString());
            data.Add("email", email.ToString());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}