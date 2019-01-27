namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Globalization;

    /// <summary>
    /// Base class for all the entities that will get
    /// serialized and stored inside Ssds.
    /// </summary>
    public class SsdsEntity<T> where T: class
    {
        string kind;
        string id;

        Dictionary<string, object> propertyBucket = new Dictionary<string, object>();

        public SsdsEntity() { }

        /// <summary>
        /// Gets or sets the entity identifier.  If not set, uses a GUID string 
        /// automatically
        /// </summary>
        [XmlElement(Namespace = @"http://schemas.microsoft.com/sitka/2008/03/")]
        public string Id
        {
            get
            {
                if (String.IsNullOrEmpty(this.id))
                {
                    this.id = Guid.NewGuid().ToString();
                }
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity kind.
        /// </summary>
        [XmlIgnore]
        public string Kind
        {
            get
            {
                if (String.IsNullOrEmpty(kind))
                {
                    kind = typeof(T).Name;
                }
                return kind;
            }
            set
            {
                kind = value;
            }
        }

        /// <summary>
        /// Gets the version coming back from SSDS, don't bother setting
        /// </summary>
        [XmlElement(Namespace = @"http://schemas.microsoft.com/sitka/2008/03/")]
        public long Version { get; set; }

        /// <summary>
        /// Contains the actual object that was serialized back
        /// </summary>
        [XmlIgnore]
        public T Entity { get; set; }

        [XmlIgnore]
        public Dictionary<string, object> PropertyBucket
        {
            get { return propertyBucket; }
        }

        [XmlAnyElement]
        public XElement[] Attributes
        {
            get
            {
                //using XElement is much easier than XmlElement to build
                //take all properties on object instance and build XElement
                var props =  from prop in typeof(T).GetProperties()
                             let val = prop.GetValue(this.Entity, null)
                             where prop.GetSetMethod() != null && allowableTypes.Contains(prop.PropertyType) && val != null
                             select new XElement(prop.Name,
                                 new XAttribute(Constants.xsi + "type", XsdTypeResolver.Solve(prop.PropertyType)),
                                 EncodeValue(val)
                                 );

                //Then stuff in any extra stuff you want
                var extra = propertyBucket.Select(
                    e =>
                         new XElement(e.Key,
                            new XAttribute(Constants.xsi + "type", XsdTypeResolver.Solve(e.Value.GetType())),
                                EncodeValue(e.Value)
                                )
                    );

                return props.Union(extra).ToArray();
            }
            set
            {
                //wrap the XElement[] with the name of the type
                var xml = new XElement(typeof(T).Name, value);

                //var factory = new XmlSerializerFactory();
                //var xs = factory.CreateSerializer(typeof(T), new XmlRootAttribute(typeof(T).Name));

                //get serializer for T (not SsdsEntity<T>)
                var xs = SsdsEntitySerializer<T>.GetSerializer(typeof(T));

                //xml.CreateReader() cannot be used as it won't support base64 content
                XmlTextReader reader = new XmlTextReader(xml.ToString(), XmlNodeType.Document, null);

                this.Entity = (T)xs.Deserialize(reader);

                //now deserialize the other stuff left over into the property bucket...
                var stuff = from v in value.AsEnumerable()
                            let props = typeof(T).GetProperties().Select(s => s.Name)
                            where !props.Contains(v.Name.ToString())
                            select v;

                foreach (var item in stuff)
                {
                    propertyBucket.Add(
                        item.Name.ToString(),
                        DecodeValue(item.Attribute(Constants.xsi + "type").Value, item.Value)
                        );
                }
            }
        }

        public void Add<T>(string key, T value)
        {
            if (!allowableTypes.Contains(typeof(T)))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Type {0} not supported in SsdsEntity", typeof(T).Name));
            }

            if (!propertyBucket.ContainsKey(key))
            {
                propertyBucket.Add(key, value);
            }
            else
            {
                //replace the value
                propertyBucket.Remove(key);
                propertyBucket.Add(key, value);
            }
        }

        private static object DecodeValue(string xsiType, string encodedValue)
        {
            switch (xsiType)
            {
                case "x:decimal":
                    return Int32.Parse(encodedValue);
                case "x:boolean":
                    return Boolean.Parse(encodedValue);
                case "x:dateTime":
                    return DateTime.Parse(encodedValue).ToLocalTime();
                case "x:base64Binary":
                    return Convert.FromBase64String(encodedValue);
                default:
                    return encodedValue;
            }
        }

        private static object EncodeValue(object p)
        {
            if (p == null)
            {
                return null;
            }

            switch (Type.GetTypeCode(p.GetType()))
            {
                case TypeCode.Boolean:
                    return p.ToString().ToLower();
                case TypeCode.DateTime:
                    return ((DateTime)p).ToUniversalTime().ToString("u");
                case TypeCode.Object: //binary type
                    byte[] binary = p as byte[];
                    if (binary != null)
                        return Convert.ToBase64String(binary);
                    else
                        return null;
                default:
                    return p.ToString();
            }
        }

        static HashSet<Type> allowableTypes = new HashSet<Type>{
            typeof(string),
            typeof(int),
            typeof(DateTime),
            typeof(long),
            typeof(bool),
            typeof(byte[]),
            typeof(double),
            typeof(short),
            typeof(decimal),
        };
    }
}
