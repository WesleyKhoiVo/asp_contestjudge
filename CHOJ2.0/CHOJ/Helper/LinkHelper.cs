using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Web.Mvc.Html;
namespace CHOJ {
	static public class LinkHelper {
		public static string QuestionList(this HtmlHelper html, string text,object id) {
			return html.ActionLink(text, "List", "Question", new { id }, null);
		}
		public static string QuestionLink(this HtmlHelper html, string text, object id) {
			return html.ActionLink(text, "Index", "Question", new { id }, null);
		}
		public static string SubmitLink(this HtmlHelper html, string text, object id) {
			return html.ActionLink(text, "Submit", "Answer", new { id }, null);
		}
		public static string GetRatio(this  HtmlHelper html, object fz, object fm) {
			int x;
			if(fm.ToString()=="0")
				x=0;
			else
				x=(int) ((Convert.ToDouble(fz) * 100) / Convert.ToDouble(fm));
			return string.Format("{0}%", x);
		}
		/// <summary>
		/// 对Question进行格式化。
		/// </summary>
		/// <param name="html"></param>
		/// <param name="text">Question，为一个字符串。</param>
		/// <returns>格式化后的Question。</returns>
		public static String QuestionFormat(this HtmlHelper html, String text) {
			text = text.Replace("<", "&lt;");
			text = text.Replace(">", "&gt;");
			text = Regex.Replace(text, @"[\n\r]+input[\n\r]+", "<br /><strong>Input</strong><br />", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"[\n\r]+output[\n\r]+", "<br /><strong>Output</strong><br />", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"[\n\r]+sample[\s+]input[\n\r]+", "<br /><strong>Sample Input</strong><br />", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"[\n\r]+sample[\s+]output[\n\r]+", "<br /><strong>Sample Output</strong><br />", RegexOptions.IgnoreCase);
			text = text.Replace("\r\n", "<br/>");
			text = text.Replace("\r", "<br/>");
			text = text.Replace("\n", "<br/>");
			return text;
		}
	}
}
