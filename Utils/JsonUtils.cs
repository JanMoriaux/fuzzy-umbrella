using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace DotNetModules.Utils
{
    public class JsonUtils
    {
        //creates a JSON string to use as message content
        public static String CreateJsonString(string content, string configuration)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;

                writer.WriteStartArray();
                writer.WriteStartObject();
                writer.WritePropertyName("device");
                writer.WriteValue(configuration);
                writer.WriteEndObject();
                writer.WriteStartObject();
                writer.WritePropertyName("content");
                writer.WriteValue(content);
                writer.WriteEndObject();
                writer.WriteEnd();
            }
            return sb.ToString();
        }
    }
}

