using System;
using System.Collections.Generic;
using System.Reflection;

namespace CompactJson
{
    internal static class ConverterFactoryHelper
    {
        // this dictionary prevents infinite recursion if properties have the same type as one
        //  of the declaring types in the object hierarchy.
        private static readonly Dictionary<Type, IConverter> CURRENTLY_INITIALIZED_TYPES = new Dictionary<Type, IConverter>();

        /// <summary>
        /// Creates a converter for the given type and the optionally given converter type.
        /// </summary>
        /// <param name="converterType">An optional converter type. This type can either be implementing
        /// <see cref="IConverter"/> or <see cref="IConverterFactory"/> or just be left null.</param>
        /// <param name="type">The type for which to create a converter.</param>
        /// <param name="converterParameters">Optional constructor parameters for converter to create.</param>
        /// <returns>The new converter.</returns>
        public static IConverter CreateConverter(Type converterType, Type type, object[] converterParameters)
        {
            // Get the factory. This may return null, if the given converter type is null.
            IConverterFactory factory = FromType(converterType);

            return GetConverter(factory, type, converterParameters);
        }

        /// <summary>
        /// Creates and initializes a converter. This takes care of nested types (recursive converter creation).
        /// </summary>
        /// <param name="factory">The converter factory to use. This must not be null.</param>
        /// <param name="type">The type for which to create the converter.</param>
        /// <param name="converterParameters">The constructor parameters for the new converter. This may be null.</param>
        /// <returns>The new converter.</returns>
        public static IConverter CreateConverter(IConverterFactory factory, Type type, object[] converterParameters)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            IConverter converter;
            lock (CURRENTLY_INITIALIZED_TYPES)
            {
                if (CURRENTLY_INITIALIZED_TYPES.TryGetValue(type, out converter))
                    return converter;

                converter = factory.Create(type, converterParameters);
                if (!(converter is IConverterInitialization))
                    return converter;

                CURRENTLY_INITIALIZED_TYPES[type] = converter;
            }
            try
            {
                ((IConverterInitialization)converter).InitializeConverter();
            }
            finally
            {
                lock (CURRENTLY_INITIALIZED_TYPES)
                    CURRENTLY_INITIALIZED_TYPES.Remove(type);
            }
            return converter;
        }

        private static IConverter GetConverter(IConverterFactory factory, Type type, object[] converterParameters)
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
