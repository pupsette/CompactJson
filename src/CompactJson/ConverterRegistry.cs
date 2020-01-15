using System;
using System.Collections.Generic;

namespace CompactJson
{
    /// <summary>
    /// The static converter registry. It is used to register
    /// type-specific converters or converter factories at the global scope.
    /// 
    /// A lot of type-specific converters and factories are registered
    /// implicitly.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    static class ConverterRegistry
    {
        static ConverterRegistry()
        {
            AddConverter(new ByteArrayConverter());
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
            AddConverter(new GuidConverter(false));
            AddConverter(new GuidConverter(true));
            AddConverter(new UnspecificConverter());
            AddConverter(new JsonValueConverter(typeof(JsonValue), true, true, true, true, true, true, true, false));
            AddConverter(new JsonValueConverter(typeof(JsonObject), allowJsonObject: true, acceptNull: true));
            AddConverter(new JsonValueConverter(typeof(JsonArray), allowJsonArray: true, acceptNull: true));
            AddConverter(new JsonValueConverter(typeof(JsonNumber), allowJsonFloat: true, allowJsonLong: true));
            AddConverter(new JsonValueConverter(typeof(JsonBoolean), allowJsonBoolean: true));
            AddConverter(new JsonValueConverter(typeof(JsonString), allowJsonString: true, acceptNull: true));

            AddConverterFactory(new ObjectConverterFactory());
            AddConverterFactory(new ListConverterFactory());
            AddConverterFactory(new ArrayConverterFactory());
            AddConverterFactory(new DictionaryConverterFactory());
            AddConverterFactory(new EnumConverterFactory());
            AddConverterFactory(new CustomConverterFactory());
        }

        /// <summary>
        /// Adds a converter for a specific .NET type at a global scope. If a converter for
        /// the type is already present, it will be replaced by the given one.
        /// </summary>
        /// <param name="converter">The converter to add.</param>
        public static void AddConverter(IConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (converter.Type == null)
                throw new ArgumentException($"The 'Type' property of the given converter ({converter}) must not be null.");

            lock (CONVERTERS)
                CONVERTERS[converter.Type] = converter;
        }

        /// <summary>
        /// Adds a <see cref="IConverterFactory"/> which is asked to convert types
        /// which are new to the <see cref="ConverterRegistry"/>.
        /// 
        /// If a type can be handled by multiple converter factories, the factory which
        /// has been added last, will be used to create a converter for the type.
        /// </summary>
        /// <param name="converterFactory">The converter factory to add.</param>
        public static void AddConverterFactory(IConverterFactory converterFactory)
        {
            if (converterFactory == null)
                throw new ArgumentNullException(nameof(converterFactory));

            lock (FACTORIES)
                FACTORIES.Add(converterFactory);
        }

        private static readonly Dictionary<Type, IConverter> CONVERTERS = new Dictionary<Type, IConverter>();
        private static readonly List<IConverterFactory> FACTORIES = new List<IConverterFactory>();
        private static readonly Dictionary<Type, int> CURRENTLY_CREATING_CONVERTERS = new Dictionary<Type, int>();

        /// <summary>
        /// Returns the type-specific converter for the given type.
        /// An exception is thrown if there is no suitable converter for
        /// the given type.
        /// </summary>
        /// <param name="type">The type for which to get the converter.</param>
        /// <returns>The converter for the given type.</returns>
        public static IConverter Get(Type type)
        {
            return Get(type, null);
        }

        /// <summary>
        /// Returns the type-specific converter for the given type.
        /// An exception is thrown if there is no suitable converter for
        /// the given type.
        /// </summary>
        /// <param name="type">The type for which to get the converter.</param>
        /// <param name="converterParameters">An optional set of converter parameters.</param>
        /// <returns>The converter for the given type.</returns>
        public static IConverter Get(Type type, object[] converterParameters)
        {
            lock (CONVERTERS)
            {
                IConverter converter;
                if (converterParameters == null && CONVERTERS.TryGetValue(type, out converter))
                    return converter;

                converter = CreateConverter(type, converterParameters);
                if (converterParameters == null || converterParameters.Length == 0)
                    CONVERTERS[type] = converter;
                return converter;
            }
        }

        private static IConverter CreateConverter(Type type, object[] converterParameters)
        {
            int factoryStartIndex;
            // recursive calls will by default skip the previously used converter factory.
            bool isRecursion = CURRENTLY_CREATING_CONVERTERS.TryGetValue(type, out factoryStartIndex);
            if (!isRecursion)
                factoryStartIndex = FACTORIES.Count - 1;

            // walk the factories from last to first
            for (int i = factoryStartIndex; i >= 0; i--)
            {
                if (FACTORIES[i].CanConvert(type))
                {
                    CURRENTLY_CREATING_CONVERTERS[type] = i - 1;
                    try
                    {
                        return FACTORIES[i].Create(type, converterParameters);
                    }
                    finally
                    {
                        if (isRecursion)
                            CURRENTLY_CREATING_CONVERTERS[type] = factoryStartIndex; // reset to the previous index
                        else
                            CURRENTLY_CREATING_CONVERTERS.Remove(type); // clear any start index
                    }
                }
            }

            throw new Exception($"Unsupported model type '{type}'.");
        }
    }
}
