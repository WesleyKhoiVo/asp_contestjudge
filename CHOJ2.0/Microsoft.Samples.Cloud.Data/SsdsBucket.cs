namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    public class SsdsEntityBucket
    {
        public XElement Response { get; set; }
        public Type[] Types { get; set; }

        public IEnumerable<SsdsEntity<T>> GetEntities<T>() where T: class, new()
        {
            SsdsEntitySerializer<T> serializer = new SsdsEntitySerializer<T>();
            return serializer.DeserializeMany(Response);
        }
    }

}
