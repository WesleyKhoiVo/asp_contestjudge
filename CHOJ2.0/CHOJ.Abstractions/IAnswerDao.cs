using CHOJ.Models;
using System.Collections.Generic;
namespace CHOJ.Abstractions
{
    public interface IAnswerDao
    {
        void SaveAnswer(Answer answer);
        IEnumerable<Answer> Status();
        IEnumerable<Answer> UserStatus(string userId,int page,int pageSize);
        IEnumerable<Answer> QuestionStatus(string questionId, int page, int pageSize);
    }
}