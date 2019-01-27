using System;
using System.Linq;
using System.Web;

namespace CHOJ
{
    public class HalfoxUser
    {
        const string Cookiekey = "chojcki";
        static readonly DateTime ExpireCookie = DateTime.Now.AddYears(-10);
        //  public static string UserId { get; set; }
        #region cookie

        static public Boolean IsExists
        {
            get
            {
                if (!HttpContext.Current.Request.Cookies.AllKeys.Contains(Cookiekey))
                    return false;
                return RequestCookie != null;
            }
        }

        static HttpCookie RequestCookie
        {
            get { return HttpContext.Current.Request.Cookies[Cookiekey]; }
        }

        static HttpCookie ResponseCookie
        {
            get
            {
                if (HttpContext.Current.Response.Cookies[Cookiekey] == null || HttpContext.Current.Response.Cookies[Cookiekey].Values.Count == 0)

                    if (RequestCookie != null)
                        HttpContext.Current.Response.Cookies.Set(RequestCookie);
                    else
                        HttpContext.Current.Response.Cookies.Add(new HttpCookie(Cookiekey));

                return HttpContext.Current.Response.Cookies[Cookiekey];
            }
            set
            {
                value.Name = Cookiekey;
                HttpContext.Current.Response.Cookies.Add(value);
            }
        }

        static string GetCookieItem(string field)
        {
            return HttpUtility.UrlDecode(!IsExists ? "" : RequestCookie[field] ?? "");
        }

        static void SetCookieItem(string field, string value)
        {
            ResponseCookie[field] = HttpUtility.UrlEncode(value);
        }

        ///<summary>清理Cookie
        ///</summary>
       static  public void Clear()
        {
            ResponseCookie = new HttpCookie(Cookiekey) { Expires = ExpireCookie };
        }

        #endregion
       static public bool HasLogOn
        {
            get { return !string.IsNullOrEmpty(OpenId); }
        }

        static public string Id
        {
            get
            {
                return GetCookieItem("id");
            }
            set
            {
                SetCookieItem("id", value);
            }
        }
        static public string OpenId
        {
            get
            {
                return GetCookieItem("OpenId");
            }
            set
            {
                SetCookieItem("OpenId", value);
            }
        }
        static public string IdType
        {
            get
            {
                return GetCookieItem("IdType");
            }
            set
            {
                SetCookieItem("IdType", value);
            }
        }
        static public string Value
        {
            get
            {
                return GetCookieItem("Value");
            }
            set
            {
                SetCookieItem("Value", value);
            }
        }
        static public string Name
        {
            get
            {
                return GetCookieItem("Name");
            }
            set
            {
                SetCookieItem("Name", value);
            }
        }
        static public string Role
        {
            get { return GetCookieItem("Role"); }
            set { SetCookieItem("Role", value); }
        }
        /// <summary>
        /// 用户Cookie信息过期限 DateTime.Now.AddDays(365);
        /// </summary>
        static public DateTime Expires
        {
            set
            {
                ResponseCookie.Expires = value;
            }
        }


    }
}