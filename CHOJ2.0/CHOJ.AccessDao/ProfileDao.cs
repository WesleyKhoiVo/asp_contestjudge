using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHOJ.Abstractions;
using CHOJ.Models;

namespace CHOJ.AccessDao
{
    public class ProfileDao : IProfileDao {
//        public static void Create(DataBaseExecutor db, string Username) {
//            db.Execute(@"INSERT INTO [User]([Username],[NickName],[Name],[Sex],[Birthday],[Grade],[School],[SchoolDetails],[Submit],[Accepted])
//VALUES(@Username,@Username,'',0,Now(),2008,'','',0,0)", "@Username", Username);
//        }
        public void Update(Profile profile)
        {
            throw new NotImplementedException();
        }

        string IProfileDao.Update(Profile profile)
        {
            throw new NotImplementedException();
        }

        public Profile Details(string openId, string idType)
        {
            throw new NotImplementedException();
        }

        public string GetNickName(string openId, string idType)
        {
            throw new NotImplementedException();
        }
    }
}