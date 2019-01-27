using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CHOJ {
	public class MvcApplication : System.Web.HttpApplication {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
            "Wiki",
            "Note/{Title}",
            new { controller = "Wiki", action = "Details" }
            );
		    routes.MapRoute(
		        "Api",
		        "LiveId",
		        new {controller = "Api", action = "LiveId"}
		        );
			routes.MapRoute(
				"Default",                                  
				"{controller}/{action}",                    
				new { controller = "Home", action = "Index"}
			);

		}

		protected void Application_Start() {
			RegisterRoutes(RouteTable.Routes);
			Registry.Init();
		}
	}
}