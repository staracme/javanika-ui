using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
namespace FrontEnd.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public string Login()
        {
            JAVADBEntities db = new JAVADBEntities();

            string username = Request["Username"];
            string password = Request["Password"];

            var customer = db.tblCustomers.Where(t => (t.Mobile == username || t.Email == username)  && t.Password == password).SingleOrDefault();

            if(customer != null)
            {
                Response.Cookies["Customer"]["CustomerID"] = customer.CustomerID.ToString();
                Response.Cookies["Customer"]["Name"] = customer.FirstName;

                return "OK";
            }
            else
            {
                return "N";
            }
        }

        public void Logout()
        {
            if(Request.Cookies["Customer"] != null && Request.Cookies["Customer"]["CustomerID"] != null)
            {
                Response.Cookies["Customer"].Expires = DateTime.Now.AddDays(-1);

                Response.Redirect("/Home");
            }
        }
    }
}