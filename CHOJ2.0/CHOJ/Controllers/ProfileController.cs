using System.Web.Mvc;
using CHOJ.Models;
using CHOJ.Service;

namespace CHOJ.Controllers
{
    [LoginedFilter]
    public class ProfileController : BaseController
    {
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {

            var model = ProfileService.GetInstance().Get() ?? new Profile();
            SetTitle(model);
            return View(model);
        }
        void SetTitle(Profile model)
        {
            Title = " Profile of " + (model.NickName.IsNullOrEmpty() ? "Me" : model.NickName);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(Profile profile)
        {
            if (profile.NickName.IsNullOrEmpty())
            {
                ModelState.AddModelError("NickName", "Nick Name is must.");
            }
            if (ViewData.ModelState.IsValid)
            {
                ProfileService.GetInstance().Update(profile);
            }
            SetTitle(profile);
            return View(profile);
        }

        public ActionResult RankList()
        {
           var model= ProfileService.GetInstance().RankList(100);
           Title = "Rank List";
           return View(model);
        }
    }
}
