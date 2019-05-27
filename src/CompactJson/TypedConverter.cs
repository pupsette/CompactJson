using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypedConverter : ConverterBase
    {
        private readonly ITypeNameResolver typeNameResolver;
        public static string TYPE_PROPERTY = "Type";

        public TypedConverter(Type type)
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

                IConverterFactory customFactory = ConverterFactoryHelper.FromType(attribute.CustomConverterType);
                IConverter converter;
                if (attribute.Type != type || customFactory != null)
                    converter = ConverterFactoryHelper.CreateConverter(customFactory, attribute.Type, null);
                else
                    converter = ObjectConverterFactory.Create(type);

                resolver.AddType(attribute.TypeName, attribute.Type, converter);
            }

            typeNameResolver = resolver;
        }

        public TypedConverter(Type type, ITypeNameResolver typeNameResolver)
            : base(type)
        {
            this.typeNameResolver = typeNameResolver;
        }

        public override IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            return new ReadingConsumer(Type, typeNameResolver, whenDone);
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
            {
                writer.Null();
                return;
            }

            Type type = value.GetType();
            if (!typeNameResolver.TryGetTypeName(type, out string typeName, out IConverter converter))
                throw new Exception($"Type '{type}' is unknown.");

            converter.Write(value, new WritingConsumer(typeName, type, writer));
        }

        private class ReadingConsumer : IJsonObjectConsumer
        {
            private IJsonObjectConsumer mWrappedConsumer;
            private readonly Type mBaseType;
            private readonly ITypeNameResolver mResolver;
            private readonly Action<object> mWhenDone;

            public ReadingConsumer(Type baseType, ITypeNameResolver resolver, Action<object> whenDone)
            {
                this.mBaseType = baseType;
                this.mResolver = resolver;
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
                    if (!mResolver.TryGetConverterFromTypeName(value, out Type type, out IConverter converter))
                        throw new Exception($"Type name '{value}' is unknown while deserializing type '{mBaseType}'.");

                    mWrappedConsumer = converter.FromObject(mWhenDone);
                }
                else
                    mWrappedConsumer.String(value);
            }

            public void PropertyName(string propertyName)
            {
                if (mWrappedConsumer == null)
                {
                    if (propertyName != TYPE_PROPERTY)
                        throw new Exception($"The property '{TYPE_PROPERTY}' was expected to be the first property in the JSON object when deserializing type '{mBaseType}', but found property '{propertyName}'.");
                }
                else
                    mWrappedConsumer.PropertyName(propertyName);
            }

            public void Done()
            {
                if (mWrappedConsumer == null)
                    throw new Exception($"The property '{TYPE_PROPERTY}' must be present in the JSON object when deserializing type '{mBaseType}'.");

                mWrappedConsumer.Done();
            }
        }

        private class WritingConsumer : IJsonConsumer
        {
            private readonly string mTypeName;
            private readonly Type mType;
            private readonly IJsonConsumer mWrappedConsumer;

            public WritingConsumer(string typeName, Type type, IJsonConsumer wrappedConsumer)
            {
                this.mTypeName = typeName;
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
                obj.PropertyName(TYPE_PROPERTY);
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
