using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;
namespace CHOJ
{
	class XmlSerializer
	{
		/// <summary>
		/// 序列化到配置文件　
		/// </summary>
		/// <typeparam name="T">序列化此类型</typeparam>
		/// <param name="obj">要序列化的对象</param>
		/// <param name="fn">键值</param>
		public static void Save<T>(T obj, string fn) where T : class
		{
			try
			{
				var mySerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
				using (var myWriter = new StreamWriter(CHServer.MapPath(fn)))
				{
					mySerializer.Serialize(myWriter, obj);
				}
			}
			catch
			{
				throw new Exception(string.Format("存储配置文件{0}时出错,编号:{1}", fn, 10359));
			}
		}
		public static void Clear(string fn) { CHCache.Remove(fn); }
		/// <summary>
		/// 从配置文件反序列化
		/// </summary>
		/// <typeparam name="T">反序列化的目标类型</typeparam>
		/// <param name="fn">键</param>
		/// <param name="IsUseCache">是否使用缓存的值</param>
		/// <returns></returns>
		public static T Load<T>(string fn, bool IsUseCache) where T : class
		{
			if (IsUseCache)
				return Load<T>(fn);
			return _Load<T>(fn) as T;
		}
		/// <summary>
		/// 从配置文件反序列化
		/// </summary>
		/// <typeparam name="T">反序列化的目标类型</typeparam>
		/// <param name="fn">键</param>
		/// <returns></returns>
		public static T Load<T>(string fn) where T : class
		{
			if (!CHCache.Contains(fn))
				CHCache.Add(fn, _Load<T>(fn));
			return CHCache.Get<T>(fn);
		}
		/// <summary>
		/// 从配置文件反序列化
		/// </summary>
		/// <typeparam name="T">反序列化的目标类型</typeparam>
		/// <param name="fn">键</param>
		/// <returns></returns>
		static object _Load<T>(string fn) where T : class
		{
			try
			{
				var mySerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
				using (var myFileStream = new StreamReader(CHServer.MapPath(fn)).BaseStream)
				{
					return mySerializer.Deserialize(myFileStream);
				}
			}
			catch
			{
				throw new Exception(string.Format("读取配置文件{0}时出错,编号:{1}", fn, 10358));
			}
		}
	}
}
