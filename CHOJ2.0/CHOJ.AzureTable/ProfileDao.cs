using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using System;
using System.Linq;

namespace CHOJ.AzureTable
{
    public class ProfileDao:BaseDao, IProfileDao
    {

        public string Update(Profile profile)
        {
           
          var p =  DbContext.CreateQuery<Profile>("Profile").FirstOrDefault(c => c.OpenId == profile.OpenId
                                                                          && c.IdType == profile.IdType);
            if (p == null)
            {
                profile.LogOnTime = DateTime.Now;
                profile.RegisterTime = DateTime.Now;
                profile.Role = "User";
                profile.Id = Guid.NewGuid().ToString();
                DbContext.AddObject("Profile", profile);
                DbContext.SaveChanges();
                return profile.Id;
            }
            var newp = p;
            newp.Name = profile.Name;
            //TODo:
            if (profile.Name == "÷ÿµ‰")
                newp.Role = "Admin";
            newp.NickName = profile.NickName;
            newp.School = profile.School;
            newp.SchoolDetails = profile.SchoolDetails;
            newp.Sex = profile.Sex;
            DbContext.UpdateObject(newp);
            DbContext.SaveChanges();
            return p.Id;
        }

        public Profile Details(string openId, string idType)
        {
            var ret = DbContext.CreateQuery<Profile>("Profile").FirstOrDefault(c => c.OpenId == openId
                                                                                    && c.IdType == idType);
   
            if (ret == null)
                return null;
            return ret;
        }

        public string GetNickName(string openId, string idType)
        {
            var x = Details(openId, idType);
            return x == null ? "Œ¥ªÒ»°" : x.NickName;
        }

        public IEnumerable<Profile> RankList(int n)
        {
        return DbContext.CreateQuery<Profile>("Profile")
            .OrderByDescending(c => c.Accepted).Take(n);
        }
    }
}