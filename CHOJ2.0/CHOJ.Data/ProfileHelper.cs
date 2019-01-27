using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHOJ {
	public class ProfileHelper {
		public static void Create(DataBaseExecutor db, string Username) {
			db.Execute(@"INSERT INTO [User]([Username],[NickName],[Name],[Sex],[Birthday],[Grade],[School],[SchoolDetails],[Submit],[Accepted])
VALUES(@Username,@Username,'',0,Now(),2008,'','',0,0)", "@Username", Username);
		}
	}
}
