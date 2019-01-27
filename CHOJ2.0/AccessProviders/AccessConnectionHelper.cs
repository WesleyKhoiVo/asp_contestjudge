//------------------------------------------------------------------------------
// <copyright file="AccessConnectionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AccessProviders {

    using System;
    using System.Web;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Threading;
    using System.Configuration;
    using System.Web.Util;
    using System.Security.Permissions;
    using System.Web.Hosting;
    using System.Security.Principal;
    using System.Security;
    using System.Diagnostics;
    using System.Web.Configuration;
	using CHOJ;

    internal static class AccessConnectionHelper {


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
       

        internal static int GetApplicationID(OleDbConnection connection, string applicationName) {
            return GetApplicationID(connection, applicationName, false);
        }

        internal static int GetApplicationID(OleDbConnection connection, string applicationName, bool createIfNeeded) {
            OleDbCommand lookupCommand = new OleDbCommand("SELECT ApplicationId FROM aspnet_Applications WHERE ApplicationName = @AppName", connection);
            lookupCommand.Parameters.Add(new OleDbParameter("@AppName", applicationName));

            object lookupResult = lookupCommand.ExecuteScalar();
            if ((lookupResult != null) && (lookupResult is int)) {
                return (int)lookupResult;
            }

            if (createIfNeeded) {
                OleDbCommand createCommand = new OleDbCommand("INSERT INTO aspnet_Applications (ApplicationName) VALUES (@AppName)",
                    connection);
                createCommand.Parameters.Add(new OleDbParameter("@AppName", applicationName));

                if (createCommand.ExecuteNonQuery() != 0) {
                    lookupResult = lookupCommand.ExecuteScalar();
                    if ((lookupResult != null) && (lookupResult is int)) {
                        return (int)lookupResult;
                    }
                }
            }

            return 0;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
	
        internal static int GetPathID(OleDbConnection connection, int applicationID, string path) {
            return GetPathID(connection, applicationID, path, false);
        }

        internal static int GetPathID(OleDbConnection connection, int applicationID, string path, bool createIfNeeded) {
            OleDbCommand lookupCommand = new OleDbCommand("SELECT PathId FROM aspnet_Paths WHERE ApplicationId = @AppId AND Path = @Path",
                connection);
            lookupCommand.Parameters.Add(new OleDbParameter("@AppId", applicationID));
            lookupCommand.Parameters.Add(new OleDbParameter("@Path", path));

            object lookupResult = lookupCommand.ExecuteScalar();
            if ((lookupResult != null) && (lookupResult is int)) {
                return (int)lookupResult;
            }

            if (createIfNeeded) {
                OleDbCommand createCommand = new OleDbCommand("INSERT INTO aspnet_Paths (ApplicationId, Path) VALUES (@AppID, @Path)",
                    connection);
                createCommand.Parameters.Add(new OleDbParameter("@AppID", applicationID));
                createCommand.Parameters.Add(new OleDbParameter("@Path", path));

                if (createCommand.ExecuteNonQuery() != 0) {
                    lookupResult = lookupCommand.ExecuteScalar();
                    if ((lookupResult != null) && (lookupResult is int)) {
                        return (int)lookupResult;
                    }
                }
            }

            return 0;
        }

        internal static int GetUserID(OleDbConnection connection, int applicationID, string userName) {
            return GetUserID(connection, applicationID, userName, false, false, DateTime.Now);
        }

        internal static int GetUserID(OleDbConnection connection, int applicationID, string userName, bool createIfNeeded) {
            return GetUserID(connection, applicationID, userName, createIfNeeded, false, DateTime.Now);
        }

        internal static int GetUserID(OleDbConnection connection, int applicationID, string userName, bool createIfNeeded, bool newUserIsAnonymous) {
            return GetUserID(connection, applicationID, userName, createIfNeeded, newUserIsAnonymous, DateTime.Now);
        }

        internal static int GetUserID(OleDbConnection connection, int applicationID, string userName, bool createIfNeeded, bool newUserIsAnonymous, DateTime lastActivityDate) {
            if (applicationID == 0 || userName == null || userName.Length < 1) // Application doesn't exist or user doesn't exist
                return 0;

            if (connection == null)
                return 0; // Wrong params!

            OleDbCommand lookupCommand = new OleDbCommand(@"SELECT UserId FROM aspnet_Users WHERE ApplicationId = @AppId AND UserName = @UserName",
                                                            connection);
            lookupCommand.Parameters.Add(new OleDbParameter("@AppId", applicationID));
            lookupCommand.Parameters.Add(new OleDbParameter("@UserName", userName));

            object lookupResult = lookupCommand.ExecuteScalar();
            if ((lookupResult != null) && (lookupResult is int)) {
                return (int)lookupResult;
            }

            if (createIfNeeded) {
                OleDbCommand createCommand = new OleDbCommand(@"INSERT INTO aspnet_Users " +
                                                                @"(ApplicationId, UserName, IsAnonymous, LastActivityDate) " +
                                                                @"VALUES (@AppID, @UserName, @IsAnonymous, @LastActivityDate)",
                                                                connection);
                createCommand.Parameters.Add(new OleDbParameter("@AppID", applicationID));
                createCommand.Parameters.Add(new OleDbParameter("@UserName", userName));
                createCommand.Parameters.Add(new OleDbParameter("@IsAnonymous", newUserIsAnonymous));
                createCommand.Parameters.Add(new OleDbParameter("@LastActivityDate", new DateTime(lastActivityDate.Year, lastActivityDate.Month, lastActivityDate.Day, lastActivityDate.Hour, lastActivityDate.Minute, lastActivityDate.Second)));

                if (createCommand.ExecuteNonQuery() != 0) {
                    lookupResult = lookupCommand.ExecuteScalar();
                    if ((lookupResult != null) && (lookupResult is int)) {
                        return (int)lookupResult;
                    }
                }
            }

            return 0;
        }

     

  
    }

    /// //////////////////////////////////////////////////////////////////////////////
   
}
