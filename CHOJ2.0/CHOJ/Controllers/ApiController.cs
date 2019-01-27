using System;
using System.Web.Mvc;
using CHOJ.OpenId;
using CHOJ.Service;

namespace CHOJ.Controllers
{
    public class ApiController : Controller
    {
        static readonly WindowsLiveLogin Wll = new WindowsLiveLogin(true);
        static readonly DateTime ExpireCookie = DateTime.Now.AddYears(-10);
        static readonly DateTime PersistCookie = DateTime.Now.AddYears(10);

        public ActionResult Logout()
        {
            HalfoxUser.Clear();
            return Redirect(Wll.GetLogoutUrl());
        }

        public ActionResult LiveId(string action, string stoken)
        {
            
            var user = Wll.ProcessLogin(Request.Form);
            if (user != null)
            {
                HalfoxUser.Value = user.Token;
                HalfoxUser.OpenId = user.Id;
                HalfoxUser.IdType = "live";
                var currentProfile = ProfileService.GetInstance().Get();
                if (currentProfile != null)
                {
                    HalfoxUser.Name = currentProfile.NickName ?? "Please setting your profile.";
                    HalfoxUser.Role = currentProfile.Role;
                    HalfoxUser.Id = currentProfile.Id;
                }
                if (user.UsePersistentCookie)
                {
                    HalfoxUser.Expires = PersistCookie;
                }
            }
            else
            {
                HalfoxUser.Expires = ExpireCookie;
            }
            return RedirectToAction("Index", "home");
        }
    }
}
