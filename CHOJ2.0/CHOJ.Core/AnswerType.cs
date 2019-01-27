using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHOJ {
	public enum AnswerType {
		/// <summary>
		/// 排队中。
		/// </summary>
		Queuing = 0,
		/// <summary>
		/// 正在编译。
		/// </summary>
		Compiling = 20,
		/// <summary>
		/// 测试中。
		/// </summary>
		Testing = 30,
		/// <summary>
		/// 超时。
		/// </summary>
		TimeLimitExceed = 40,
		/// <summary>
		/// 测试失败。
		/// </summary>
		WrongAnswer = 50,
		/// <summary>
		/// 内存超出限制。
		/// </summary>
		MemoryLimitExceed = 60,
		/// <summary>
		/// 编译失败。
		/// </summary>
		CompileError = 70,
		/// <summary>
		/// 危险代码。
		/// </summary>
		DangerCode = 80,
		/// <summary>
		/// 运行时错误
		/// </summary>
		RunningError = 90,
		/// <summary>
		/// 测试通过。
		/// </summary>
		Accepted = 250,
		
	}
}
