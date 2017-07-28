using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace LogViewer
{
    [DataContract]
    public class ApplicationData
    {
        [DataMember]
        public List<string> Ips = new List<string>();

        [DataMember]
        public List<string> Pids = new List<string>();

        [DataMember]
        public List<string> Tids = new List<string>();

        [DataMember]
        public List<string> Methods = new List<string>();

        public static ApplicationData ReadFromJson(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(ApplicationData));
                        return (ApplicationData)serializer.ReadObject(fs);
                    }
                }
                catch (SerializationException)
                {
                    return new ApplicationData();
                }
            }
            else
            {
                return new ApplicationData();
            }
        }

        public static void WriteToJson(string file, ApplicationData data)
        {
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var serializer = new DataContractJsonSerializer(typeof(ApplicationData));
                serializer.WriteObject(fs, data);
            }
        }
    }
}
