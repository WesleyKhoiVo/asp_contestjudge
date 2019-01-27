using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.Devices;
namespace CHOJ {
	public class Registry {
		static public void Init() {
			var sc = new ServerComputer();
            try
            {
                //	if (sc.Registry.CurrentUser.OpenSubKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\DbgJITDebugLaunchSetting") != null) {

                var ret = sc.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\", "DbgJITDebugLaunchSetting", 1);
                if (ret.ToString() != "1")
                    sc.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\", "DbgJITDebugLaunchSetting", 1);

                //		}
                //		sc.Registry.CurrentUser.Close();
            }
            catch
            {
            }
            finally
            {
                //		sc.Registry.CurrentUser.Close();
            }
		}
	}
}
