﻿using System;

namespace CompactJson
{
    /// <summary>
    /// A converter which can be used to encode type information
    /// into JSON objects.
    /// This is helpful to support serialization and deserialization
    /// of class hierarchies.
    /// If class 'A' is base class of class 'B' you may add <see cref="TypeNameAttribute"/>s
    /// to class 'A' to assign names to these classes.
    /// Additionally, add the <see cref="TypedConverterFactory"/> using the
    /// <see cref="CustomConverterAttribute"/>.
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

        /// <summary>
        /// Initializes a new <see cref="TypedConverter"/> with the given type.
        /// <see cref="TypeNameAttribute"/>s must be present at the given type.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <param name="typeProperty">The name of the JSON property which should
        /// be used to encode the type information.</param>
        public TypedConverter(Type type, string typeProperty)
            : base(type)
        {
            TypeNameAttribute[] attributes = TypeNameAttribute.GetKnownTypes(type);
            if (attributes == null)
                throw new Exception($"Type {type} is not supported by {nameof(TypedConverter)} due to missing {nameof(TypeNameAttribute)}s.");

            TypeNameResolver resolver = new TypeNameResolver();
            foreach (var attribute in attributes)
            {
                if (!type.IsAssignableFrom(attribute.Type))
                    throw new Exception($"Type '{attribute.Type}' cannot be assigned a name using the {nameof(TypeNameAttribute)} because it does not inherit from '{type}'.");

                resolver.AddType(attribute.TypeName, attribute.Type);
            }

            mTypeNameResolver = resolver;
            mTypeProperty = typeProperty ?? throw new ArgumentNullException(nameof(typeProperty));
        }

        /// <summary>
        /// Initializes a new <see cref="TypedConverter"/> with the given type.
        /// The given type name resolver removes the need for <see cref="TypeNameAttribute"/>s
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
            if (!mTypeNameResolver.TryGetTypeName(type, out string typeName))
                throw new Exception($"Type '{type}' is unknown.");

            IConverter converter = ConverterRegistry.Get(type);
            converter.Write(value, new WritingConsumer(typeName, mTypeProperty, type, writer));
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
                return mWrappedConsumer.Array();
            }

            public void Boolean(bool value)
            {
                mWrappedConsumer.Boolean(value);
            }

            public void Null()
            {
                mWrappedConsumer.Null();
            }

            public void Number(double value)
            {
                mWrappedConsumer.Number(value);
            }

            public void Number(long value)
            {
                mWrappedConsumer.Number(value);
            }

            public IJsonObjectConsumer Object()
            {
                return mWrappedConsumer.Object();
            }

            public void String(string value)
            {
                if (mWrappedConsumer == null)
                {
                    if (!mParent.mTypeNameResolver.TryGetType(value, out Type type))
                        throw new Exception($"Type name '{value}' is unknown while deserializing type '{mBaseType}'.");

                    IConverter converter = ConverterRegistry.Get(type);
                    mWrappedConsumer = converter.FromObject(mWhenDone);
                }
                else
                    mWrappedConsumer.String(value);
            }

            public void PropertyName(string propertyName)
            {
                if (mWrappedConsumer == null)
                {
                    if (propertyName != mParent.mTypeProperty)
                        throw new Exception($"The property '{mParent.mTypeProperty}' was expected to be the first property in the JSON object when deserializing type '{mBaseType}', but found property '{propertyName}'.");
                }
                else
                    mWrappedConsumer.PropertyName(propertyName);
            }

            public void Done()
            {
                if (mWrappedConsumer == null)
                    throw new Exception($"The property '{mParent.mTypeProperty}' must be present in the JSON object when deserializing type '{mBaseType}'.");

                mWrappedConsumer.Done();
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

            public IJsonObjectConsumer Object()
            {
                IJsonObjectConsumer obj = mWrappedConsumer.Object();
                obj.PropertyName(mTypeProperty);
                obj.String(mTypeName);
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