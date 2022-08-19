using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace CompactJson
{
    internal sealed class ObjectConverter : ConverterBase, IConverterInitialization
    {
        internal struct PropInfo
        {
            public string Name;
            public Action<object, object> Setter;
            public Func<object, object> Getter;
            public IConverter Converter;
            public object DefaultValue;
            public bool SuppressDefaultValue;
            public bool EmitNullValue;
        }

        private readonly Dictionary<string, PropInfo> mProps = new Dictionary<string, PropInfo>(StringComparer.OrdinalIgnoreCase);
        private PropInfo[] mPropList;
        private Func<object> mConstructor;

        private void AddProperty(MemberInfo memberInfo, Func<object, object> getter, Action<object, object> setter, Type propertyType)
        {
            // check, if a custom converter has been assigned
            Type converterType = JsonCustomConverterAttribute.GetConverterType(memberInfo, out object[] converterParameters);
            IConverter converter = ConverterFactoryHelper.CreateConverter(converterType, propertyType, converterParameters);

            // check, if a custom property name should be used
            JsonPropertyAttribute jsonProperty = memberInfo.GetCustomAttribute<JsonPropertyAttribute>(true);
			string propertyName = jsonProperty?.Name ?? memberInfo.Name;

            mProps.Add(propertyName, new PropInfo
            {
                Name = propertyName,
                Setter = setter,
                Getter = getter,
                Converter = converter,
                DefaultValue = GetDefaultValue(propertyType),
                SuppressDefaultValue = SuppressDefaultValue(memberInfo),
                EmitNullValue = EmitNullValue(memberInfo)
            });
        }

        private object GetDefaultValue(Type type)
        {
            object defaultValue = null;
            if (type.IsValueType)
                defaultValue = Activator.CreateInstance(type);
            return defaultValue;
        }

        void IConverterInitialization.InitializeConverter()
        {
            mProps.Clear();

            foreach (PropertyInfo property in Type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (PropertyQualifies(property))
                    AddProperty(property, CreateGet(property), CreateSet(property), property.PropertyType);
            }

            foreach (FieldInfo field in Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (FieldQualifies(field))
                    AddProperty(field, CreateGet(field), CreateSet(field), field.FieldType);
            }

            mPropList = mProps.Values.ToArray();

            mConstructor = GetConstructor(Type);
        }

        private static bool MemberQualifies(MemberInfo memberInfo, bool autoInclude)
        {
            // check, if the member should be ignored
            if (Attribute.IsDefined(memberInfo, typeof(JsonIgnoreMemberAttribute)))
                return false;

            if (memberInfo.IsDefined(typeof(JsonPropertyAttribute), true))
                return true;

            return autoInclude && !Attribute.IsDefined(memberInfo, typeof(IgnoreDataMemberAttribute));
        }

        private static bool FieldQualifies(FieldInfo field)
        {
            bool autoInclude = (field.IsPublic && !field.IsInitOnly);
            return MemberQualifies(field, autoInclude);
        }

        private static bool PropertyQualifies(PropertyInfo property)
        {
            bool autoInclude = (property.GetSetMethod(true) != null) && (property.GetGetMethod()?.IsPublic ?? false);
            return MemberQualifies(property, autoInclude);
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
            MethodInfo getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                return obj => throw new Exception($"Property '{propertyInfo.Name}' of type {propertyInfo.DeclaringType} has no getter, hence it cannot be converted to JSON.");

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
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
            MethodInfo setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
                return (obj, val) => throw new Exception($"Property '{propertyInfo.Name}' of type {propertyInfo.DeclaringType} has no setter, hence it cannot be read from JSON.");

            // use reflection for structs
            // expression doesn't correctly set value
            if (propertyInfo.DeclaringType.IsValueType)
            {
                return propertyInfo.SetValue;
            }

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);

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
            if (fieldInfo.IsInitOnly)
                return (obj, val) => throw new Exception($"Field '{fieldInfo.Name}' of type {fieldInfo.DeclaringType} is readonly, hence it cannot be read from JSON.");

            // use reflection for structs
            // expression doesn't correctly set value
            if (fieldInfo.DeclaringType.IsValueType)
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

        private static bool SuppressDefaultValue(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(JsonSuppressDefaultValueAttribute));
        }

        private static bool EmitNullValue(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(JsonEmitNullValueAttribute));
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

                try
                {
                    return mCurrentPropInfo.Converter.FromArray(value =>
                    {
                        try
                        {
                            mCurrentPropInfo.Setter(mObject, value);
                        }
                        catch (Exception exS)
                        {
                            throw CreateSetterError(exS);
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            public void Boolean(bool value)
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromBoolean(value));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Reading property '{mCurrentPropInfo.Name}' of type '{mObject.GetType().Name}' failed: {ex.Message}", ex);
                }
            }

            public void Done()
            {
                mWhenDone(mObject);
            }

            public void Null()
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNull());
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            public void Number(double value)
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNumber(value));
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            public void Number(long value)
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNumber(value));
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            public void Number(ulong value)
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromNumber(value));
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            public IJsonObjectConsumer Object()
            {
                if (!mValidProperty)
                    return DummyConsumer.INSTANCE;

                try
                {
                    return mCurrentPropInfo.Converter.FromObject(value =>
                    {
                        try
                        {
                            mCurrentPropInfo.Setter(mObject, value);
                        }
                        catch (Exception ex)
                        {
                            throw CreateSetterError(ex);
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
            }

            private Exception CreateSetterError(Exception ex)
            {
                return new Exception($"Reading property '{mCurrentPropInfo.Name}' of type '{mObject.GetType().Name}' failed: {ex.Message}", ex);
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

                public void Number(ulong value)
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

                        if (pi.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            mValidProperty = true;
                            mCurrentPropInfo = pi;
                            return;
                        }
                    }
                    mValidProperty = false;
                    return;
                }
                mValidProperty = mProps.TryGetValue(propertyName, out mCurrentPropInfo);
            }

            public void String(string value)
            {
                if (!mValidProperty)
                    return;

                try
                {
                    mCurrentPropInfo.Setter(mObject, mCurrentPropInfo.Converter.FromString(value));
                }
                catch (Exception ex)
                {
                    throw CreateSetterError(ex);
                }
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
                if (propValue == null)
                {
                    if (!property.EmitNullValue)
                        continue;
                }
                else if (property.SuppressDefaultValue && Equals(propValue, property.DefaultValue))
                    continue;

                objConsumer.PropertyName(property.Name);
                property.Converter.Write(propValue, objConsumer);
            }

            objConsumer.Done();
        }
    }
}
