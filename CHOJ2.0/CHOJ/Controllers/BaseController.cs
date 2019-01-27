using System.Web.Mvc;
using System.Configuration;

namespace CHOJ.Controllers {
	[HandleError]
	abstract public class BaseController : Controller {
		protected void InitIntPage(ref int? x) {
			if (!x.HasValue) x = 1;
			if (x.Value == 0) x = 1;
		}
		DataBaseExecutor _db;
		protected DataBaseExecutor Db {
			get {
				if (_db == null) _db = new DataBaseExecutor(new OleDbDataOpener(ConfigurationManager.ConnectionStrings["AccessFileName"].ConnectionString));
				return _db;
			}
		}

	    public string Title { set { ViewData["Page_Title"] = value;}
	    }  
	}
}
