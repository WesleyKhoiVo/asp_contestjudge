using System;
using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using IBatisNet.DataAccess;
using System.Linq;
namespace CHOJ.Service
{
    public class WikiService
    {
          private static WikiService _instance = new WikiService();
        private IDaoManager _daoManager;
        private IWikiDao _Dao;



        private WikiService()
        {
            _daoManager = ServiceConfig.GetInstance().DaoManager;
            _Dao = (IWikiDao)_daoManager.GetDao(typeof(IWikiDao));
        
        }

        IWikiDao WikiDao
        {
            get { return _Dao; }
        }

        public static WikiService GetInstance()
        {
            return _instance;
        }

        private string WIKICACHE = "WIKICACHE";

        public void Add(Wiki wiki)
        {
            wiki.AddTime = DateTime.Now;
            WikiDao.Add(wiki);
            CHCache.Remove(WIKICACHE);
        }

        public void Delete(string id)
        {
            WikiDao.Delete(id);
            CHCache.Remove(WIKICACHE);
        }

        public Wiki Get(string title)
        {
            return List().FirstOrDefault(c => c.Title == title);
        }

        public IEnumerable<Wiki> List()
        {
            if(!CHCache.Contains(WIKICACHE))
            {
                CHCache.Add(WIKICACHE, WikiDao.List());
            }
            return CHCache.Get<IEnumerable<Wiki>>(WIKICACHE);
        }
        public void Update(Wiki wiki ,string id)
        {
            wiki.AddTime = DateTime.Now;
            wiki.Id = id;
            WikiDao.Update(wiki);
            CHCache.Remove(WIKICACHE);
        }
        public Wiki GetById(string id)
        {
            return List().FirstOrDefault(c => c.Id == id);
        }
    }
}