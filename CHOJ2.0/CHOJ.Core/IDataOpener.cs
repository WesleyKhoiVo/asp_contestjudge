using System.Data;
using System.Data.Common;
namespace CHOJ {
	///<summary>
	///数据库的OpenEr接口
	///come from chsns#
	///</summary>
	public interface IDataOpener {
		///<summary>Connection对象
		///</summary>
		IDbConnection Connection { get; }
		///<summary>Command对象
		///</summary>
		DbCommand Command { get; }
		///<summary>打开数据库连接
		///</summary>
		///<param name="SQLtext"></param>
		void Open(string SQLtext);
		///<summary>关闭Command
		///</summary>
		void Close();
		///<summary>得到Adapter
		///</summary>
		///<returns></returns>
		DbDataAdapter GetAdapter();
		///<summary>添加参数
		///</summary>
		///<param name="key"></param>
		///<param name="value"></param>
		void AddWithValue(string key,object value);
		///<summary>销毁对象
		///</summary>
		void Dispose();
	}
}
