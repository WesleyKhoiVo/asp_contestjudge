using System.Collections.Generic;
using CHOJ.Models;

namespace CHOJ.Abstractions
{
    public interface IQuestionDao
    {
        IEnumerable<Question> AllQuestion();
        void Add(Question question);
        void Update(Question question);
        void Delete(string id);
    }
}