using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace CHOJ {
    public class SecurityCodeLoader {
        static SecurityCodeLoader() {
            CodeList = new Dictionary<string, string>();
        }
        static Dictionary<string,string> CodeList { get; set; }
       public  static string GetSecurityCode(string rootPath,string compilerName)
        {
            if (!CodeList.Keys.Contains(compilerName))
            {
                string p = string.Format("{1}Config/{0}.txt", compilerName, rootPath);
                using (var rs = new StreamReader(p))
                {
                    CodeList[compilerName] = rs.ReadToEnd();
                }
            }
            return CodeList[compilerName];
        }
    }
}
