using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace CHOJ {
	public class GroupHelper {
		const string GROUP = "CHOJ.GROUP";
		static public IEnumerable<DataRow> GroupList(DataBaseExecutor db) {
			var rows =GroupTable(db).AsEnumerable().Where(c=>c.Field<int>("type")==0).OrderBy(c=>c.Field<int>("order"));
			return rows;
		}
		static public DataRow GetGroup(DataBaseExecutor db, long id) {
			var rets = GroupTable(db).AsEnumerable().Where(c => c.Field<int>("id") == id).FirstOrDefault();
			if (rets != null) return rets;
			return null;
		}
		static DataTable GroupTable(DataBaseExecutor db) {
			string key = GROUP;
			DataTable dt = null;
			if (CHCache.Contains(key)) {
				dt = CHCache.Get<DataTable>(key);
			}
			else {
				dt = db.GetTable(@"SELECT * FROM [Group]");
				CHCache.Add(key, dt);
			}
			return dt;
		}
		static public void GroupClear(long groupid) {
			string key = GROUP;
			if (CHCache.Contains(key))
				CHCache.Remove(key);
		}
	}
}
