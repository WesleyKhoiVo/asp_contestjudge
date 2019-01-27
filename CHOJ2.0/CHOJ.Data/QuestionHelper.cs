using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
namespace CHOJ {
	public class QuestionHelper {
		static readonly string QUESTIONLIST = "CHOJ.QUESTIONLIST";
		static DataTable AllQuestion(DataBaseExecutor db) {
			string key = QUESTIONLIST;
			DataTable dt = null;
			if (CHCache.Contains(key)) {
				dt = CHCache.Get<DataTable>(key);
			}
			else {
				dt = db.GetTable(@"SELECT * FROM [Question]");// where istrue=1
				CHCache.Add(key, dt);
			}
			return dt;
		}

		static public IEnumerable<DataRow> QuestionList(DataBaseExecutor db, long groupid, int p) {
			string key = string.Format(QUESTIONLIST, groupid);
			return AllQuestion(db).AsEnumerable().Where(c => c.Field<int>("GroupID") == groupid).OrderBy(c => c.Field<int>("ID"));
				//.Skip(20 * (p - 1)).Take(20);
		}
		static public DataRow Question(DataBaseExecutor db, long id) {
			return AllQuestion(db).AsEnumerable().Where(c => c.Field<int>("ID") == id).FirstOrDefault();
		}
		static public void QuestionListClear() {
			string key = QUESTIONLIST;
			if (CHCache.Contains(key))
				CHCache.Remove(key);
		}
	}
}
