using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Netryoshka
{
    public static class JTokenExtensions
    {
        public static void SetLineInfo(this JToken token, IJsonLineInfo? lineInfo, JsonLoadSettings? settings)
        {
            if (settings != null && settings.LineInfoHandling != LineInfoHandling.Load)
            {
                return;
            }

            if (lineInfo == null || !lineInfo.HasLineInfo())
            {
                return;
            }

            token.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
        }

        public static void SetLineInfo(this JToken token, int lineNumber, int linePosition)
        {
            token.AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
        }

        private class LineInfoAnnotation
        {
            public readonly int LineNumber;
            public readonly int LinePosition;

            public LineInfoAnnotation(int lineNumber, int linePosition)
            {
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }
        }
    }

}
