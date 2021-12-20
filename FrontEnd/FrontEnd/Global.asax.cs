using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FrontEnd.Infra;
using log4net;
using SA.LA;

namespace FrontEnd
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            AutoMapperWebProfile.Run();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //UnityConfig.RegisterComponents();
            log4net.Config.XmlConfigurator.Configure();
        }

        protected void Session_Start()
        {
            Session["Session_Start"] = $"Session_Start {DateTime.Now}";
            
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Booking Summary Controller",
                    Source = "SMTP Email()",
                    Message = exception.Message,
                    Exception = exception
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }
        }
    }
}
