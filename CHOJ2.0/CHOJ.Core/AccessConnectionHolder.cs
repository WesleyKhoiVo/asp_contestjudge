using System;
using System.Data.OleDb;
using System.Threading;
using System.Web;
using System.Collections;
using System.IO;
using System.Configuration;

namespace CHOJ {
	public sealed class AccessConnectionHolder {
		private const string s_connPrefix = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
		static private Hashtable _Connections = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
		public OleDbConnection Connection;
		private bool _Opened;
		public DateTime CreateDate;
		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="connection"></param>
		public AccessConnectionHolder(OleDbConnection connection) {
			Connection = connection;
			CreateDate = DateTime.Now;
		}
		public void Open(HttpContext context) {
			// Step 1: 获取排他锁
			Monitor.Enter(this);
			if (_Opened)
				return; // 已经打开即不做操作
			// Step 3: 打开连接
			try {
				Connection.Open();
			}
			catch {
				// 释放指定对象上的排他锁
				Monitor.Exit(this);
				throw; // re-throw the exception
			}
			_Opened = true; // 转换状态!
		}

		public void Close() {
			if (!_Opened) // Not open!
				return;
			// 关闭连接
			Connection.Close();
			_Opened = false;
			// 退出排它锁
			Monitor.Exit(this);
		}

		private static void BuildConnectionForFileName(string dbFileName) {
			/////////////////////////////////////////////
			// Step 0: Check if connection already exists
			if (_Connections[dbFileName] != null)
				return;
			/////////////////////////////////////////////
			// Step 1: Check if it is a valid connection string
			bool isConnString = false;
			OleDbConnection conn = null;

			if (dbFileName.IndexOf(';') >= 0 && dbFileName.IndexOf('=') >= 0) { // Is probably a connection string
				try {
					conn = new OleDbConnection(dbFileName);
					try {
						conn.Open();
						isConnString = true;
					}
					finally {
						conn.Close();
					}
				}
				catch {
					isConnString = false;
				}
			}

			if (isConnString) {
				_Connections.Add(dbFileName, new AccessConnectionHolder(conn));
				return;
			}

			////////////////////////////////////////////////////////////////////
			// Step 2: Check is it's a full path: use as-is, if it is a full path
			if (Path.IsPathRooted(dbFileName)) {
				EnsureValidMdbFile(dbFileName);
				_Connections.Add(dbFileName, new AccessConnectionHolder(new OleDbConnection(s_connPrefix + dbFileName)));
				return;
			}

			////////////////////////////////////////////////////////////
			// Step 3: Ensure that it doesn't try to walk up a directory
			if (dbFileName.Contains("..")) {
				throw new Exception("File name can not contain dot dot(..): " + dbFileName);
			}

			////////////////////////////////////////////////////////////
			// Step 4: Get the full path for this (relative) filename
			string filename = GetFullPathNameFromDBFileName(dbFileName);

			////////////////////////////////////////////////////////////
			// Step 5: Create and add connection
			EnsureValidMdbFile(filename);
			_Connections.Add(dbFileName, new AccessConnectionHolder(new OleDbConnection(s_connPrefix + filename)));
		}
		public static AccessConnectionHolder GetConnection(string dbFileName, bool revertImpersonation) {
			dbFileName = dbFileName.Trim();

			/////////////////////////////////////////////////
			// Lock the connections table, and see if it already exists
			lock (_Connections) {
				AccessConnectionHolder holder = (AccessConnectionHolder)_Connections[dbFileName];
				if (holder != null && !File.Exists(holder.Connection.DataSource)) {
					_Connections.Remove(dbFileName);
					holder = null;
				}
				if (holder == null) {
					BuildConnectionForFileName(dbFileName);
					holder = (AccessConnectionHolder)_Connections[dbFileName];
				}
				if (holder == null) {
					return null;
				}
				holder.Open(null);
				return holder;
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////
		private static string GetFullPathNameFromDBFileName(string relativeFileName) {
			relativeFileName = relativeFileName.Replace('/', '\\'); // replace / with \
			if (relativeFileName.StartsWith("~\\"))
				relativeFileName = relativeFileName.Substring(2);
			else if (relativeFileName.StartsWith("\\"))
				relativeFileName = relativeFileName.Substring(1);
			return Path.Combine(HttpRuntime.AppDomainAppPath, relativeFileName);
		}
		/////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////
		private static void EnsureValidMdbFile(string fileName) {
			OleDbConnection conn = null;
			try {
				conn = new OleDbConnection(s_connPrefix + fileName);
				conn.Open();
			}
			catch {
				throw new Exception("AccessFile is not valid: " + fileName);
			}
			finally {
				if (conn != null)
					conn.Close();
			}
		}

		/////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////
		public static string GetFileNameFromConnectionName(string connectionName, bool appLevel) {
			ConnectionStringSettings connObj = ConfigurationManager.ConnectionStrings[connectionName];
			if (connObj != null) {
				return connObj.ConnectionString;
			}

			return null;
		}

		/////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////
		public static Exception GetBetterException(Exception e, AccessConnectionHolder holder) {
			try {
				if (!(e is OleDbException) || holder.Connection == null ||
					holder.Connection.DataSource == null || holder.Connection.DataSource.Length < 1) {
					return e;
				}
				if (!File.Exists(holder.Connection.DataSource)) {
					return new FileNotFoundException(String.Empty, holder.Connection.DataSource, e);
				}
			}
			finally {
				if (holder.Connection != null)
					holder.Connection.Close();
			}

			FileStream s = null;
			Exception eWrite = null;
			try {
				s = File.OpenWrite(holder.Connection.DataSource);
			}
			catch (Exception except) {
				eWrite = except;
			}
			finally {
				if (s != null)
					s.Close();
			}
			if (eWrite != null && (eWrite is UnauthorizedAccessException)) {
				HttpContext context = HttpContext.Current;
				if (context != null) {
					context.Response.Clear();
					context.Response.StatusCode = 500;
					context.Response.Write("Cannot write to DB File");
					context.Response.End();
				}
				return new Exception("AccessFile is not writtable", eWrite);
			}
			return e;
		}

		public  static void CheckConnectionString(string fileName) {
			if (fileName.IndexOf(';') >= 0 && fileName.IndexOf('=') >= 0) // Is probably a connection string
				return;
			if (Path.IsPathRooted(fileName)) { // Full path
				if (!File.Exists(fileName))
					throw new Exception("$safeprojectname$ File not found: " + fileName);
				return;
			}
			char c = fileName[0];
			if (c == '/' || c == '\\') {
				throw new Exception("$safeprojectname$ File can not start with this char: " + c);
			}
			if (fileName.Contains("..")) {
				throw new Exception("File name can not contain '..': " + fileName);
			}
		}

		public static DateTime RoundToSeconds(DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
		}
	}
}
