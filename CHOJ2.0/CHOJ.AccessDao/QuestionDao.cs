using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;

namespace CHOJ.AccessDao
{
    public class QuestionDao:BaseSqlMapDao, IQuestionDao {

        public IList<Question> AllQuestion() {
            return ExecuteQueryForList<Question>("AllQuestion", null);
        }
    }
}