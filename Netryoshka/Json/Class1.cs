using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netryoshka.Json
{
    internal class Class1
    {
        public static JArray Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None && !reader.Read())
            {
                throw new Exception("Error reading JArray from JsonReader.");
            }

            AdvanceReaderToContent(reader);

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception("Error reading JArray from JsonReader");
            }

            var a = new JArray();
            a.SetLineInfo(reader as IJsonLineInfo, settings);

            a.ReadTokenFrom(reader, settings);

            return a;
        }



        public static bool AdvanceReaderToContent(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read()) return false;
            }
            return true;
        }

    }


    
}
