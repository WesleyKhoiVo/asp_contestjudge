using System.Web.Mvc;
using CHOJ.Service;
using CHOJ.Models;

namespace CHOJ.Controllers {
	public class GroupController : BaseController {
		public ActionResult List() {
		    Title = "Category List";
            var ret = GroupService.GetInstance().GroupList();
			return View(ret);
		}
        [LoginedFilter(Role="Admin")]
        public ActionResult Management()
        {
            Title = "Category Management";
            var model = GroupService.GetInstance().GroupList();
            return View(model);
        }
        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(Group group)
        {
            Title = "Create a Category";
            GroupService.GetInstance().Add(group);
            return RedirectToAction("Management");
        }

        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Delete(string id)
        {
            GroupService.GetInstance().Delete(id);
            return RedirectToAction("Management");
        }
	}
}
