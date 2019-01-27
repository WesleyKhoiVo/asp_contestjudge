using System;
using CHOJ.Abstractions;
using CHOJ.Models;
using IBatisNet.DataAccess;
using System.Collections.Generic;
namespace CHOJ.Service
{
    public class ProfileService
    {
        private static ProfileService _instance = new ProfileService();
        private IDaoManager _daoManager;
        private IProfileDao _Dao;



        private ProfileService()
        {
            _daoManager = ServiceConfig.GetInstance().DaoManager;
            _Dao = (IProfileDao)_daoManager.GetDao(typeof(IProfileDao));
        }

        IProfileDao ProfileDao
        {
            get { return _Dao; }
        }

        public static ProfileService GetInstance()
        {
            return _instance;
        }

        public Profile Get()
        {
            return ProfileDao.Details(HalfoxUser.OpenId, HalfoxUser.IdType);
        }

        public void Update(Profile profile)
        {
            profile.OpenId = HalfoxUser.OpenId;
            profile.IdType = HalfoxUser.IdType;
            var id = ProfileDao.Update(profile);
            HalfoxUser.Id = id;
            HalfoxUser.Name = profile.NickName;
        }
        public string GetNickName()
        {
            return ProfileDao.GetNickName(HalfoxUser.OpenId, HalfoxUser.IdType);
        }


        public IEnumerable<Profile> RankList(int n)
        {
            string key = "ranklist";
            if(!CHCache.Contains(key))
            {
                CHCache.Add(key, ProfileDao.RankList(n), TimeSpan.FromMinutes(3));
            }
            return CHCache.Get<IEnumerable<Profile>>(key);
        }
    }
}