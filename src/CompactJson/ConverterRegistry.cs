using System;
using System.Collections.Generic;
using System.Reflection;

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

            AddConverterFactory(new ListConverterFactory());
            AddConverterFactory(new ArrayConverterFactory());
            AddConverterFactory(new DictionaryConverterFactory());
            AddConverterFactory(new EnumConverterFactory());

            // fallback converter factory
            OBJECT_CONVERTER_FACTORY = new ObjectConverterFactory();
        }

        /// <summary>
        /// Adds a converter for a specific .NET type at a global scope.
        /// </summary>
        /// <param name="converter">The converter to add.</param>
        public static void AddConverter(IConverter converter)
        {
            lock (CONVERTERS)
                CONVERTERS.Add(converter.Type, converter);
        }

        /// <summary>
        /// Adds a <see cref="IConverterFactory"/> which is asked to convert types
        /// which are new to the <see cref="ConverterRegistry"/>.
        /// 
        /// If a type can be handled by multiple converter factories, the factory which
        /// has been added first, will be used to create a converter for the type.
        /// </summary>
        /// <param name="converterFactory">The converter factory to add.</param>
        public static void AddConverterFactory(IConverterFactory converterFactory)
        {
            lock (FACTORIES)
                FACTORIES.Add(converterFactory);
        }

        private static readonly Dictionary<Type, IConverter> CONVERTERS = new Dictionary<Type, IConverter>();
        private static readonly List<IConverterFactory> FACTORIES = new List<IConverterFactory>();
        private static readonly IConverterFactory OBJECT_CONVERTER_FACTORY;

        public static IConverter Get(Type type)
        {
            return Get(type, null);
        }

        private static string GetMemberNameString(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return "";

            return " of member '" + memberInfo.Name + "' of class " + memberInfo.DeclaringType.Name;
        }

        public static IConverter Get(Type type, ConverterParameters converterParameters)
        {
            lock (CONVERTERS)
            {
                IConverter converter;
                if (converterParameters == null && CONVERTERS.TryGetValue(type, out converter))
                    return converter;

                converter = CreateConverter(type, converterParameters);
                if (converterParameters == null)
                    CONVERTERS[type] = converter;
                return converter;
            }
        }

        private static IConverter CreateConverter(Type type, ConverterParameters converterParameters)
        {
            for (int i = 0; i < FACTORIES.Count; i++)
            {
                if (FACTORIES[i].CanConvert(type))
                    return FACTORIES[i].Create(type, converterParameters);
            }

            if (OBJECT_CONVERTER_FACTORY.CanConvert(type))
                return OBJECT_CONVERTER_FACTORY.Create(type, converterParameters);

            throw new Exception($"Unsupported model type '{type}'.");
        }
    }
}
