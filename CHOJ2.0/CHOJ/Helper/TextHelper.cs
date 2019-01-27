using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CHOJ {
	static public class TextHelper {
		static public string Status(this HtmlHelper Html, object obj,object guid) {			
			string n=Enum.GetName(typeof(AnswerType),obj);
			AnswerType at = (AnswerType)Enum.Parse(typeof(AnswerType), n, true);
			if (at == AnswerType.CompileError)
				n= string.Format("<a href='/CompilerInfo/{0}.txt'>{1}</a>", guid, n);
			if(at==AnswerType.Accepted)
				n = string.Format("<span class='red'>{0}</span>", n);
			return n;
		}
	}
}
