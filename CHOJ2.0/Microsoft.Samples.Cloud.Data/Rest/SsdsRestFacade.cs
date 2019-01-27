namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;

    /// <summary>
    /// Facade used to generate HttpRequests to Ssds.
    /// </summary>
    public class SsdsRestFacade
    {
        /// <summary>
        /// Holds a reference to the username used to connect to Ssds.
        /// </summary>
        private string userName;

        /// <summary>
        /// Holds a reference to the password used to connect to Ssds.
        /// </summary>
        private string password;

        /// <summary>
        /// Holds a reference to the scope where the request will be done.
        /// </summary>
        private Uri scope;

        /// <summary>
        /// Creates an instance of SsdsRestFacade.
        /// </summary>
        /// <param name="userName">The username used to connect to Ssds.</param>
        /// <param name="password">The password used to connect to Ssds.</param>
        /// <param name="scope">The scope (url) used to connect to Ssds.</param>
        public SsdsRestFacade(string userName, string password, Uri scope)
        {
            this.userName = userName;
            this.password = password;
            this.scope = scope;
        }

        /// <summary>
        /// Sends a Post message to the current container using the payload
        /// as the message body.
        /// </summary>
        /// <param name="payload">The message that will be posted.</param>
        /// <returns>The url of the create object.</returns>
        public string Insert(string payload)
        {
            WebRequest request = this.CreateRequest(this.scope, HttpVerbs.Post, payload);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Unexpected return code: {0}", response.StatusCode);
                    throw new InvalidOperationException(message);
                }

                return response.Headers[HttpResponseHeader.Location];
            }
        }

        public string Insert(SsdsBlobEntity blob)
        {
            return this.InsertOrUpdate(blob, HttpVerbs.Post, ConcurrencyPattern.Always);
        }

        private string InsertOrUpdate(SsdsBlobEntity blob, string verb, ConcurrencyPattern concurrencyPattern)
        {
            string locationResult = String.Empty;
            IAsyncResult asyncResult = null;
            Exception backgroundError = null;                        
            blob.InitRead();

            Uri containerScope = this.scope;
            if (verb == HttpVerbs.Put)
            {
                 containerScope = new Uri( String.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.scope, blob.Id));
            }

            HttpWebRequest uploadRequest = CreateStreamingRequest(containerScope, verb, blob);
            if (verb == HttpVerbs.Post)
            {
                uploadRequest.Headers["Slug"] = blob.Id;
            }

            if (concurrencyPattern == ConcurrencyPattern.IfMatch)
            {
                uploadRequest.Headers.Add("if-match", blob.Version);
            }
            else if (concurrencyPattern == ConcurrencyPattern.IfNoneMatch)
            {
                uploadRequest.Headers.Add("if-none-match", blob.Version);
            }

            try
            {                
                using (Stream requestStream = uploadRequest.GetRequestStream())
                {
                    while (blob.ReadBuffer() > 0)
                    {                        
                        requestStream.Write(blob.Buffer, 0, blob.ReadSize);
                        if (asyncResult == null)
                        {
                            asyncResult = uploadRequest.BeginGetResponse((IAsyncResult res) =>
                            {
                                HttpWebRequest req = (HttpWebRequest)res.AsyncState;
                                try
                                {
                                    using (HttpWebResponse response = (HttpWebResponse)req.EndGetResponse(res))
                                    {
                                        locationResult = response.Headers[HttpResponseHeader.Location];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    backgroundError = ex;
                                }
                            }, uploadRequest);
                        }                        
                    } 
                }

                if (asyncResult != null)
                {
                    asyncResult.AsyncWaitHandle.WaitOne();
                    if (backgroundError != null)
                    {
                        throw backgroundError;
                    }
                }
            }
            catch (WebException ex)
            {
                string errorMsg = ex.Message;

                if (ex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
                    {
                        errorMsg = ReadResponse(errorResponse);
                    }
                }

                throw new ApplicationException(errorMsg);
            }

            return locationResult;
        }

        /// <summary>
        /// Sends a Get message to the current container using the query 
        /// as the query string for the request.
        /// </summary>
        /// <param name="query">The query string to be sended.</param>
        /// <returns>The HttpResponse body as string.</returns>
        public XElement Get(string query)
        {
            //TODO:  figure out where and how we handle errors
            Uri destination = new Uri(String.Format(CultureInfo.InvariantCulture, "{0}?q={1}", this.scope, HttpUtility.UrlEncode(query)));
            WebRequest request = this.CreateRequest(destination, HttpVerbs.Get);

            XElement xml = new XElement("Error"); //should never see this...

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new System.IO.StreamReader(stream))
            {
                if (!reader.EndOfStream)
                {
                    xml = XElement.Load(reader);
                }
            }

            return xml;
        }

        public SsdsBlobEntity GetBlob(string entityId)
        {
            SsdsBlobEntity blob = null;
            WebRequest request = this.CreateRequest(new Uri(String.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.scope, entityId)), HttpVerbs.Get);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                blob = new SsdsBlobEntity();
                blob.Append(response.GetResponseStream());
                blob.MimeType = response.Headers[HttpResponseHeader.ContentType];
                blob.ContentDisposition = response.Headers["Content-Disposition"];
                blob.Version = response.Headers[HttpResponseHeader.ETag];
                blob.Id = entityId;
            }

            return blob;
        }

        /// <summary>
        /// Sends a Delete message to the current container using the entityId 
        /// as the last fragment of the Uri.
        /// </summary>
        /// <param name="entityId">The identifier for the object where the message is going.</param>
        public void Delete(string entityId, ConcurrencyPattern concurrencyPattern, string version)
        {
            Uri destination = HttpRestUriTemplates.EntityTemplate.BindByPosition(this.scope, entityId);
            WebRequest request = this.CreateRequest(destination, HttpVerbs.Delete);

            if (concurrencyPattern == ConcurrencyPattern.IfMatch)
            {
                request.Headers.Add("if-match", version);                
            }
            else if (concurrencyPattern == ConcurrencyPattern.IfNoneMatch)
            {
                request.Headers.Add("if-none-match", version);                
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Unexpected return code: {0}", response.StatusCode);
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Sends a Put message to the current container using the paylod as the message body.
        /// </summary>
        /// <param name="payload">The message body.</param>
        public void Update(string payload, ConcurrencyPattern concurrencyPattern, string version)
        {
            WebRequest request = this.CreateRequest(this.scope, HttpVerbs.Put, payload);
            if (concurrencyPattern == ConcurrencyPattern.IfMatch)
            {
                request.Headers.Add("if-match", version);
            }
            else if (concurrencyPattern == ConcurrencyPattern.IfNoneMatch)
            {
                request.Headers.Add("if-none-match", version);
            }
            
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Unexpected return code: {0}", response.StatusCode);
                    throw new InvalidOperationException(message);
                }
            }
        }

        public void Update(SsdsBlobEntity blob, ConcurrencyPattern concurrencyPattern)
        {
            this.InsertOrUpdate(blob, HttpVerbs.Put, concurrencyPattern);
        }

        /// <summary>
        /// Sends a GET message to the entity provided as parameteer and returns 
        /// true if the response status is 200 OK else it returns false.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>A value indicating whether the entity exists.</returns>
        public bool EntityExists(string name)
        {
            Uri destination = HttpRestUriTemplates.EntityTemplate.BindByPosition(this.scope, name);
            WebRequest request = this.CreateRequest(destination, HttpVerbs.Get);            
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an instance of WebRequest.
        /// </summary>
        /// <param name="target">The Uri where the request will be done.</param>
        /// <param name="verb">The verb that will be used to do the request.</param>
        /// <returns>A new instance of WebRequest.</returns>
        private WebRequest CreateRequest(Uri target, string verb)
        {
            return this.CreateRequest(target, verb, null);
        }

        /// <summary>
        /// Creates an instance of WebRequest.
        /// </summary>
        /// <param name="destiny">The Uri where the request will be done.</param>
        /// <param name="verb">The verb that will be used to do the request.</param>
        /// <param name="payload">The message that will be written on request body.</param>
        /// <returns>A new instance of WebRequest.</returns>
        private WebRequest CreateRequest(Uri target, string verb, string payload)
        {
            WebRequest request = HttpWebRequest.Create(target);
            request.Credentials = new NetworkCredential(this.userName, this.password);
            request.Method = verb;

            var bytes = payload != null ? new UTF8Encoding().GetBytes(payload) : new byte[] { };

            request.ContentLength = bytes.Length;

            if (verb != HttpVerbs.Get && verb != HttpVerbs.Delete)
            {
                request.ContentType = RequestContentTypes.ApplicationXml;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return request;
        }

        private HttpWebRequest CreateStreamingRequest(Uri containerUri, string verb, SsdsBlobEntity entity)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(containerUri);            
            request.Method = verb;
            request.ContentLength = entity.Length;
            request.ContentType = entity.MimeType;
            request.Timeout = 30000;

            System.Net.Mime.ContentDisposition contentDisposition = new System.Net.Mime.ContentDisposition();
            contentDisposition.FileName = entity.ContentDisposition;
            request.Headers["Content-Disposition"] = contentDisposition.ToString();

            // Disable buffering by the client so that we get true streaming.
            request.AllowWriteStreamBuffering = false;

            // Construct the Authorization header for this request manually since there is a known
            // issue on HttpWebRequest with streaming authenticated requests which don't use the GET verb.
            byte[] credentialBuffer = new UTF8Encoding().GetBytes(String.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.userName, this.password));
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(credentialBuffer));
            return request;
        }

        public static string ReadResponse(HttpWebResponse response)
        {            
            if (response == null)
            {
                throw new ArgumentNullException("response", "Value cannot be null");
            }

            string responseBody = String.Empty;
            using (Stream rspStm = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(rspStm))
                {
                    responseBody = reader.ReadToEnd();
                }
            }

            return responseBody;
        }
    }
}
