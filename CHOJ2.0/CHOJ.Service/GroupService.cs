using System;
using System.Linq;
using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using IBatisNet.DataAccess;

namespace CHOJ.Service
{
    public class GroupService
    {
        private static readonly GroupService _instance = new GroupService();
        private readonly IDaoManager _daoManager;
        private readonly IGroupDao _groupDao;



        private GroupService()
        {
            _daoManager = ServiceConfig.GetInstance().DaoManager;
            _groupDao = (IGroupDao)_daoManager.GetDao(typeof(IGroupDao));
        }

        IGroupDao GroupDao
        {
            get { return _groupDao; }
        }

        public static GroupService GetInstance()
        {
            return _instance;
        }

        private const string GROUPLISTSTRING = "CHOJ_GROUPLIST";

        public IEnumerable<Group> GroupList()
        {
            if (!CHCache.Contains(GROUPLISTSTRING))
            {
                var gl = GroupDao.GroupList();
                CHCache.Add(GROUPLISTSTRING, gl, TimeSpan.FromMinutes(3));
            }
            return CHCache.Get<IEnumerable<Group>>(GROUPLISTSTRING);
        }

        public Group GetGroup(string id)
        {
            return GroupList().SingleOrDefault(c => c.Id == id);
        }

        public void Add(Group group)
        {
            GroupDao.Add(group);
            CHCache.Remove(GROUPLISTSTRING);
        }
        public void Delete(string id)
        {
            GroupDao.Delete(id);
            CHCache.Remove(GROUPLISTSTRING);
        }
    }
}