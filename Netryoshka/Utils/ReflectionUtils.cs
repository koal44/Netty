using System;

namespace Netryoshka.Utils
{
    public static class ReflectionUtils
    {
        public static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsNullable(Type t)
        {
            if (t.IsValueType)
            {
                return IsNullableType(t);
            }

            return true;
        }
    }
}
