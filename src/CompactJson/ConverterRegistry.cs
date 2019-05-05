using System;
using System.Collections.Generic;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    static class ConverterRegistry
    {
        static ConverterRegistry()
        {
            AddConverter(new Int32Converter(false));
            AddConverter(new Int32Converter(true));
            AddConverter(new Int64Converter(false));
            AddConverter(new Int64Converter(true));
            AddConverter(new DoubleConverter(false));
            AddConverter(new DoubleConverter(true));
            AddConverter(new FloatConverter(false));
            AddConverter(new FloatConverter(true));
            AddConverter(new StringConverter());
            AddConverter(new BooleanConverter(false));
            AddConverter(new BooleanConverter(true));
            AddConverter(new DateTimeConverter(false));
            AddConverter(new DateTimeConverter(true));
            AddConverter(new UnspecificConverter());
            AddConverter(new JsonValueConverter(typeof(JsonValue), true, true, true, true, true, true, true, false));
            AddConverter(new JsonValueConverter(typeof(JsonObject), allowJsonObject: true, acceptNull: true));
            AddConverter(new JsonValueConverter(typeof(JsonArray), allowJsonArray: true, acceptNull: true));
            AddConverter(new JsonValueConverter(typeof(JsonNumber), allowJsonFloat: true, allowJsonLong: true));
            AddConverter(new JsonValueConverter(typeof(JsonBoolean), allowJsonBoolean: true));
            AddConverter(new JsonValueConverter(typeof(JsonString), allowJsonString: true, acceptNull: true));
        }

        /// <summary>
        /// Adds a converter for a specific .NET type at a global scope.
        /// </summary>
        /// <param name="converter">The converter to add.</param>
        public static void AddConverter(IConverter converter)
        {
            lock (TYPES)
                TYPES.Add(converter.Type, converter);
        }

        private static readonly Dictionary<Type, IConverter> TYPES = new Dictionary<Type, IConverter>();

        public static IConverter Get(Type type)
        {
            lock (TYPES)
            {
                IConverter converter;
                if (TYPES.TryGetValue(type, out converter))
                    return converter;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = type.GetGenericArguments()[0];
                    converter = new ListConverter(type, Get(elementType), false);
                    TYPES[type] = converter;
                }
                else if (type.IsEnum)
                {
                    converter = new EnumConverter(type);
                    TYPES[type] = converter;
                }
                else if (type.IsArray)
                {
                    Type elementType = type.GetElementType();
                    converter = new ListConverter(type, Get(elementType), true);
                    TYPES[type] = converter;
                }
                else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
                {
                    ObjectConverter result = new ObjectConverter(type);
                    TYPES[type] = converter = result;
                    result.Reflect();
                }
                else
                    throw new Exception($"Unsupported model type '{type}'.");

                return converter;
            }
        }
    }
}
