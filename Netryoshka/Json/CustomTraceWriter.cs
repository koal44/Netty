using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Linq;

namespace Netryoshka.Json
{
    public class CustomTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception? ex)
        {
            var exText = ex != null ? $"Exception: {ex.Message}" : string.Empty;
            var lvlText = $"Level: {level}";
            var msgText = $"Message: {message}";

            var components = new[] { lvlText, msgText, exText }
                             .Where(s => !string.IsNullOrEmpty(s));

            Debug.WriteLine(string.Join(", ", components));
        }
    }
}
