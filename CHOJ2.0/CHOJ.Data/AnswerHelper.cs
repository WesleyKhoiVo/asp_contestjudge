using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
namespace CHOJ {
	public class AnswerHelper {
		static public void SaveAnswer(DataBaseExecutor db, long qid, string username, int st,
			string CompilerName, long UseTime, long UseMemory,Guid Guid) {
			db.Execute(@"INSERT INTO Answer 
(QuestionID, Username, Status, Addtime, Complier, Type, UseTime, UseMemory,[Guid] )
values(@qid,@un,@st, Now(),@cn,0,@UseTime,@UseMemory,@Guid)", "@qid", qid, "@un", username, "@st", st,
								"@cn", CompilerName, "@UseTime", UseTime, "@UseMemory", UseMemory
								, "@Guid", Guid.ToString()
								);
		}
		static public IEnumerable<DataRow> MySatus(DataBaseExecutor db,string username) {
			return db.GetTable(@"SELECT Answer.QuestionID, Answer.Username, 
Answer.Status, Answer.Addtime, Answer.Complier, Answer.UseTime, 
Answer.UseMemory, Question.Title, Answer.ID,Answer.Guid
FROM Answer INNER JOIN Question ON Answer.QuestionID = Question.ID 
where answer.username=@un
order by answer.id desc", "@un", username).AsEnumerable().ToList();
		}
		static public IEnumerable<DataRow> Status(DataBaseExecutor db, int p) {
			return db.GetTable(@"SELECT Answer.QuestionID, Answer.Username, 
Answer.Status, Answer.Addtime, Answer.Complier, Answer.UseTime, 
Answer.UseMemory, Question.Title, Answer.ID,Answer.Guid
FROM Answer INNER JOIN Question ON Answer.QuestionID = Question.ID 
order by answer.id desc").AsEnumerable().Skip(20 * (p - 1)).Take(20);
		}
	}
}
