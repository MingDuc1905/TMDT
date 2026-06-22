using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.SqlServer.Types;


namespace ShipFood
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Empty - test minimal startup
        }

        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                string message = "Error: " + ex.Message;
                if (ex.InnerException != null)
                    message += " | Inner: " + ex.InnerException.Message;
                
                try
                {
                    System.IO.File.AppendAllText(
                        Server.MapPath("~/App_Data/errors.log"),
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine
                    );
                }
                catch { }
            }
        }
    }
}
