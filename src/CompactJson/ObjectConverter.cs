using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CompactJson
{
    internal sealed class ObjectConverter : ConverterBase
    {
        internal struct PropInfo
        {
            public string Name;
            public Action<object, object> Setter;
            public Func<object, object> Getter;
            public IConverter Converter;
            public object DefaultValue;
            public bool EmitDefaultValue;
        }

        private readonly Dictionary<string, PropInfo> mProps = new Dictionary<string, PropInfo>();
        private PropInfo[] mPropList;
        private Func<object> mConstructor;

        private static readonly Dictionary<Type, ObjectConverter> CURRENTLY_REFLECTED_TYPES = new Dictionary<Type, ObjectConverter>();

        private void AddProperty(MemberInfo memberInfo, Func<object, object> getter, Action<object, object> setter, Type propertyType)
        {
            Type converterType = CustomConverterAttribute.GetConverterType(memberInfo);
            IConverterFactory factory = ConverterFactoryHelper.FromType(converterType);

            ConverterParameters parameters = ConverterParameters.Reflect(memberInfo);
            IConverter converter = ConverterFactoryHelper.CreateConverter(factory, propertyType, parameters);

            mProps.Add(memberInfo.Name, new PropInfo
            {
                Name = memberInfo.Name,
                Setter = setter,
                Getter = getter,
                Converter = converter,
                DefaultValue = GetDefaultValue(propertyType),
                EmitDefaultValue = EmitDefaultValue(memberInfo)
            });
        }

        private object GetDefaultValue(Type type)
        {
            object defaultValue = null;
            if (type.IsValueType)
                defaultValue = Activator.CreateInstance(type);
            return defaultValue;
        }

        internal void Reflect()
        {
            mProps.Clear();

            foreach (var property in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty))
                AddProperty(property, CreateGet(property), CreateSet(property), property.PropertyType);

            foreach (FieldInfo field in Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                AddProperty(field, CreateGet(field), CreateSet(field), field.FieldType);

            mPropList = mProps.Values.ToArray();
            Array.Sort(mPropList, (p1, p2) => p1.Name.CompareTo(p2.Name));

            mConstructor = GetConstructor(Type);
        }

        private static Func<object> GetConstructor(Type objtype)
        {
            try
            {
                Expression expression = Expression.New(objtype);
                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<object>), expression);

                return (Func<object>)lambdaExpression.Compile();
            }
            catch
            {
                return () => Activator.CreateInstance(objtype);
            }
        }

        private static Expression EnsureCastExpression(Expression expression, Type targetType)
        {
            Type expressionType = expression.Type;
            if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
                return expression;
            if (targetType.IsValueType)
                return Expression.Unbox(expression, targetType);
            return Expression.Convert(expression, targetType);
        }

        //private static Func<object, bool> CreateTypeEquals(Type type)
        //{
        //    ParameterExpression valueParameter =  Expression.Parameter(typeof(object), "value");
        //    MethodInfo mi = typeof(object).GetMethod("GetType");
        //    Expression.Call(valueParameter)
        //    Expression expr = Expression.TypeIs(valueParameter, type);
        //    return (Func<object, bool>)Expression.Lambda(typeof(Func<object, bool>), expr, valueParameter).Compile();
        //}

        private static Func<object, object> CreateGet(FieldInfo fieldInfo)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");

            Expression getExpression;
            if (fieldInfo.IsStatic)
            {
                getExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, fieldInfo.DeclaringType);
                getExpression = Expression.Field(readInstanceParameter, fieldInfo);
            }

            // cast to object
            getExpression = EnsureCastExpression(getExpression, typeof(object));

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<object, object>), getExpression, instanceParameter);
            Func<object, object> compiled = (Func<object, object>)lambdaExpression.Compile();
            return compiled;
        }

        private static Func<object, object> CreateGet(PropertyInfo propertyInfo)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");

            MethodInfo getMethod = propertyInfo.GetGetMethod(false);

            Expression getExpression;
            if (getMethod.IsStatic)
            {
                getExpression = Expression.Call(getMethod);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType);
                getExpression = Expression.Call(readInstanceParameter, getMethod);
            }

            // cast to object
            getExpression = EnsureCastExpression(getExpression, typeof(object));

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<object, object>), getExpression, instanceParameter);
            Func<object, object> compiled = (Func<object, object>)lambdaExpression.Compile();
            return compiled;
        }

        private static Action<object, object> CreateSet(PropertyInfo propertyInfo)
        {
            // use reflection for structs
            // expression doesn't correctly set value
            if (propertyInfo.DeclaringType.IsValueType)
            {
                return propertyInfo.SetValue;
            }

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);
            MethodInfo setMethod = propertyInfo.GetSetMethod(false);

            Expression setExpression;
            if (setMethod.IsStatic)
            {
                setExpression = Expression.Call(setMethod, readValueParameter);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType);
                setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
            }

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<object, object>), setExpression, instanceParameter, valueParameter);
            Action<object, object> compiled = (Action<object, object>)lambdaExpression.Compile();
            return compiled;
        }

        private static Action<object, object> CreateSet(FieldInfo fieldInfo)
        {
            // use reflection for structs
            // expression doesn't correctly set value
            if (fieldInfo.DeclaringType.IsValueType || fieldInfo.IsInitOnly)
            {
                return fieldInfo.SetValue;
            }

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            Expression fieldExpression;
            if (fieldInfo.IsStatic)
            {
                fieldExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression sourceExpression = EnsureCastExpression(instanceParameter, fieldInfo.DeclaringType);
                fieldExpression = Expression.Field(sourceExpression, fieldInfo);
            }

            Expression valueExpression = EnsureCastExpression(valueParameter, fieldExpression.Type);

            BinaryExpression assignExpression = Expression.Assign(fieldExpression, valueExpression);
            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<object, object>), assignExpression, instanceParameter, valueParameter);

            return (Action<object, object>)lambdaExpression.Compile();
        }

        internal ObjectConverter(Type type) : base(type)
        {
        }

        private static bool EmitDefaultValue(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute<EmitDefaultValueAttribute>() != null;
        }

        public override object FromNull()
        {
            return null;
        }

        public override IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            return new Consumer(mConstructor(), mProps, mPropList, whenDone);
        }

        private class Consumer : IJsonObjectConsumer
        {
            public Consumer(object obj, Dictionary<string, PropInfo> props, PropInfo[] propList, Action<object> whenDone)
            {
                mObject = obj;
                mProps = props;
                mPropList = propList;
                mWhenDone = whenDone;
            }

            private readonly object mObject;
            private readonly Dictionary<string, PropInfo> mProps;
            private readonly PropInfo[] mPropList;
            private readonly Action<object> mWhenDone;
            private bool mValidProperty;
            private PropInfo mCurrentPropInfo;
            private int mLastPropertyIndex;

            public IJsonArrayConsumer Array()
            {
                if (!mValidProperty)
                    return DummyConsumer.INSTANCE;

                return mCurrentPropInfo.Converter.FromArray(value => mCurrentPropInfo.Setter(mObject, value));
            }

            public void Boolean(bool value)
            {
                if (!mValidProperty)
                    return;

                mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromBoolean(value));
            }

            public void Done()
            {
                mWhenDone(mObject);
            }

            public void Null()
            {
                if (!mValidProperty)
                    return;

                mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNull());
            }

            public void Number(double value)
            {
                if (!mValidProperty)
                    return;

                mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNumber(value));
            }

            public void Number(long value)
            {
                if (!mValidProperty)
                    return;

                mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNumber(value));
            }

            public IJsonObjectConsumer Object()
            {
                if (!mValidProperty)
                    return DummyConsumer.INSTANCE;

                return mCurrentPropInfo.Converter.FromObject(value => mCurrentPropInfo.Setter(mObject, value));
            }

            private sealed class DummyConsumer : IJsonObjectConsumer, IJsonArrayConsumer
            {
                public static readonly DummyConsumer INSTANCE = new DummyConsumer();

                public IJsonArrayConsumer Array()
                {
                    return this;
                }

                public void Boolean(bool value)
                {
                }

                public void Done()
                {
                }

                public void Null()
                {
                }

                public void Number(double value)
                {
                }

                public void Number(long value)
                {
                }

                public IJsonObjectConsumer Object()
                {
                    return this;
                }

                public void PropertyName(string propertyName)
                {
                }

                public void String(string value)
                {
                }
            }

            public void PropertyName(string propertyName)
            {
                if (mPropList.Length < 10)
                {
                    for (int i = 0; i < mPropList.Length; i++)
                    {
                        PropInfo pi = mPropList[mLastPropertyIndex];
                        mLastPropertyIndex++;
                        if (mLastPropertyIndex == mPropList.Length)
                            mLastPropertyIndex = 0;

                        if (pi.Name == propertyName)
                        {
                            mValidProperty = true;
                            mCurrentPropInfo = pi;
                            return;
                        }
                    }
                    mValidProperty = false;
                }
                mValidProperty = mProps.TryGetValue(propertyName, out mCurrentPropInfo);
            }

            public void String(string value)
            {
                if (!mValidProperty)
                    return;

                mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromString(value));
            }
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
            {
                writer.Null();
                return;
            }

            var objConsumer = writer.Object();
            foreach (var property in mPropList)
            {
                object propValue = property.Getter(value);
                if (!property.EmitDefaultValue && Equals(propValue, property.DefaultValue))
                    continue;

                objConsumer.PropertyName(property.Name);
                property.Converter.Write(propValue, objConsumer);
            }

            objConsumer.Done();
        }
    }
}
