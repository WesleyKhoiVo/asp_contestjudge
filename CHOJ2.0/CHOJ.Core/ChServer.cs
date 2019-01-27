using System.Web;

namespace CHOJ
{
	/// <summary>
	/// 封装系统的HttpServerUtility类
	/// </summary>
	public class CHServer
	{
		/// <summary>
		/// Maps the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string MapPath(string path)
		{
			return HttpContext.Current.Server.MapPath(path);
		}


		/// <summary>
		/// URLs the encode.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string UrlEncode(string s)
		{
			return HttpUtility.UrlEncode(s);
		}

		/// <summary>
		/// URLs the encode.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string UrlEncode(object s)
		{
			return HttpUtility.UrlEncode(s.ToString());
		}

		/// <summary>
		/// URLs the decode.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string UrlDecode(string s)
		{
			return HttpUtility.UrlDecode(s);
		}

		/// <summary>
		/// HTMLs the encode.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string HtmlEncode(string s)
		{
			return HttpUtility.HtmlEncode(s);
		}

		/// <summary>
		/// HTMLs the decode.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string HtmlDecode(string s)
		{
			return HttpUtility.HtmlDecode(s);
		}
	}
}