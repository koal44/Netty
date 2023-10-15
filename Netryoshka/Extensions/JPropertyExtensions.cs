using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Netryoshka.Extensions
{
    public static class JPropertyExtensions
    {
        private static ConstructorInfo? _stringParameterConstructorInfo;
        private static ConstructorInfo StringParameterConstructorInfo
        {
            get
            {
                if (_stringParameterConstructorInfo == null)
                {
                    var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                    var args = new Type[] { typeof(string) };

                    _stringParameterConstructorInfo = typeof(JProperty).GetConstructor(flags, null, args, null)
                        ?? throw new InvalidOperationException("Could not find the expected JProperty constructor.");
                }

                return _stringParameterConstructorInfo;
            }
        }

        public static JProperty CreateFromPropertyName(string propertyName)
        {
            return (JProperty)StringParameterConstructorInfo.Invoke(new object[] { propertyName });
        }
    }

}
