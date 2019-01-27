using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CHOJ {
	/// <summary>
	/// 数据库执行者
	/// the Executor of DataBase
	/// </summary>
	public class DataBaseExecutor : IDisposable {
		///<summary>打开者
		///</summary>
		public IDataOpener DataOpener { get; set; }
		/// <summary>
		/// init
		/// </summary>
		/// <param name="dataopen">IDataOpener</param>
		public DataBaseExecutor(IDataOpener dataopen) {
			DataOpener = dataopen;
		}


		///<summary>
		/// 直接执行Sql语句
		///</summary>
		///<param name="sqltext"></param>
		public int Execute(string sqltext) {
			return Execute(sqltext, new Dictionary());
		}

		///<summary>
		///  直接执行Sql语句
		///</summary>
		///<param name="sqltext"></param>
		///<param name="dict">参数</param>
		public int Execute(string sqltext, Dictionary dict) {
			DataOpener.Open(sqltext);
			foreach (string key in dict.Keys) {
				DataOpener.AddWithValue(key, dict[key]);
			}
			ConnectionState previousConnectionState = DataOpener.Command.Connection.State;

			if (previousConnectionState == ConnectionState.Closed)
				DataOpener.Command.Connection.Open();
			int ret =DataOpener.Command.ExecuteNonQuery(); ;
			DataOpener.Close();
			return ret;
		}

		///<summary>执行Sql语句并返回受影响的行数
		/// 多进行插入删除更新等工作
		///</summary>
		///<param name="sqltext">Sql语句</param>
		///<param name="args">参数</param>
		///<returns>返回受影响的行数</returns>
		public int Execute(string sqltext, params object[] args) {
			return Execute(sqltext, Dictionary.CreateFromArgs(args));
		}

		public object ExecuteScalar(string sqltext) {
			return ExecuteScalar(sqltext, new Dictionary());
		}

		public object ExecuteScalar(string sqltext, Dictionary dict) {
			DataOpener.Open(sqltext);
			foreach (string key in dict.Keys) {
				DataOpener.AddWithValue(key, dict[key]);
			}
			object ret = DataOpener.Command.ExecuteScalar();
			DataOpener.Close();
			return ret;
		}

		public object ExecuteScalar(string name, params object[] args) {
			return ExecuteScalar(name, Dictionary.CreateFromArgs(args));
		}


		public DataTable GetTable(string name) {
			return GetTable(name, new Dictionary());
		}
		public DataRowCollection GetRows(string text) {
			return GetTable(text).Rows;
		}
		public DataTable GetTable(string name, Dictionary dict) {
			DataOpener.Open(name);
			foreach (string key in dict.Keys) {
				DataOpener.AddWithValue(key, dict[key]);
			}
			var dt = new DataTable();
			DbDataAdapter adp = DataOpener.GetAdapter();
			adp.Fill(dt);
			DataOpener.Close();
			return dt;
		}
		public DataRowCollection GetRows(string text, Dictionary dict) {
			return GetTable(text, dict).Rows;
		}
		public DataTable GetTable(string name, params object[] args) {
			return GetTable(name, Dictionary.CreateFromArgs(args));
		}
		public DataRowCollection GetRows(string text, params object[] args) {
			return GetTable(text, args).Rows;
		}
		public void Dispose() {
			DataOpener.Dispose();
		}

		#region GetValue
		public String GetValue(String text, IDictionary<string, object> dict) {
			DataOpener.Open(text);
			DbDataReader reader;
			foreach (string key in dict.Keys) {
				DataOpener.AddWithValue(key, dict[key]);
			}
			DataOpener.Command.Parameters["@RETURN_VALUE"].Direction = ParameterDirection.ReturnValue;
			reader = DataOpener.Command.ExecuteReader();
			string result = DataOpener.Command.Parameters["@RETURN_VALUE"].Value.ToString();
			DataOpener.Close();
			return result;
		}
		public String GetValue(String text, params object[] args) {
			return GetValue(text, Dictionary.CreateFromArgs(args));
		}
		public String GetValue(String text) {
			return GetValue(text,new Dictionary());
		}
		#endregion
	}
}
