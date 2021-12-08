using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd.App_Start
{
    public class IocConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();
            container.RegisterType < icustomerservice, customerservice = "" > ();
            container.RegisterType < iaccountservice, accountservice = "" > ();
            container.RegisterType < ifundtransferservice, fundtransferservice = "" > ();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        public static void RegisterDependencies()
        {
            
            try
            {

                var builder = new ContainerBuilder();

                builder.RegisterControllers(Assembly.GetExecutingAssembly());
                builder.RegisterControllers(typeof(WebApiApplication).Assembly);


                // Get your HttpConfiguration.
                var config = GlobalConfiguration.Configuration;
                // Register your Web API controllers.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());



                // builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                #region UserManagement
                builder.RegisterType<UserRepository>().As<IUserRepository<User>>().InstancePerRequest();
                builder.RegisterType<UserBL>().As<IUserBL>().InstancePerRequest();

                
                #endregion

                #region Security
                
                #endregion

                #region Masters
                
                #endregion

                #region Others
               
                #endregion


                #region Parameters & Configurations
               
                #endregion

                #region Notifications
                
                #endregion


                IContainer container = builder.Build();
                DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
                config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
                //app.UseAutofacMiddleware(container);
                //app.UseAutofacWebApi(config);
                //app.UseWebApi(config);
            }
            catch (System.Exception ex)
            {


            }
        }

    }
}