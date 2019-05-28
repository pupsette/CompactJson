using System;
using System.Reflection;

namespace CompactJson
{
    internal static class ConverterFactoryHelper
    {
        public static IConverter CreateConverter(Type converterType, Type type, object[] converterParameters)
        {
            IConverterFactory factory = FromType(converterType);
            return CreateConverter(factory, type, converterParameters);
        }

        private static IConverter CreateConverter(IConverterFactory factory, Type type, object[] converterParameters)
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
        private static IConverterFactory FromType(Type converterOrFactoryType)
        {
            if (converterOrFactoryType == null)
                return null;

            if (typeof(IConverter).IsAssignableFrom(converterOrFactoryType))
                return new FixedConverterFactory((IConverter)Instantiate(converterOrFactoryType));

            if (typeof(IConverterFactory).IsAssignableFrom(converterOrFactoryType))
                return (IConverterFactory)Instantiate(converterOrFactoryType);

            throw new Exception($"{converterOrFactoryType} must implement either {nameof(IConverter)} or {nameof(IConverterFactory)}.");
        }

        public static T GetConverterParameter<T>(Type converterFactory, object[] parameters, int index, int minParameters, int maxParameters)
        {
            if (parameters == null)
            {
                if (minParameters > 0)
                    throw new Exception($"{converterFactory} expects at least {minParameters} converter parameters.");

                return default(T);
            }
            if (parameters.Length < minParameters)
                throw new Exception($"{converterFactory} expects at least {minParameters} converter parameters, but only {parameters.Length} were given.");

            if (parameters.Length > maxParameters)
                throw new Exception($"{converterFactory} cannot take more than {maxParameters} converter parameters, but {parameters.Length} were given.");

            if (index < parameters.Length && parameters[index] != null)
                return (T)parameters[index];

            return default(T);
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

            public IConverter Create(Type type, object[] parameters)
            {
                if (parameters != null && parameters.Length > 0)
                    throw new Exception($"{type} does not take additional converter parameters.");

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
