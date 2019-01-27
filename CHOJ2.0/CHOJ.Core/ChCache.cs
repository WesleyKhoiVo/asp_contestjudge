using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace CHOJ {
	/// <summary>
	/// 缓存类 
	/// AU:邹健
	/// LE:2007 10 24
	/// </summary>
	public class CHCache {
		#region 新加属性
		///<summary>
		/// 属性
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
		/// 得到某一特定缓存
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns></returns>
		public static object Get(string key) {
			return Cache[key];
		}
		/// <summary>
		/// 得到某一特定缓存，并转为特定类型
		/// </summary>
		/// <typeparam name="T">返回的类型</typeparam>
		/// <param name="key">键值</param>
		/// <returns></returns>
		public static T Get<T>(string key) where T : class {
			return Cache[key] as T;
		}
		/// <summary>
		/// 缓存中是否包含某一键值
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns></returns>
		public static bool Contains(string key) {
			return Cache[key] != null;
		}
		/// <summary>
		/// 将对象添加到缓存中
		/// </summary>
		/// <param name="key">为对象对应的键值</param>
		/// <param name="obj">对象</param>
		public static void Add(string key, object obj) {
			Cache.Add(key, obj, null, DateTime.MaxValue
					, new TimeSpan(1, 0, 0),
					CacheItemPriority.Normal, null);
		}
		/// <summary>
		/// 将对象添加到缓存中
		/// </summary>
		/// <param name="key">为对象对应的键值</param>
		/// <param name="obj">对象</param>
		/// <param name="ts">过期时间</param>
		public static void Add(string key, object obj, TimeSpan ts) {
			Cache.Add(key, obj, null, DateTime.MaxValue
					, ts,
					CacheItemPriority.Normal, null);
		}

		#endregion




		static TimeSpan? _timespan;

		#region 缓存过期
		/// <summary>
		/// 检测缓存是否过期。
		/// </summary>
		/// <param name="CacheName">缓存名称</param>
		/// <returns>布尔型,缓存存在则返回True。</returns>
		static public bool IsNullorEmpty(String CacheName)
		{
			if (HttpContext.Current.Cache[CacheName] != null)
				return String.IsNullOrEmpty(HttpContext.Current.Cache[CacheName].ToString());
			return true;
		}

		#endregion
	
		#region 获取缓存
		/// <summary>
		/// 通过缓存名称得到缓存
		/// </summary>
		/// <param name="CacheName">缓存名称</param>
		/// <returns>Object类型</returns>
		static public object GetCache(String CacheName) {
			if(HttpContext.Current.Cache[CacheName]==null)
				return "undefine with Cache name is "+ CacheName;
			return HttpContext.Current.Cache[CacheName];
		}
		#endregion
		

		#region 移除缓存
		/// <summary>
		/// 清除所有缓存
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
		

	



		#region 属性
		/// <summary>
		/// 获取或设置缓存的存储时间,默认值为1天
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
