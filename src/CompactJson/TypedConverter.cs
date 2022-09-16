using System;
using System.Collections.Generic;

namespace CompactJson
{
    /// <summary>
    /// A converter which can be used to encode type information
    /// into JSON objects.
    /// This is helpful to support serialization and deserialization
    /// of class hierarchies.
    /// If class 'A' is base class of class 'B' you may add <see cref="JsonTypeNameAttribute"/>s
    /// to class 'A' to assign names to these classes.
    /// Additionally, add the <see cref="TypedConverterFactory"/> using the
    /// <see cref="JsonCustomConverterAttribute"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypedConverter : ConverterBase
    {
        private readonly ITypeNameResolver mTypeNameResolver;
        private readonly string mTypeProperty;
        private readonly ObjectConverter mBaseClassConverter;

        /// <summary>
        /// Initializes a new <see cref="TypedConverter"/> with the given type.
        /// <see cref="JsonTypeNameAttribute"/>s must be present at the given type.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <param name="typeProperty">The name of the JSON property which should
        /// be used to encode the type information.</param>
        public TypedConverter(Type type, string typeProperty)
            : this(type, CreateTypeNameResolverFromAttributes(type), typeProperty)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TypedConverter"/> with the given type.
        /// The given type name resolver removes the need for <see cref="JsonTypeNameAttribute"/>s
        /// to be present.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <param name="typeNameResolver">A type name resolver is already populated
        /// with type name mappings.</param>
        /// <param name="typeProperty">The name of the JSON property which should
        /// be used to encode the type information.</param>
        public TypedConverter(Type type, ITypeNameResolver typeNameResolver, string typeProperty)
            : base(type)
        {
            mBaseClassConverter = new ObjectConverter(type);
            ((IConverterInitialization)mBaseClassConverter).InitializeConverter();
            mTypeNameResolver = typeNameResolver ?? throw new ArgumentNullException(nameof(typeNameResolver));
            mTypeProperty = typeProperty ?? throw new ArgumentNullException(nameof(typeProperty));
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an object
        /// begins. This method must return an object consumer which
        /// is used to pass the property data of the object. Once, there are 
        /// no more properties, the <paramref name="whenDone"/> callback must 
        /// be called by the <see cref="IJsonObjectConsumer"/> implementation 
        /// in order to pass the resulting .NET object.
        /// </summary>
        /// <param name="whenDone">A callback which must be used to pass
        /// the resulting .NET object, when Done() of the object consumer
        /// gets called.</param>
        /// <returns>An implementation of <see cref="IJsonObjectConsumer"/> which
        /// is used to consume the properties of the JSON object.</returns>
        public override IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            return new ReadingConsumer(Type, this, whenDone);
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a 'null'
        /// value was parsed/produced.
        /// 
        /// The implementation in <see cref="TypedConverter"/> will return null.
        /// </summary>
        /// <returns>Returns null.</returns>
        public override object FromNull()
        {
            return null;
        }

        /// <summary>
        /// Writes data of the .NET object to the given <see cref="IJsonConsumer"/>
        /// in order to convert it to a JSON string or to any other representation of
        /// JSON data.
        /// </summary>
        /// <param name="value">The .NET object to write. Implementors may assume
        /// that the type of the incoming value can be cast to the target type 
        /// of this converter, however, it may be null.</param>
        /// <param name="writer">The JSON consumer which must be used to write 
        /// the data to according to the contents of the given value.</param>
        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
            {
                writer.Null();
                return;
            }

            Type type = value.GetType();
            if (!mTypeNameResolver.TryGetTypeName(type, out string typeName) && type != mTypeNameResolver.DefaultType)
                throw new Exception($"Type '{type}' is unknown.");

            IConverter converter = GetConverterForType(type);
            converter.Write(value, new WritingConsumer(typeName, mTypeProperty, type, writer));
        }

        private IConverter GetConverterForType(Type type)
        {
            if (type == this.Type)
                return mBaseClassConverter;
            else
            {
                var converter = ConverterRegistry.Get(type);
                if (!(converter is TypedConverter typedConverter))
                    throw new Exception($"Expected converter registry to return an instance of {nameof(TypedConverter)} for type '{type}'.");

                return typedConverter.mBaseClassConverter;
            }
        }

        private static ITypeNameResolver CreateTypeNameResolverFromAttributes(Type type)
        {
            JsonTypeNameAttribute[] attributes = JsonTypeNameAttribute.GetKnownTypes(type);
            if (attributes == null)
                throw new Exception($"Type {type} is not supported by {nameof(TypedConverter)} due to missing {nameof(JsonTypeNameAttribute)}s.");

            Type baseType = FindBaseClass(type);

            TypeNameResolver resolver = new TypeNameResolver();
            foreach (var attribute in attributes)
            {
                if (!baseType.IsAssignableFrom(attribute.Type))
                    throw new Exception($"Type '{attribute.Type}' cannot be assigned a name using the {nameof(JsonTypeNameAttribute)} because it does not inherit from '{type}'.");

                if (attribute.TypeName == null)
                {
                    if (resolver.DefaultType != null)
                        throw new Exception($"Type '{attribute.Type}' and '{resolver.DefaultType}' cannot both be the default type when serializaing '{baseType}'.");

                    resolver.DefaultType = attribute.Type;
                }
                else
                    resolver.AddType(attribute.TypeName, attribute.Type);
            }
            return resolver;
        }

        private static Type FindBaseClass(Type type)
        {
            while (type != null && type != typeof(object))
            {
                JsonCustomConverterAttribute attr = (JsonCustomConverterAttribute)Attribute.GetCustomAttribute(type, typeof(JsonCustomConverterAttribute), false);
                if (attr != null && attr.ConverterType == typeof(TypedConverterFactory))
                    return type;

                type = type.BaseType;
            }
            throw new Exception($"{nameof(TypedConverterFactory)} was not found somewhere in the inheritence chain of {type.Name}.");
        }

        private class ReadingConsumer : IJsonObjectConsumer
        {
            private IJsonObjectConsumer mWrappedConsumer;
            private readonly Type mBaseType;
            private readonly TypedConverter mParent;
            private readonly Action<object> mWhenDone;

            public ReadingConsumer(Type baseType, TypedConverter parent, Action<object> whenDone)
            {
                this.mBaseType = baseType;
                this.mParent = parent;
                this.mWhenDone = whenDone;
            }

            public IJsonArrayConsumer Array()
            {
                ThrowIfNoConsumer();
                return mWrappedConsumer.Array();
            }

            public void Boolean(bool value)
            {
                ThrowIfNoConsumer();
                mWrappedConsumer.Boolean(value);
            }

            public void Null()
            {
                ThrowIfNoConsumer();
                mWrappedConsumer.Null();
            }

            public void Number(double value)
            {
                ThrowIfNoConsumer();
                mWrappedConsumer.Number(value);
            }

            public void Number(long value)
            {
                ThrowIfNoConsumer();
                mWrappedConsumer.Number(value);
            }

            public void Number(ulong value)
            {
                ThrowIfNoConsumer();
                mWrappedConsumer.Number(value);
            }

            public IJsonObjectConsumer Object()
            {
                ThrowIfNoConsumer();
                return mWrappedConsumer.Object();
            }

            public void String(string value)
            {
                if (mWrappedConsumer == null)
                {
                    if (!mParent.mTypeNameResolver.TryGetType(value, out Type type))
                        throw new Exception($"Type name '{value}' is unknown while deserializing type '{mBaseType}'.");

                    mWrappedConsumer = mParent.GetConverterForType(type).FromObject(mWhenDone);
                }
                mWrappedConsumer.String(value);
            }

            public void PropertyName(string propertyName)
            {
                if (propertyName.Equals(mParent.mTypeProperty, StringComparison.OrdinalIgnoreCase))
                {
                    if (mWrappedConsumer != null)
                        throw new Exception($"The property '{mParent.mTypeProperty}' must only appear once in the JSON object when deserializing type '{mBaseType}'.");

                    return;
                }
                else
                {
                    if (mWrappedConsumer == null && !TryCreateDefaultType())
                        throw new Exception($"The property '{mParent.mTypeProperty}' must be the first property in the JSON object when deserializing type '{mBaseType}'.");
                }

                mWrappedConsumer.PropertyName(propertyName);
            }

            public void Done()
            {
                if (mWrappedConsumer == null && !TryCreateDefaultType())
                    throw new Exception($"The property '{mParent.mTypeProperty}' must be present in the JSON object when deserializing type '{mBaseType}'.");

                mWrappedConsumer.Done();
            }

            private void ThrowIfNoConsumer()
            {
                if (mWrappedConsumer == null)
                    throw new Exception($"The property '{mParent.mTypeProperty}' must be a string when deserializing type '{mBaseType}'.");
            }

            private bool TryCreateDefaultType()
            {
                if (mParent.mTypeNameResolver.DefaultType == null)
                    return false;

                mWrappedConsumer = mParent.GetConverterForType(mParent.mTypeNameResolver.DefaultType).FromObject(mWhenDone);
                return true;
            }
        }

        private class WritingConsumer : IJsonConsumer
        {
            private readonly string mTypeName;
            private readonly string mTypeProperty;
            private readonly Type mType;
            private readonly IJsonConsumer mWrappedConsumer;

            public WritingConsumer(string typeName, string typeProperty, Type type, IJsonConsumer wrappedConsumer)
            {
                this.mTypeName = typeName;
                this.mTypeProperty = typeProperty;
                this.mType = type;
                this.mWrappedConsumer = wrappedConsumer;
            }

            public IJsonArrayConsumer Array()
            {
                Refuse();
                return null; // will never be reached
            }

            public void Boolean(bool value)
            {
                Refuse();
            }

            public void Null()
            {
                mWrappedConsumer.Null();
            }

            public void Number(double value)
            {
                Refuse();
            }

            public void Number(long value)
            {
                Refuse();
            }

            public void Number(ulong value)
            {
                Refuse();
            }

            public IJsonObjectConsumer Object()
            {
                IJsonObjectConsumer obj = mWrappedConsumer.Object();
                if (mTypeName != null)
                {
                    obj.PropertyName(mTypeProperty);
                    obj.String(mTypeName);
                }
                return obj;
            }

            public void String(string value)
            {
                Refuse();
            }

            private void Refuse()
            {
                throw new Exception($"The converter for type {mType} (and type name '{mTypeName}') was expected to write a JSON object.");
            }
        }
    }
}
