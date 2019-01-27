namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Globalization;

    /// <summary>
    /// Encodes the entity into the format used as Payload by SSDS.
    /// </summary>
    /// <typeparam name="T">The type of the entity to serialize.</typeparam>
    public class SsdsEntitySerializer<T> where T : class
    {        
        static Dictionary<string, XmlSerializer> _serializers = new Dictionary<string, XmlSerializer>();
        static object _lock = new object();

        /// <summary>
        /// Deserializes the given encoded string into an instance of T.
        /// </summary>
        /// <param name="encodedEntity">The xml string of the entity.</param>
        /// <returns>An instance of T with populated with the encoded data.</returns>
        public SsdsEntity<T> Deserialize(string entityXml)
        {
            return Deserialize(XElement.Parse(entityXml));
        }

        /// <summary>
        /// Serializes an entity to a string in the payload format required by SSDS
        /// </summary>
        /// <param name="entity">SsdsEntity wrapping the actual object</param>
        /// <returns></returns>
        public string Serialize(SsdsEntity<T> entity)
        {
            //add a bunch of namespaces and override the default ones too
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("s", Constants.ns.NamespaceName);
            namespaces.Add("x", Constants.x.NamespaceName);
            namespaces.Add("xsi", Constants.xsi.NamespaceName);

            //use the cached serializer for performance
            XmlSerializer xs = GetSerializer();

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.OmitXmlDeclaration = true;
        
            using (var ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, xws))
                {                    
                    xs.Serialize(writer, entity, namespaces);                    

                    ms.Position = 0; //reset to beginning

                    using (var sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Serializes an entity to a string in the payload format required by SSDS
        /// </summary>
        /// <param name="instance">The instance of the entity to serialize.</param>
        /// <param name="id">A unique id for the entity</param>
        /// <returns>An string containing the data of the entity in the payload format.</returns>
        public string Serialize(T instance, string id)
        {
            return Serialize(new SsdsEntity<T> { Entity = instance, Id = id });
        }

        public IEnumerable<SsdsEntity<T>> DeserializeMany(XElement response)
        {
            return response.Elements(typeof(T).Name).Select(f => Deserialize(f));
        }

        public SsdsEntity<T> Deserialize(XElement node)
        {      
            //Ryan - commenting out this XmlSerializer code - the issue is that
            //Deserialization of Generic types requires Full Trust for some reason
            //so we have to skirt around this issue by manually constructing the SsdsEntity
            //and then only calling Deserialize for the T.  Full Trust kills this from
            //running in Windows Azure
            
            // we have an issue here caching
            //the serializers... var xs = GetSerializer(node.Name.LocalName);

            ////xml.CreateReader() cannot be used as it won't support base64 content
            //XmlTextReader reader = new XmlTextReader(node.ToString(), XmlNodeType.Document, null);
            //SsdsEntity<T> result = (SsdsEntity<T>)xs.Deserialize(reader);
            //if (result != null)
            //{
            //    result.Kind = node.Name.LocalName;
            //}

            var result =  new SsdsEntity<T>
            {
                Id = node.Element(Constants.ns + "Id").Value,
                Version = Int64.Parse(node.Element(Constants.ns + "Version").Value, CultureInfo.InvariantCulture),
                Attributes = node.Elements().Where(f => f.Name.Namespace != Constants.ns).ToArray()
            };

            return result;
        }

        internal static XmlSerializer GetSerializer()
        {
            //calling this one returns a serializer for SsdsEntity<T>
            return GetSerializer(null);
        }

        internal static XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer xs = null;

            //indicates if we are using T or SsdsEntity<T>
            bool useBaseType = (type != null);

            string xmlName = useBaseType ? type.Name : typeof(T).Name;
            string cacheKeyName = useBaseType ? type.Name : typeof(SsdsEntity<T>).Name;

            //quick hit cache with no lock
            if (_serializers.ContainsKey(cacheKeyName))
                return _serializers[cacheKeyName];

            //otherwise, lock and create cache
            lock (_lock)
            {
                //check to see if someone beat you to it however.
                if (!_serializers.ContainsKey(cacheKeyName))
                {
                    //for whatever reason, this xmlserializer is not internally cached
                    //unless you use the ctor(string, type), so we have to cache it ourselves
                    var factory = new XmlSerializerFactory();

                    //passing in a specific type will create a serializer for that type and
                    //not just for SsdsEntity<T> - this is used in set for SsdsEntity
                    Type serializedType = useBaseType ? type : typeof(SsdsEntity<T>);

                    xs = factory.CreateSerializer(serializedType, new XmlRootAttribute(xmlName));
                    _serializers.Add(cacheKeyName, xs);
                }
                else
                {
                    xs = _serializers[cacheKeyName];
                }
            }
            return xs;
        }
    }
}