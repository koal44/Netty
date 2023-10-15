using Netryoshka.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using static Netryoshka.Utils.JsonUtil;

namespace Netryoshka
{
    public static class JContainerExtensions
    {
        public static void ReadTokenFrom(this JContainer container, JsonReader reader, JsonLoadSettings? options)
        {
            int startDepth = reader.Depth;

            if (!reader.Read())
            {
                throw new JsonReaderException($"Error reading {container.GetType().Name} from JsonReader.");
            }

            container.ReadContentFrom(reader, options);

            int endDepth = reader.Depth;

            if (endDepth > startDepth)
            {
                throw new JsonReaderException($"Unexpected end of content while loading {container.GetType().Name}.");
            }
        }


        public static void ReadContentFrom(this JContainer container, JsonReader reader, JsonLoadSettings? settings)
        {
            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;
            JContainer? parent = container;

            do
            {
                if (parent is JProperty p && p.Value != null)
                {
                    if (parent == container)
                    {
                        return;
                    }

                    parent = parent.Parent;
                }

                if (parent == null) throw new JsonReaderException("Unexpected null parent");

                switch (reader.TokenType)
                {
                    case JsonToken.None:
                        // new reader. move to actual content
                        break;
                    case JsonToken.StartArray:
                        var a = new JArray();
                        a.SetLineInfo(lineInfo, settings);
                        parent.Add(a);
                        parent = a;
                        break;

                    case JsonToken.EndArray:
                        if (parent == container)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartObject:
                        var o = new JObject();
                        o.SetLineInfo(lineInfo, settings);
                        parent.Add(o);
                        parent = o;
                        break;
                    case JsonToken.EndObject:
                        if (parent == container)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartConstructor:
                        var constructor = new JConstructor(reader.Value!.ToString()!);
                        constructor.SetLineInfo(lineInfo, settings);
                        parent.Add(constructor);
                        parent = constructor;
                        break;
                    case JsonToken.EndConstructor:
                        if (parent == container)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                        var v = new JValue(reader.Value);
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Comment:
                        if (settings != null && settings.CommentHandling == CommentHandling.Load)
                        {
                            v = JValue.CreateComment(reader.Value!.ToString());
                            v.SetLineInfo(lineInfo, settings);
                            parent.Add(v);
                        }
                        break;
                    case JsonToken.Null:
                        v = JValue.CreateNull();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Undefined:
                        v = JValue.CreateUndefined();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.PropertyName:
                        JProperty? property = ReadProperty(reader, settings, lineInfo, parent);
                        if (property != null)
                        {
                            parent = property;
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"The JsonReader should not be on a token of type {reader.TokenType}.");
                }
            } while (reader.Read());
        }


        public static JProperty? ReadProperty(JsonReader r, JsonLoadSettings? settings, IJsonLineInfo? lineInfo, JContainer parent)
        {
            if (settings is not CustomLoadSettings customSettings)
            {
                throw new ArgumentException("Expected CustomLoadSettings", nameof(settings));
            }

            JObject parentObject = (JObject)parent;
            string propertyName = r.Value!.ToString()!;

            JProperty? existingPropertyWithName = parentObject.Property(propertyName, StringComparison.Ordinal);

            if (existingPropertyWithName != null)
            {
                if (customSettings.PropertiesToIgnore.Contains(propertyName))
                {
                    return null; // Ignore this duplicate (supposedly a converter will handle it)
                }
                else if (customSettings.PropertiesToConsider.Contains(propertyName))
                {
                    throw new JsonReaderException($"Duplicate property '{propertyName}' found and it's not marked to be ignored.");
                }
            }

            //var property = new JProperty(propertyName);
            var property = JPropertyExtensions.CreateFromPropertyName(propertyName);
            property.SetLineInfo(lineInfo, settings);

            if (existingPropertyWithName == null)
            {
                parent.Add(property);
            }
            else
            {
                existingPropertyWithName.Replace(property);
            }

            return property;
        }

    }

}
