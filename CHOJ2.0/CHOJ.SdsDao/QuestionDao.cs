using System;
using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using Microsoft.Samples.Cloud.Data;
using System.Linq;
namespace CHOJ.SdsDao
{
    public class QuestionDao:BaseDao,IQuestionDao
    {
        public IEnumerable<Question> AllQuestion()
        {
            SsdsContainer c1 = DbContext.OpenContainer("Question");
            return c1
                .Query<Question>(c => true)
                .Select(c => c.Entity)
                .OrderBy(c=>c.AddTime);
        }

        public void Add(Question question)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Question");
            var id = Guid.NewGuid().ToString();
            question.Id=id;
            c1.Insert<Question>(new SsdsEntity<Question>
                                    {
                                        Id = id,
                                        Entity = question
                                    });
        }

        public void Update(Question question)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Question");
            var q=c1.Single<Question>(c => c.Id == question.Id).Entity;

            q.Title = question.Title;
            q.Body = question.Body;
            q.MemoryLimit = question.MemoryLimit;
            q.TimeLimit = question.TimeLimit;
            q.Test = question.Test;
            q.GroupId = question.GroupId;
            c1.Update<Question>(q, question.Id, ConcurrencyPattern.Always);

        }

        public void Delete(string id)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Question");
            c1.Delete(id);
        }
    }
}