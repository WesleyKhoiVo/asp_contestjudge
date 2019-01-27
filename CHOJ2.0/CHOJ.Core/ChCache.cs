using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace CHOJ {
	/// <summary>
	/// ������ 
	/// AU:�޽�
	/// LE:2007 10 24
	/// </summary>
	public class CHCache {
		#region �¼�����
		///<summary>
		/// ����
		///</summary>
		protected static Cache Cache {
			get {
				return HttpContext.Current.Cache;
			}
		}
		///<summary>
		///</summary>
		///<param name="key"></param>
		public object this[string key]
		{
			get { return Get(key); }
		}

		/// <summary>
		/// �õ�ĳһ�ض�����
		/// </summary>
		/// <param name="key">��ֵ</param>
		/// <returns></returns>
		public static object Get(string key) {
			return Cache[key];
		}
		/// <summary>
		/// �õ�ĳһ�ض����棬��תΪ�ض�����
		/// </summary>
		/// <typeparam name="T">���ص�����</typeparam>
		/// <param name="key">��ֵ</param>
		/// <returns></returns>
		public static T Get<T>(string key) where T : class {
			return Cache[key] as T;
		}
		/// <summary>
		/// �������Ƿ����ĳһ��ֵ
		/// </summary>
		/// <param name="key">��ֵ</param>
		/// <returns></returns>
		public static bool Contains(string key) {
			return Cache[key] != null;
		}
		/// <summary>
		/// ���������ӵ�������
		/// </summary>
		/// <param name="key">Ϊ�����Ӧ�ļ�ֵ</param>
		/// <param name="obj">����</param>
		public static void Add(string key, object obj) {
			Cache.Add(key, obj, null, DateTime.MaxValue
					, new TimeSpan(1, 0, 0),
					CacheItemPriority.Normal, null);
		}
		/// <summary>
		/// ���������ӵ�������
		/// </summary>
		/// <param name="key">Ϊ�����Ӧ�ļ�ֵ</param>
		/// <param name="obj">����</param>
		/// <param name="ts">����ʱ��</param>
		public static void Add(string key, object obj, TimeSpan ts) {
			Cache.Add(key, obj, null, DateTime.MaxValue
					, ts,
					CacheItemPriority.Normal, null);
		}

		#endregion




		static TimeSpan? _timespan;

		#region �������
		/// <summary>
		/// ��⻺���Ƿ���ڡ�
		/// </summary>
		/// <param name="CacheName">��������</param>
		/// <returns>������,��������򷵻�True��</returns>
		static public bool IsNullorEmpty(String CacheName)
		{
			if (HttpContext.Current.Cache[CacheName] != null)
				return String.IsNullOrEmpty(HttpContext.Current.Cache[CacheName].ToString());
			return true;
		}

		#endregion
	
		#region ��ȡ����
		/// <summary>
		/// ͨ���������Ƶõ�����
		/// </summary>
		/// <param name="CacheName">��������</param>
		/// <returns>Object����</returns>
		static public object GetCache(String CacheName) {
			if(HttpContext.Current.Cache[CacheName]==null)
				return "undefine with Cache name is "+ CacheName;
			return HttpContext.Current.Cache[CacheName];
		}
		#endregion
		

		#region �Ƴ�����
		/// <summary>
		/// ������л���
		/// </summary>
		static public void RemoveAll() {
			foreach (DictionaryEntry de in Cache)
			{
				Cache.Remove(de.Key.ToString());
			}
		}
		///<summary>
		///</summary>
		///<param name="key"></param>
		static public void Remove(string key) {
			Cache.Remove(key);
		}
		#endregion
		

	



		#region ����
		/// <summary>
		/// ��ȡ�����û���Ĵ洢ʱ��,Ĭ��ֵΪ1��
		/// </summary>
		static public TimeSpan Timespan {
			get
			{
				if(_timespan==null){
					return new TimeSpan(1, 0, 0, 0, 0);
				}
				return _timespan.Value;
			}
			set { _timespan = value; }
		}


#endregion

	}
}