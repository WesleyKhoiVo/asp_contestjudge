namespace CHOJ.OpenId
{
    /*
     * FILE:        WindowsLiveLogin.cs
     *                                                                      
     * DESCRIPTION: Sample implementation of Web Authentication protocol in C#.
     *              Also includes trusted login and application verification
     *              sample implementation.
     *
     * VERSION:     1.0
     *
     * Copyright (c) 2007 Microsoft Corporation.  All Rights Reserved.
     */

    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.Configuration;
    using System.Security.Cryptography;
    using System.IO;
    using System.Net;
    using System.Xml;


    /// <summary>
    /// Sample implementation of Web Authentication protocol in C#.
    /// Also includes trusted login and application verification
    /// sample implementation.
    /// </summary>
    public class WindowsLiveLogin
    {
        /* Implementation of basic methods for Web Authentication support. */

        /// <summary>
        /// Stub implementation for logging debug output. You can run
        /// a tool such as 'dbmon' to see the output.
        /// </summary>
        static void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            System.Diagnostics.Debug.Flush();
        }

        /// <summary>
        /// Holds the user information after a successful login.
        /// </summary>
        public class User
        {
            /// <summary>
            /// Creates a User object.
            /// </summary>
            public User(string timestamp, string id, string flags, string context, string token)
            {
                SetTimestamp(timestamp);
                Id = id;
                SetFlags(flags);
                Context=context;
                Token=token;
            }

            DateTime _timestamp;

            /// <summary>
            ///  Timestamp is the time as obtained from the SSO token.
            /// </summary>
            public DateTime Timestamp { get { return _timestamp; } }

            private void SetTimestamp(string timeStamp)
            {
                if (timeStamp == null)
                {
                    throw new ArgumentException("Error: User: Null timestamp in token.");
                }

                int timestampInt;

                try
                {
                    timestampInt = Convert.ToInt32(timeStamp);
                }
                catch (Exception)
                {
                    throw new ArgumentException("Error: User: Invalid timestamp: "
                                                + timeStamp);
                }

                var refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                _timestamp = refTime.AddSeconds(timestampInt);
            }

            string _id;

            /// <summary>
            /// Id is the pairwise unique ID for the user.
            /// </summary>
            public string Id
            {
                get { return _id; }
                private set
                {
                    if (value == null)
                    {
                        throw new ArgumentException("Error: User: Null id in token.");
                    }
                    var re = new Regex(@"^\w+$");
                    if (!re.IsMatch(value))
                    {
                        throw new ArgumentException("Error: User: Invalid id: " + value);
                    }
                    _id = value;
                }
            }

            bool _usePersistentCookie;

            /// <summary>
            /// Indicates whether the application
            /// is expected to store the user token in a session or
            /// persistent cookie.
            /// </summary>
            public bool UsePersistentCookie { get { return _usePersistentCookie; } }

            private void SetFlags(string flags)
            {
                _usePersistentCookie = false;
                if (string.IsNullOrEmpty(flags))
                {
                }
                else
                {
                    try
                    {
                        var flagsInt = Convert.ToInt32(flags);
                        _usePersistentCookie = ((flagsInt%2) == 1);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Error: User: Invalid flags: "
                                                    + flags);
                    }
                }
            }

            /// <summary>
            /// Context is the application context that was originally
            /// passed to the login request, if any.
            /// </summary>
            public string Context { get; private set; }


            /// <summary>
            /// Token is the encrypted Web Authentication token containing
            /// the UID. This can be cached in a cookie and the UID can be
            /// retrieved by calling the ProcessToken method.
            /// </summary>
            public string Token { get; private set; }
        }

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID and secret key.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// </summary>
        public WindowsLiveLogin(String appId, String secret) :
            this(appId, secret, null) { }

        /// <summary>
        /// Initialize the WindowsLiveLogin module with the
        /// application ID, secret key, and security algorithm to use.
        ///
        /// We recommend that you employ strong measures to protect
        /// the secret key. The secret key should never be
        /// exposed to the Web or other users.
        /// </summary>
        public WindowsLiveLogin(String appId, String secret, String securityAlgorithm)
        {
            AppId = appId;
            Secret = secret;
            SecurityAlgorithm = securityAlgorithm;
        }

        /// <summary>
        /// Initialize the WindowsLiveLogin module from the
        /// web.config file if loadAppSettings is true. Otherwise,
        /// you will have to manually set the AppId, Secret and
        /// SecurityAlgorithm properties.
        /// </summary>
        public WindowsLiveLogin(bool loadAppSettings)
        {
            if (!loadAppSettings) { return; }

            NameValueCollection appSettings = WebConfigurationManager.AppSettings;
            if (appSettings == null)
            {
                throw new IOException("Error: WindowsLiveLogin: Failed to load the Web application settings.");
            }

            AppId = appSettings["wll_appid"];
            Secret = appSettings["wll_secret"];
            SecurityAlgorithm = appSettings["wll_securityalgorithm"];
            BaseUrl = appSettings["wll_baseurl"];
            SecureUrl = appSettings["wll_secureurl"];
        }

        /// <summary><![CDATA[
        /// Initialize the WindowsLiveLogin module from a settings file. 
        /// 
        /// 'settingsFile' specifies the location of the XML settings
        /// file containing the application ID, secret key, and an optional
        /// security algorithm.  The file is of the following format:
        /// 
        /// <windowslivelogin>
        ///   <appid>APPID</appid>
        ///   <secret>SECRET</secret>
        ///   <securityalgorithm>wsignin1.0</securityalgorithm>
        /// </windowslivelogin>
        /// 
        /// We recommend that you store the Windows Live Login settings file
        /// in an area on your server that cannot be accessed through
        /// the Internet. This file contains important confidential
        /// information.      
        /// ]]></summary>
        public WindowsLiveLogin(string settingsFile)
        {
            NameValueCollection settings = ParseSettings(settingsFile);
            AppId = settings["appid"];
            Secret = settings["secret"];
            SecurityAlgorithm = settings["securityalgorithm"];
            BaseUrl = settings["baseurl"];
            SecureUrl = settings["secureurl"];
        }

        string _appId;

        /// <summary>
        /// You can use this property to get or set the application ID.
        /// </summary>
        public string AppId
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                var re = new Regex(@"^\w+$");
                if (!re.IsMatch(value))
                {
                    throw new ArgumentException("Error: AppId: Application ID must be alphanumeric: " + value);
                }

                _appId = value;
            }

            get
            {
                if (string.IsNullOrEmpty(_appId))
                {
                    throw new InvalidOperationException("Error: AppId: Application ID was not set. Aborting.");
                }

                return _appId;
            }
        }

        byte[] _cryptKey;
        byte[] _signKey;

        /// <summary>
        /// You can use this method to set your secret key if
        /// one was not provided at initialization time.
        /// </summary>
        public string Secret
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                if (value.Length < 16)
                {
                    throw new ArgumentException("Error: Secret: Secret key is expected to be longer than 16 characters: " + value.Length);
                }

                _cryptKey = Derive(value, "ENCRYPTION");
                _signKey = Derive(value, "SIGNATURE");
            }

            get { return null; }
        }

        string _securityAlgorithm;

        /// <summary>
        /// This property gets or sets the security algorithm being
        /// used.
        /// </summary>
        public string SecurityAlgorithm
        {
            set { _securityAlgorithm = value; }

            get
            {
                return string.IsNullOrEmpty(_securityAlgorithm) ? "wsignin1.0" : _securityAlgorithm;
            }
        }

        string _baseUrl;

        /// <summary>
        /// Sets the URL to use for the Windows Live Login server. You
        /// should not have to use or change this. Furthermore, we
        /// recommend that you use the Sign In control instead of
        /// the URL methods provided here.
        /// </summary>
        public string BaseUrl
        {
            set { _baseUrl = value; }

            get
            {
                return string.IsNullOrEmpty(_baseUrl) ? "http://login.live.com/" : _baseUrl;
            }
        }

        string _secureUrl;

        /// <summary>
        /// Set the secure (HTTPS) URL to use for the Windows Live Login server.
        /// You should not have to use or change this directly.
        /// </summary>
        public string SecureUrl
        {
            set { _secureUrl = value; }

            get
            {
                return string.IsNullOrEmpty(_secureUrl) ? "https://login.live.com/" : _secureUrl;
            }
        }

        /// <summary>
        /// Processes the login response from the Windows Live Login server.
        /// </summary>
        ///
        /// <param name="query">Contains the preprocessed POST query
        /// such as that returned by HttpRequest.Form</param>
        /// 
        /// <returns>The method returns a User object on successful
        /// sign-in; otherwise null.</returns>
        public User ProcessLogin(NameValueCollection query)
        {
            if (query == null)
            {
                Debug("Error: ProcessLogin: Invalid query.");
                return null;
            }

            var action = query["action"];

            if (action != "login")
            {
                Debug("Warning: ProcessLogin: query action ignored: " + action);
                return null;
            }

            var token = query["stoken"];
            var context = query["appctx"];

            if (context != null)
            {
                context = HttpUtility.UrlDecode(context);
            }

            return ProcessToken(token, context);
        }

        /// <summary>
        /// Returns the sign-in URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <returns>Sign-in URL</returns>
        public string GetLoginUrl()
        {
            return GetLoginUrl(null);
        }

        /// <summary>
        /// Returns the sign-in URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <param name="context">If you specify it, <paramref
        /// name="context"/> will be returned as-is in the login
        /// response for site-specific use.</param>
        /// <returns>Sign-in URL</returns>
        public string GetLoginUrl(string context)
        {
            string algPart = "&alg=" + SecurityAlgorithm;
            string contextPart = (context == null) ?
              String.Empty : "&appctx=" + HttpUtility.UrlEncode(context);
            return BaseUrl + "wlogin.srf?appid=" + AppId +
              algPart + contextPart;
        }

        /// <summary>
        /// Returns the sign-out URL to use for the Windows Live Login server.
        /// We recommend that you use the Sign In control instead.
        /// </summary>
        /// <returns>Sign-out URL</returns>
        public string GetLogoutUrl()
        {
            return BaseUrl + "logout.srf?appid=" + AppId;
        }

        /// <summary>
        /// Decodes and validates a Web Authentication token. Returns a User
        /// object on success.
        /// </summary>
        public User ProcessToken(string token)
        {
            return ProcessToken(token, null);
        }

        /// <summary>
        /// Decodes and validates a Web Authentication token. Returns a User
        /// object on success. If a context is passed in, it will be
        /// returned as the context field in the User object.
        /// </summary>
        public User ProcessToken(string token, string context)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug("Error: ProcessToken: Invalid token.");
                return null;
            }

            string stoken = DecodeToken(token);
            if (string.IsNullOrEmpty(stoken))
            {
                Debug("Error: ProcessToken: Failed to decode token: " + token);
                return null;
            }

            stoken = ValidateToken(stoken);
            if (string.IsNullOrEmpty(stoken))
            {
                Debug("Error: ProcessToken: Failed to validate token: " + token);
                return null;
            }

            NameValueCollection parsedToken = HttpUtility.ParseQueryString(stoken);
            if (parsedToken.Count < 3)
            {
                Debug("Error: ProcessToken: Failed to parse token after decoding: " +
                      token);
                return null;
            }

            string appId = parsedToken["appid"];
            if (appId != AppId)
            {
                Debug("Error: ProcessToken: Application ID in token did not match ours: " +
                      appId + ", " + AppId);
                return null;
            }

            User user = null;
            try
            {
                user = new User(parsedToken["ts"],
                                parsedToken["uid"],
                                parsedToken["flags"],
                                context, token);
            }
            catch (Exception e)
            {
                Debug("Error: ProcessToken: Contents of token considered invalid: " + e);
            }
            return user;
        }

        /// <summary>
        /// When a user signs out of Windows Live or a Windows Live
        /// application, a best-effort attempt is made to sign the user out
        /// from all other Windows Live applications the user might be signed
        /// in to. This is done by calling the handler page for each
        /// application with 'action' parameter set to 'clearcookie' in the query
        /// string. The application handler is then responsible for clearing
        /// any cookies or data associated with the login. After successfully
        /// signing the user out, the handler should return a GIF (any
        /// GIF) as response to the action=clearcookie query.
        ///
        /// This function returns an appropriate content type and body
        /// response that the application handler can return to
        /// signify a successful sign-out from the application.
        /// </summary>
        public void GetClearCookieResponse(out string type, out byte[] content)
        {
            const string gif =
              "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAEALAAAAAABAAEAAAIBTAA7";
            type = "image/gif";
            content = Convert.FromBase64String(gif);
        }

        /// <summary>
        /// Decode the given token. Returns null on failure.
        /// </summary>
        ///
        /// <list type="number">
        /// <item>First, the string is URL unescaped and base64
        /// decoded.</item>
        /// <item>Second, the IV is extracted from the first 16 bytes
        /// of the string.</item>
        /// <item>Finally, the string is decrypted by using the
        /// encryption key.</item> 
        /// </list>
        public string DecodeToken(string token)
        {
            if (_cryptKey == null || _cryptKey.Length == 0)
            {
                throw new InvalidOperationException("Error: DecodeToken: Secret key was not set. Aborting.");
            }

            const int ivLength = 16;
            byte[] ivAndEncryptedValue = U64(token);

            if ((ivAndEncryptedValue == null) ||
                (ivAndEncryptedValue.Length <= ivLength) ||
                ((ivAndEncryptedValue.Length % ivLength) != 0))
            {
                Debug("Error: DecodeToken: Attempted to decode invalid token.");
                return null;
            }

            Rijndael aesAlg = null;
            MemoryStream memStream = null;
            CryptoStream cStream = null;
            StreamReader sReader = null;
            string decodedValue;

            try
            {
                aesAlg = new RijndaelManaged {KeySize = 128, Key = _cryptKey, Padding = PaddingMode.PKCS7};
                memStream = new MemoryStream(ivAndEncryptedValue);
                var iv = new byte[ivLength];
                memStream.Read(iv, 0, ivLength);
                aesAlg.IV = iv;
                cStream = new CryptoStream(memStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
                sReader = new StreamReader(cStream, Encoding.ASCII);
                decodedValue = sReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug("Error: DecodeToken: Decryption failed: " + e);
                return null;
            }
            finally
            {
                try
                {
                    if (sReader != null) { sReader.Close(); }
                    if (cStream != null) { cStream.Close(); }
                    if (memStream != null) { memStream.Close(); }
                    if (aesAlg != null) { aesAlg.Clear(); }
                }
                catch (Exception e)
                {
                    Debug("Error: DecodeToken: Failure during resource cleanup: " + e);
                }
            }

            return decodedValue;
        }

        /// <summary>
        /// Creates a signature for the given string by using the
        /// signature key.
        /// </summary>
        public byte[] SignToken(string token)
        {
            if (_signKey == null || _signKey.Length == 0)
            {
                throw new InvalidOperationException("Error: SignToken: Secret key was not set. Aborting.");
            }

            if (string.IsNullOrEmpty(token))
            {
                Debug("Attempted to sign null token.");
                return null;
            }

            using (HashAlgorithm hashAlg = new HMACSHA256(_signKey))
            {
                byte[] data = Encoding.Default.GetBytes(token);
                byte[] hash = hashAlg.ComputeHash(data);
                return hash;
            }
        }

        /// <summary>
        /// Extracts the signature from the token and validates it.
        /// </summary>
        public string ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug("Error: ValidateToken: Invalid token.");
                return null;
            }

            string[] s = { "&sig=" };
            string[] bodyAndSig = token.Split(s, StringSplitOptions.None);

            if (bodyAndSig.Length != 2)
            {
                Debug("Error: ValidateToken: Invalid token: " + token);
                return null;
            }

            byte[] sig = U64(bodyAndSig[1]);

            if (sig == null)
            {
                Debug("Error: ValidateToken: Could not extract the signature from the token.");
                return null;
            }

            byte[] sig2 = SignToken(bodyAndSig[0]);

            if (sig2 == null)
            {
                Debug("Error: ValidateToken: Could not generate a signature for the token.");
                return null;
            }

            if (sig.Length == sig2.Length)
            {
                for (int i = 0; i < sig.Length; i++)
                {
                    if (sig[i] != sig2[i]) { goto badSig; }
                }

                return token;
            }

        badSig:
            Debug("Error: ValidateToken: Signature did not match.");
            return null;
        }

        /* Implementation of the methods needed to perform Windows Live
           application verification as well as trusted login. */

        /// <summary>
        /// Generates an Application Verifier token.
        /// </summary>
        public string GetAppVerifier()
        {
            return GetAppVerifier(null);
        }

        /// <summary>
        /// Generates an Application Verifier token. An IP address
        /// can be included in the token.
        /// </summary>
        public string GetAppVerifier(string ip)
        {
            string ipPart = (ip == null) ? String.Empty : ("&ip=" + ip);
            string token = "appid=" + AppId + "&ts=" + GetTimestamp() + ipPart;
            string sig = E64(SignToken(token));

            if (sig == null)
            {
                Debug("Error: GetAppVerifier: Failed to sign the token.");
                return null;
            }

            token += "&sig=" + sig;
            return HttpUtility.UrlEncode(token);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token. The application security token
        /// will be generated for the Windows Live site.
        ///
        /// JavaScript Output Notation (JSON) output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl()
        {
            return GetAppLoginUrl(null, null, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'.
        ///
        /// JSON output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl(string siteId)
        {
            return GetAppLoginUrl(siteId, null, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// JSON output is returned:
        ///
        /// {"token":"&lt;value&gt;"}
        /// </summary>
        public string GetAppLoginUrl(string siteId, string ip)
        {
            return GetAppLoginUrl(siteId, ip, false);
        }

        /// <summary>
        /// Returns the URL needed to retrieve the application
        /// security token.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// If 'js' is false, then JSON output is returned: 
        ///
        /// {"token":"&lt;value&gt;"}
        ///
        /// Otherwise, a JavaScript response is returned. It is assumed
        /// that WLIDResultCallback is a custom function implemented to
        /// handle the token value:
        /// 
        /// WLIDResultCallback("&lt;tokenvalue&gt;");
        /// </summary>
        public string GetAppLoginUrl(string siteId, string ip, bool js)
        {
            string algPart = "&alg=" + SecurityAlgorithm;
            string sitePart = (siteId == null) ? "" : "&id=" + siteId;
            string jsPart = (!js) ? String.Empty : "&js=1";
            string url = SecureUrl + "wapplogin.srf?app=" +
              GetAppVerifier(ip) + algPart + sitePart + jsPart;
            return url;
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application login URL. The
        /// application security token will be generated for the
        /// Windows Live site.
        /// </summary>
        public string GetAppSecurityToken()
        {
            return GetAppSecurityToken(null, null);
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application login URL.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'.
        /// </summary>
        public string GetAppSecurityToken(string siteId)
        {
            return GetAppSecurityToken(siteId, null);
        }

        /// <summary>
        /// Retrieves the application security token for application
        /// verification from the application login URL.
        ///
        /// By default, the application security token will be
        /// generated for the Windows Live site; a specific Site ID
        /// can optionally be specified in 'siteId'. The IP address
        /// can also optionally be included in 'ip'.
        ///
        /// Implementation note: The application security token is
        /// downloaded from the application login URL in JSON format
        /// {"token":"&lt;value&gt;"}, so we need to extract
        /// &lt;value&gt; from the string and return it as seen here.
        /// </summary>
        public string GetAppSecurityToken(string siteId, string ip)
        {
            string url = GetAppLoginUrl(siteId, ip);
            string body = Fetch(url);
            if (body == null)
            {
                Debug("Error: GetAppSecurityToken: Failed to download token.");
                return null;
            }

            var re = new Regex("{\"token\":\"(.*)\"}");
            GroupCollection gc = re.Match(body).Groups;

            if (gc.Count != 2)
            {
                Debug("Error: GetAppSecurityToken: Failed to extract token: " + body);
                return null;
            }

            CaptureCollection cc = gc[1].Captures;

            if (cc.Count != 1)
            {
                Debug("Error: GetAppSecurityToken: Failed to extract token: " + body);
                return null;
            }

            return cc[0].ToString();
        }

        /// <summary>
        /// Returns a string that can be passed to the GetTrustedParams
        /// function as the 'retcode' parameter. If this is specified as
        /// the 'retcode', then the app will be used as return URL
        /// after it finishes trusted login.  
        /// </summary>
        public string GetAppRetCode()
        {
            return "appid=" + AppId;
        }

        /// <summary>
        /// Returns a table of key-value pairs that must be posted to
        /// the login URL for trusted login. Use HTTP POST to do
        /// this. Be aware that the values in the table are neither
        /// URL nor HTML escaped and may have to be escaped if you are
        /// inserting them in code such as an HTML form.
        /// 
        /// The user to be trusted on the local site is passed in as
        /// string 'user'.
        /// </summary>
        public NameValueCollection GetTrustedParams(string user)
        {
            return GetTrustedParams(user, null);
        }

        /// <summary>
        /// Returns a table of key-value pairs that must be posted to
        /// the login URL for trusted login. Use HTTP POST to do
        /// this. Be aware that the values in the table are neither
        /// URL nor HTML escaped and may have to be escaped if you are
        /// inserting them in code such as an HTML form.
        /// 
        /// The user to be trusted on the local site is passed in as
        /// string 'user'.
        /// 
        /// Optionally, 'retcode' specifies the resource to which
        /// successful login is redirected, such as Windows Live Mail,
        /// and is typically a string in the format 'id=2000'. If you
        /// pass in the value from GetAppRetCode instead, login will
        /// be redirected to the application. Otherwise, an HTTP 200
        /// response is returned.
        /// </summary>
        public NameValueCollection GetTrustedParams(string user, string retcode)
        {
            string token = GetTrustedToken(user);

            if (token == null) { return null; }

            token = "<wst:RequestSecurityTokenResponse xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><wst:RequestedSecurityToken><wsse:BinarySecurityToken xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">" + token + "</wsse:BinarySecurityToken></wst:RequestedSecurityToken><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><wsa:EndpointReference xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\"><wsa:Address>uri:WindowsLiveID</wsa:Address></wsa:EndpointReference></wsp:AppliesTo></wst:RequestSecurityTokenResponse>";

            var nvc = new NameValueCollection(3);
            nvc["wa"] = SecurityAlgorithm;
            nvc["wresult"] = token;

            if (retcode != null)
            {
                nvc["wctx"] = retcode;
            }

            return nvc;
        }

        /// <summary>
        /// Returns the trusted login token in the format needed by the
        /// trusted login gadget.
        ///
        /// User to be trusted on the local site is passed in as string
        /// 'user'.
        /// </summary>
        public string GetTrustedToken(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                Debug("Error: GetTrustedToken: Invalid user specified.");
                return null;
            }

            string token = "appid=" + AppId + "&uid=" +
              HttpUtility.UrlEncode(user) + "&ts=" + GetTimestamp();
            string sig = E64(SignToken(token));

            if (sig == null)
            {
                Debug("Error: GetTrustedToken: Failed to sign the token.");
                return null;
            }

            token += "&sig=" + sig;
            return HttpUtility.UrlEncode(token);
        }

        /// <summary>
        /// Returns the trusted sign-in URL to use for the Windows Live
        /// Login server. 
        /// </summary>
        public string GetTrustedLoginUrl()
        {
            return SecureUrl + "wlogin.srf";
        }

        /// <summary>
        /// Returns the trusted sign-out URL to use for the Windows Live
        /// Login server. 
        /// </summary>
        public string GetTrustedLogoutUrl()
        {
            return SecureUrl + "logout.srf?appid=" + AppId;
        }

        /* Helper methods */

        static NameValueCollection ParseSettings(string settingsFile)
        {
            if (settingsFile == null)
            {
                throw new ArgumentNullException("settingsFile");
            }

            var settings = new NameValueCollection();

            // Throws an exception on any failure.
            var xd = new XmlDocument();
            xd.Load(settingsFile);

            XmlNode appIdNode = xd.SelectSingleNode("//windowslivelogin/appid");
            String appId = (appIdNode == null) ? null : appIdNode.InnerText;
            if (string.IsNullOrEmpty(appId))
            {
                throw new XmlException("Error: parseSettings: Could not read appid node in settings file: " + settingsFile);
            }
            settings["appid"] = appId;

            XmlNode secretNode = xd.SelectSingleNode("//windowslivelogin/secret");
            String secret = (secretNode == null) ? null : secretNode.InnerText;
            if (string.IsNullOrEmpty(secret))
            {
                throw new XmlException("Error: parseSettings: Could not read secret node in settings file: " + settingsFile);
            }
            settings["secret"] = secret;

            XmlNode securityAlgorithmNode = xd.SelectSingleNode("//windowslivelogin/securityalgorithm");
            settings["securityalgorithm"] = (securityAlgorithmNode == null) ? null : securityAlgorithmNode.InnerText;

            XmlNode baseUrlNode = xd.SelectSingleNode("//windowslivelogin/baseurl");
            settings["baseurl"] = (baseUrlNode == null) ? null : baseUrlNode.InnerText;

            XmlNode secureUrlNode = xd.SelectSingleNode("//windowslivelogin/secureurl");
            settings["secureurl"] = (secureUrlNode == null) ? null : secureUrlNode.InnerText;
            return settings;
        }

        /// <summary>
        /// Derives the signature or encryption key, given the secret key 
        /// and prefix as described in the SDK documentation.
        /// </summary>
        static byte[] Derive(string secret, string prefix)
        {
            using (HashAlgorithm hashAlg = HashAlgorithm.Create("SHA256"))
            {
                const int keyLength = 16;
                byte[] data = Encoding.Default.GetBytes(prefix + secret);
                byte[] hashOutput = hashAlg.ComputeHash(data);
                var byteKey = new byte[keyLength];
                Array.Copy(hashOutput, byteKey, keyLength);
                return byteKey;
            }
        }

        /// <summary>
        /// Generates a timestamp suitable for the application
        /// verifier token.
        /// </summary>
        static string GetTimestamp()
        {
            var refTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan ts = DateTime.UtcNow - refTime;
            return ((uint)ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Base64-encode and URL-escape a byte array.
        /// </summary>
        static string E64(byte[] b)
        {
            string s = null;
            if (b == null) { return s; }

            try
            {
                s = Convert.ToBase64String(b);
                s = HttpUtility.UrlEncode(s);
            }
            catch (Exception e)
            {
                Debug("Error: E64: Base64 conversion error: " + e);
            }

            return s;
        }

        /// <summary>
        /// URL-unescape and Base64-decode a string.
        /// </summary>
        static byte[] U64(string s)
        {
            byte[] b = null;
            if (s == null) { return b; }
            s = HttpUtility.UrlDecode(s);

            try
            {
                b = Convert.FromBase64String(s);
            }
            catch (Exception e)
            {
                Debug("Error: U64: Base64 conversion error: " + s + ", " + e);
            }
            return b;
        }

        /// <summary>
        /// Fetch the contents given a URL.
        /// </summary>
        static string Fetch(string url)
        {
            string body = null;
            try
            {
                WebRequest req = WebRequest.Create(url);
                req.Method = "GET";
                WebResponse res = req.GetResponse();
                using (var sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    body = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug("Error: Fetch: Failed to get the document: " + url +
                      ", " + e);
            }
            return body;
        }
    }
}

