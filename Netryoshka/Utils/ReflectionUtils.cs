using System;
using System.Reflection;

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


        /// <summary>
        /// Finds the base type where a particular property is declared.
        /// </summary>
        /// <param name="type">The type to start searching from.</param>
        /// <param name="propName">The name of the property to find.</param>
        /// <returns>The base type where the property is declared, or <c>null</c> if the property is not found.</returns>
        /// <example>
        /// <code>
        /// Type? declaringType = GetDeclaringBaseTypeOfProperty(typeof(TreeView), "ThemeStyle");
        /// </code>
        /// </example>
        public static Type? GetDeclaringBaseTypeOfProperty(Type? type, string propName)
        {
            Type? lastTypeWherePropertyExists = null;

            while (type != null)
            {
                var propInfo = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo != null)
                {
                    lastTypeWherePropertyExists = type;
                }
                else
                {
                    break;
                }

                type = type.BaseType;
            }

            return lastTypeWherePropertyExists;
        }


        /// <summary>
        /// Retrieves a non-public property of a given type from a specified object. 
        /// This method will traverse the inheritance chain to find the property if not present in the given object's type.
        /// </summary>
        /// <typeparam name="PropType">The expected type of the property to retrieve.</typeparam>
        /// <param name="obj">The object from which to retrieve the property.</param>
        /// <param name="propName">The name of the non-public property to retrieve.</param>
        /// <returns>The value of the specified non-public property, cast to the given type.</returns>
        /// <exception cref="Exception">Thrown when the specified property is not found or is not of the expected type.</exception>
        public static PropType GetNonPublicProperty<PropType>(object obj, string propName)
        {
            Type? type = obj.GetType();
            var propInfo = type.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (propInfo != null)
            {
                object? value = propInfo.GetValue(obj);
                if (value is PropType prop)
                {
                    return prop;
                }
            }
            throw new Exception($"Could not get {propName} property");
        }
    }
}
