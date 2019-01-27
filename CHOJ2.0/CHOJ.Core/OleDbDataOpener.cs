using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace CHOJ
{
	///<summary>OleDB的DataOpener
	///</summary>
	public class OleDbDataOpener : IDataOpener
	{
		private const string s_connPrefix = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
		private readonly OleDbCommand _Command;
		private readonly OleDbConnection _Connection;
		AccessConnectionHolder holder;
		///<summary>构造函数
		///</summary>
		///<param name="filename">形如~/的地址</param>
		public OleDbDataOpener(string filename)
		{
			holder = AccessConnectionHolder.GetConnection(filename, true);
			_Connection = holder.Connection;
			_Command = new OleDbCommand();
		}

		#region 打开关闭数据库

		void IDataOpener.Open(string SQLtext)
		{
			Open(CommandType.Text, SQLtext);
		}

		public void Close()
		{
			Command.Parameters.Clear();
		}

		public DbDataAdapter GetAdapter()
		{
			//throw new Exception(Command.CommandText);
			return new OleDbDataAdapter(_Command);
		}

		public void AddWithValue(string key, object value)
		{
			_Command.Parameters.AddWithValue(key, value);
		}

		public void Dispose()
		{
			holder.Close();
		}

		private void Open(CommandType type, string text)
		{
			Command.Connection = _Connection;
			Command.CommandType = type;
			Command.CommandText = text;
			if (_Connection.State != ConnectionState.Open)
				Command.Connection.Open();
		}



		public DbCommand Command
		{
			get { return _Command; }
		}



		public IDbConnection Connection {
			get { return _Connection; }
		}

		#endregion
	}
}