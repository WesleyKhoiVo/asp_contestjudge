namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.IO;
    using Microsoft.Win32;

    public class SsdsBlobEntity : IDisposable
    {
        public const int DefaultBufferSize = 64 * 1024;
        public const string DefaultMimeType = "application/octet-stream";

        private Stream content;        

        public string Id
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        public string ContentDisposition
        {
            get;
            set;
        }

        public string MimeType
        {
            get;
            set;
        }        

        public int BufferSize
        {
            get;
            set;
        }

        public int ReadSize
        {
            get;
            private set;
        }

        public byte[] Buffer
        {
            get;
            private set;
        }

        public long Length
        {
            get
            {
                return this.content.Length;
            }
        }
       
        public SsdsBlobEntity()
        {
            this.Version = "0";
            this.ReadSize = -1;
            this.BufferSize = DefaultBufferSize;
            this.MimeType = DefaultMimeType;
            this.content = new MemoryStream();
        }

        public SsdsBlobEntity(string fileName)
        {
            this.Version = "0";
            this.ReadSize = -1;
            this.BufferSize = DefaultBufferSize;
            this.MimeType = GetMimeTypeFromFileName(fileName);
            this.ContentDisposition = (new FileInfo(fileName)).Name;
            this.content = File.OpenRead(fileName);            
        }

        public SsdsBlobEntity(Stream memory)
        {
            this.Version = "0";
            this.ReadSize = -1;
            this.BufferSize = DefaultBufferSize;
            this.MimeType = DefaultMimeType;
            this.content = memory;
        }

        public void Dispose()
        {
            if (this.content != null)
            {                
                this.content.Dispose();
            }
        }

        public void InitRead()
        {
            this.ReadSize = -1;
            this.content.Position = 0;
            this.Buffer = new Byte[this.BufferSize];
        }

        public int ReadBuffer()
        {
            this.ReadSize = this.content.Read(this.Buffer, 0, this.Buffer.Length);
            return this.ReadSize;
        }

        public void UpdateContent(Stream stream)
        {
            this.content.Dispose();
            this.content = stream;
        }

        public long Append(Stream stream)
        {
            this.Buffer = new byte[this.BufferSize];
            long length = 0L;
            int read;
            do
            {
                read = stream.Read(this.Buffer, 0, this.Buffer.Length);
                if (read > 0)
                {
                    length += (long)read;
                    this.content.Write(this.Buffer, 0, read);
                }
            } while (read > 0);

            return length;
        }

        private static string GetMimeTypeFromFileName(string blobFile)
        {
            if (String.IsNullOrEmpty(blobFile))
            {
                return null;
            }

            string contentType = DefaultMimeType;

            string fileExt = Path.GetExtension(blobFile).ToLowerInvariant();
            RegistryKey fileExtKey = Registry.ClassesRoot.OpenSubKey(fileExt);

            const string contentTypeValueString = "Content Type";

            if (fileExtKey != null && fileExtKey.GetValue(contentTypeValueString) != null)
            {
                contentType = fileExtKey.GetValue(contentTypeValueString).ToString();
            }

            return contentType;
        }        
    }
}
