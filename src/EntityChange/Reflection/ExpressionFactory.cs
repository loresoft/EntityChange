#nullable disable

using System.Linq.Expressions;
using System.Reflection;

namespace EntityChange.Reflection;

internal static class ExpressionFactory
{
    public static Func<object, object[], object> CreateMethod(MethodInfo methodInfo)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        // parameters to execute
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

        // build parameter list
        var parameterExpressions = new List<Expression>();
        var paramInfos = methodInfo.GetParameters();
        for (int i = 0; i < paramInfos.Length; i++)
        {
            // (Ti)parameters[i]
            var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));

            Type parameterType = paramInfos[i].ParameterType;
            if (parameterType.IsByRef)
                parameterType = parameterType.GetElementType();

            var valueCast = Expression.Convert(valueObj, parameterType);

            parameterExpressions.Add(valueCast);
        }

        // non-instance for static method, or ((TInstance)instance)
        var instanceCast = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.DeclaringType);

        // static invoke or ((TInstance)instance).Method
        var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

        // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
        if (methodCall.Type == typeof(void))
        {
            var lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);
            var execute = lambda.Compile();

            return (instance, parameters) =>
            {
                execute(instance, parameters);
                return null;
            };
        }
        else
        {
            var castMethodCall = Expression.Convert(methodCall, typeof(object));
            var lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);

            return lambda.Compile();
        }
    }

    public static Func<object> CreateConstructor(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var typeInfo = type.GetTypeInfo();


        var constructorInfo = typeInfo.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
            throw new ArgumentException("Could not find constructor for type.", nameof(type));

        var instanceCreate = Expression.New(constructorInfo);

        var instanceCreateCast = typeInfo.IsValueType
            ? Expression.Convert(instanceCreate, typeof(object))
            : Expression.TypeAs(instanceCreate, typeof(object));

        var lambda = Expression.Lambda<Func<object>>(instanceCreateCast);

        return lambda.Compile();
    }

    public static Func<object, object> CreateGet(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        if (!propertyInfo.CanRead)
            return null;

        var instance = Expression.Parameter(typeof(object), "instance");
        var declaringType = propertyInfo.DeclaringType;
        var getMethod = propertyInfo.GetGetMethod(true);

        var instanceCast = CreateCast(instance, declaringType, getMethod.IsStatic);

        var call = Expression.Call(instanceCast, getMethod);
        var valueCast = Expression.TypeAs(call, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(valueCast, instance);
        return lambda.Compile();
    }

    public static Func<object, object> CreateGet(FieldInfo fieldInfo)
    {
        if (fieldInfo == null)
            throw new ArgumentNullException(nameof(fieldInfo));

        var instance = Expression.Parameter(typeof(object), "instance");
        var declaringType = fieldInfo.DeclaringType;

        var instanceCast = CreateCast(instance, declaringType, fieldInfo.IsStatic);

        var fieldAccess = Expression.Field(instanceCast, fieldInfo);
        var valueCast = Expression.TypeAs(fieldAccess, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(valueCast, instance);
        return lambda.Compile();
    }

    public static Action<object, object> CreateSet(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        if (!propertyInfo.CanWrite)
            return null;

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var declaringType = propertyInfo.DeclaringType;
        var propertyType = propertyInfo.PropertyType;
        var setMethod = propertyInfo.GetSetMethod(true);

        var instanceCast = CreateCast(instance, declaringType, setMethod.IsStatic);
        var valueCast = CreateCast(value, propertyType, false);

        var call = Expression.Call(instanceCast, setMethod, valueCast);
        var parameters = new[] { instance, value };

        var lambda = Expression.Lambda<Action<object, object>>(call, parameters);
        return lambda.Compile();
    }

    public static Action<object, object> CreateSet(FieldInfo fieldInfo)
    {
        if (fieldInfo == null)
            throw new ArgumentNullException(nameof(fieldInfo));

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var declaringType = fieldInfo.DeclaringType;
        var fieldType = fieldInfo.FieldType;

        var instanceCast = CreateCast(instance, declaringType, fieldInfo.IsStatic);
        var valueCast = CreateCast(value, fieldType, false);

        var member = Expression.Field(instanceCast, fieldInfo);
        var assign = Expression.Assign(member, valueCast);

        var parameters = new[] { instance, value };

        var lambda = Expression.Lambda<Action<object, object>>(assign, parameters);
        return lambda.Compile();
    }


    private static UnaryExpression CreateCast(ParameterExpression instance, Type declaringType, bool isStatic)
    {
        if (isStatic)
            return null;

        // value as T is slightly faster than (T)value, so if it's not a value type, use that
        if (declaringType.GetTypeInfo().IsValueType)
            return Expression.Convert(instance, declaringType);
        else
            return Expression.TypeAs(instance, declaringType);
    }
}
