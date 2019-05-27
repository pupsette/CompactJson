using System;
using System.Reflection;

namespace CompactJson
{
    internal static class ConverterFactoryHelper
    {
        public static IConverter CreateConverter(IConverterFactory factory, Type type, ConverterParameters converterParameters)
        {
            if (factory != null)
            {
                if (!factory.CanConvert(type))
                    throw new Exception($"Converter factory {factory.GetType()} is not able to convert type '{type}'.");

                IConverter converter = factory.Create(type, converterParameters);
                if (converter == null)
                    throw new Exception($"{nameof(IConverterFactory.Create)} implementation of {factory.GetType()} did not return a converter for type {type}.");

                return converter;
            }

            return ConverterRegistry.Get(type, converterParameters);
        }

        /// <summary>
        /// Creates a converter factory from the given type. The given type may either
        /// implement <see cref="IConverter"/> or <see cref="IConverterFactory"/> or just 
        /// be null. In case type is null, the returned converter factory is also null.
        /// </summary>
        /// <param name="converterOrFactoryType">The type implementing either
        /// <see cref="IConverter"/> or <see cref="IConverterFactory"/>.</param>
        /// <returns>The converter factory or null, if <paramref name="converterOrFactoryType"/>
        /// is null.</returns>
        public static IConverterFactory FromType(Type converterOrFactoryType)
        {
            if (converterOrFactoryType == null)
                return null;

            if (typeof(IConverter).IsAssignableFrom(converterOrFactoryType))
                return new FixedConverterFactory((IConverter)Instantiate(converterOrFactoryType));

            if (typeof(IConverterFactory).IsAssignableFrom(converterOrFactoryType))
                return (IConverterFactory)Instantiate(converterOrFactoryType);

            throw new Exception($"{converterOrFactoryType} must implement either {nameof(IConverter)} or {nameof(IConverterFactory)}.");
        }

        private class FixedConverterFactory : IConverterFactory
        {
            private readonly IConverter converter;

            public FixedConverterFactory(IConverter converter)
            {
                this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            }

            public bool CanConvert(Type type)
            {
                return converter.Type == null || converter.Type == type;
            }

            public IConverter Create(Type type, ConverterParameters parameters)
            {
                return converter;
            }
        }

        private static object Instantiate(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new Exception($"Type '{type}' must have a default constructor.");
            return constructor.Invoke(null);
        }
    }
}
