using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CHOJ.OpenId;

namespace CHOJ.Controllers {

	public class HomeController : BaseController {
		public ActionResult Index() {
		    Title = "CHOJ# Home";
			return View();
		}

		public ActionResult About() {
		    Title = "About CHOJ#";
			return View();
		}

	}
}
