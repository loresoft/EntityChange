﻿using System.Reflection;

namespace EntityChange.Reflection;

/// <summary>
/// An accessor class for <see cref="PropertyInfo"/>.
/// </summary>
public class PropertyAccessor : MemberAccessor
{
    private readonly PropertyInfo _propertyInfo;
    private readonly Lazy<Func<object, object>> _getter;
    private readonly Lazy<Action<object, object>> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> instance to use for this accessor.</param>
    public PropertyAccessor(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        _propertyInfo = propertyInfo;
        Name = _propertyInfo.Name;
        MemberType = _propertyInfo.PropertyType;

        HasGetter = _propertyInfo.CanRead;
        _getter = new Lazy<Func<object, object>>(() => DelegateFactory.CreateGet(_propertyInfo));

        HasSetter = _propertyInfo.CanWrite;
        _setter = new Lazy<Action<object, object>>(() => DelegateFactory.CreateSet(_propertyInfo));
    }


    /// <summary>
    /// Gets the type of the member.
    /// </summary>
    /// <value>The type of the member.</value>
    public override Type MemberType { get; }

    /// <summary>
    /// Gets the member info.
    /// </summary>
    /// <value>The member info.</value>
    public override MemberInfo MemberInfo => _propertyInfo;

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>The name of the member.</value>
    public override string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this member has getter.
    /// </summary>
    /// <value><c>true</c> if this member has getter; otherwise, <c>false</c>.</value>
    public override bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this member has setter.
    /// </summary>
    /// <value><c>true</c> if this member has setter; otherwise, <c>false</c>.</value>
    public override bool HasSetter { get; }


    /// <summary>
    /// Returns the value of the member.
    /// </summary>
    /// <param name="instance">The object whose member value will be returned.</param>
    /// <returns>
    /// The member value for the instance parameter.
    /// </returns>
    public override object GetValue(object instance)
    {
        if (_getter == null || !HasGetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        var get = _getter.Value;
        if (get == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        return get(instance);
    }

    /// <summary>
    /// Sets the value of the member.
    /// </summary>
    /// <param name="instance">The object whose member value will be set.</param>
    /// <param name="value">The new value for this member.</param>
    public override void SetValue(object instance, object value)
    {
        if (_setter == null || !HasSetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        var set = _setter.Value;
        if (set == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        set(instance, value);
    }
}
