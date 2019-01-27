using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHOJ {
	public class OJException : Exception {
		public OJException(string message) : base(message) {
			message = string.Format("发生错误:{0}", message);
		}
	}
}
