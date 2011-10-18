using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics.CodeAnalysis;

namespace civilsalary.data
{
    static class UtilityExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string Serialize(this XmlObjectSerializer serializer, object value)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");

            using(var sw = new StringWriter())
            {
                using(var xml = new XmlTextWriter(sw))
                {
                    serializer.WriteObject(xml, value);
                }
                return sw.GetStringBuilder().ToString();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static object Deserialize(this XmlObjectSerializer serializer, string xml)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");

            using (var sr = new StringReader(xml))
            {
                using (var xmlR = new XmlTextReader(sr))
                {
                    var obj = serializer.ReadObject(xmlR);
                    return obj;
                }
            }
        }

    }
}