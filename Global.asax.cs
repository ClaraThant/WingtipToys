using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
//below two lines is for initializing the DB and creating the tables in it
using System.Data.Entity;
using WingtipToys.Models;

namespace WingtipToys
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //setting a rule to initialize the db when the product context is first used
            Database.SetInitializer(new ProductDatabaseInitializer());
            // TEMPORARY - forces EF to create and seed the database now
            using (var db = new ProductContext())
            {
                db.Database.Initialize(force: false);
            }

        }
    }
}