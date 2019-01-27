using System;
using CHOJ.Abstractions;
using CHOJ.Models;
using IBatisNet.DataAccess;
using System.Collections.Generic;
using IBatisNet.DataAccess.SessionStore;

namespace CHOJ.Service
{
    public class QuestionService
    {
       private static QuestionService _instance = new QuestionService();
        private IDaoManager _daoManager;
        private IQuestionDao _Dao;



        private QuestionService()
        {
            _daoManager = ServiceConfig.GetInstance().DaoManager;
            _Dao = (IQuestionDao)_daoManager.GetDao(typeof(IQuestionDao));
            _daoManager.SessionStore = new HybridWebThreadSessionStore(_daoManager.Id);
        }

        IQuestionDao QuestionDao
        {
            get { return _Dao; }
        }

        public static QuestionService GetInstance()
        {
            return _instance;
        }

        private string ALLQUESTIONSTRING = "CHOJ_ALLQUESTION";

        public IEnumerable<Question> All()
        {
            if (!CHCache.Contains(ALLQUESTIONSTRING))
            {
                CHCache.Add(ALLQUESTIONSTRING, QuestionDao.AllQuestion(), TimeSpan.FromMinutes(3));
            }
            return CHCache.Get<IEnumerable<Question>>(ALLQUESTIONSTRING);
        }
        public void Create(Question question)
        {
            question.UserId = HalfoxUser.Id;
            question.AddTime = DateTime.Now;
            
            QuestionDao.Add(question);
            RemoveCache();
        }
        public void Delete(string id)
        {
            QuestionDao.Delete(id);
            RemoveCache();
        }
        public void RemoveCache()
        {
            CHCache.Remove(ALLQUESTIONSTRING);
        }
        public void Update(Question question,string id)
        {
            question.Id = id;
            QuestionDao.Update(question);
            RemoveCache();
        }
    }
}