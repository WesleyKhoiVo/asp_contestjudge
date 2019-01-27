using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHOJ {
	/// <summary>
	/// 编译器
	/// </summary>
	public class Compiler {
		/// <summary>
		/// 唯一标识
		/// </summary>
		public Guid Guid { get; set; }
		/// <summary>
		/// 编译器名
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 编译器类型
		/// </summary>
		public string Language { get; set; }
		/// <summary>
		/// 时间的倍数,如.net基本为10倍时间
		/// </summary>
		public double Level { get; set; }
		/// <summary>
		/// 高危代码
		/// </summary>
		public List<string> DangerCode { get; set; }
	}
}
