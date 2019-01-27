using System;
using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using Microsoft.Samples.Cloud.Data;
using System.Linq;
namespace CHOJ.AzureTable
{
    public class AnswerDao:BaseDao,IAnswerDao
    {
        public void SaveAnswer(Answer answer)
        {
            //answer
            SsdsContainer c1 = DbContext.OpenContainer("Answer");
            c1.Insert(answer, answer.Id);
            //question
            var answerType = (AnswerType) answer.Status;
            var c2 = DbContext.OpenContainer("Question");
            var q = c2.Single<Question>(
                c => c.Id == answer.QuestionId
                );
            if (q != null)
            {
                q.Entity.SubmitCount++;
                if (answerType == AnswerType.Accepted)
                    q.Entity.AcceptedCount++;
                c2.Update(q);
            }
            //user
            var c3 = DbContext.OpenContainer("Profile");

            var p = c3.Single<Profile>(c => c.Id == answer.UserId);
            if (p != null)
            {
                p.Entity.Submit++;
                if (answerType == AnswerType.Accepted)
                    p.Entity.Accepted++;
                c3.Update(p);
            }
        }

        public IEnumerable<Answer> Status()
        {
            SsdsContainer c1 = DbContext.OpenContainer("Answer");
            return c1.Query<Answer>(c => true).Select(c => c.Entity)
                .OrderByDescending(c => c.AddTime);

        }

        public IEnumerable<Answer> UserStatus(string userId, int page, int pageSize)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Answer");
            return c1.Query<Answer>(c => c.Entity.UserId == userId)
                .Select(c => c.Entity)
                .OrderByDescending(c => c.AddTime);
        }

        public IEnumerable<Answer> QuestionStatus(string questionId, int page, int pageSize)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Answer");
            return c1.Query<Answer>(c => c.Entity.QuestionId == questionId)
                .Select(c => c.Entity)
                .OrderByDescending(c => c.AddTime);
        }
    }
}