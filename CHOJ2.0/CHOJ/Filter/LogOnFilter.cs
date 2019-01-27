using System;
using System.Web.Mvc;

namespace CHOJ
{
    public class LoginedFilter : ActionFilterAttribute
    {
        public string Role { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var url = new UrlHelper(filterContext.RequestContext);
            if(!Role.IsNullOrEmpty())
            {
                if(HalfoxUser.Role!=Role)
                    filterContext.HttpContext.Response.Redirect(url.Action("Login","Account"));
            }
            if (!HalfoxUser.HasLogOn)
            {
                filterContext.HttpContext.Response.Redirect(url.Action("Login", "Account"));
            }
        }
    }
}