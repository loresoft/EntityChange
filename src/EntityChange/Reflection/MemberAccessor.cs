using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace EntityChange.Reflection;

/// <summary>
/// A <see langword="base"/> class for member accessors.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public abstract class MemberAccessor : IMemberAccessor, IEquatable<IMemberAccessor>
{
    private readonly Lazy<ColumnAttribute> _columnAttribute;
    private readonly Lazy<KeyAttribute> _keyAttribute;
    private readonly Lazy<NotMappedAttribute> _notMappedAttribute;
    private readonly Lazy<DatabaseGeneratedAttribute> _databaseGeneratedAttribute;
    private readonly Lazy<ConcurrencyCheckAttribute> _concurrencyCheckAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberAccessor"/> class.
    /// </summary>
    protected MemberAccessor(MemberInfo memberInfo)
    {
        MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));

        _columnAttribute = new Lazy<ColumnAttribute>(() => MemberInfo.GetCustomAttribute<ColumnAttribute>(true));
        _keyAttribute = new Lazy<KeyAttribute>(() => MemberInfo.GetCustomAttribute<KeyAttribute>(true));
        _notMappedAttribute = new Lazy<NotMappedAttribute>(() => MemberInfo.GetCustomAttribute<NotMappedAttribute>(true));
        _databaseGeneratedAttribute = new Lazy<DatabaseGeneratedAttribute>(() => MemberInfo.GetCustomAttribute<DatabaseGeneratedAttribute>(true));
        _concurrencyCheckAttribute = new Lazy<ConcurrencyCheckAttribute>(() => MemberInfo.GetCustomAttribute<ConcurrencyCheckAttribute>(true));
    }


    /// <summary>
    /// Gets the <see cref="Type"/> of the member.
    /// </summary>
    /// <value>The <see cref="Type"/> of the member.</value>
    public abstract Type MemberType { get; }

    /// <summary>
    /// Gets the <see cref="MemberInfo"/> for the accessor.
    /// </summary>
    /// <value>The member info.</value>
    public MemberInfo MemberInfo { get; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>The name of the member.</value>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this member has getter.
    /// </summary>
    /// <value><c>true</c> if this member has getter; otherwise, <c>false</c>.</value>
    public abstract bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this member has setter.
    /// </summary>
    /// <value><c>true</c> if this member has setter; otherwise, <c>false</c>.</value>
    public abstract bool HasSetter { get; }

    /// <summary>
    /// Gets the database column name that a property is mapped to
    /// </summary>
    /// <value>
    /// The database column name that a property is mapped to
    /// </value>
    public string Column => _columnAttribute.Value?.Name ?? Name;

    /// <summary>
    /// Gets a value indicating that this property is the unique identify for the entity
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is key; otherwise, <c>false</c>.
    /// </value>
    public bool IsKey => _keyAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating that this property is the unique identify for the entity
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property is key; otherwise, <c>false</c>.
    /// </value>
    public bool IsNotMapped => _notMappedAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating that this property participates in optimistic concurrency check
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property participates in optimistic concurrency check; otherwise, <c>false</c>.
    /// </value>
    public bool IsConcurrencyCheck => _concurrencyCheckAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating that this property is database generated
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property is database generated; otherwise, <c>false</c>.
    /// </value>
    public bool IsDatabaseGenerated => _databaseGeneratedAttribute.Value != null
        && _databaseGeneratedAttribute.Value.DatabaseGeneratedOption != DatabaseGeneratedOption.None;


    /// <summary>
    /// Returns the value of the member.
    /// </summary>
    /// <param name="instance">The object whose member value will be returned.</param>
    /// <returns>
    /// The member value for the instance parameter.
    /// </returns>
    public abstract object GetValue(object instance);

    /// <summary>
    /// Sets the value of the member.
    /// </summary>
    /// <param name="instance">The object whose member value will be set.</param>
    /// <param name="value">The new value for this member.</param>
    public abstract void SetValue(object instance, object value);


    /// <summary>
    /// Determines whether the specified <see cref="IMemberAccessor"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="IMemberAccessor"/> to compare with this instance.</param>
    /// <returns>
    /// 	<c>true</c> if the specified <see cref="IMemberAccessor"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(IMemberAccessor other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Equals(other.MemberInfo, MemberInfo);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// 	<c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != typeof(MemberAccessor))
            return false;

        return Equals((MemberAccessor)obj);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        return MemberInfo.GetHashCode();
    }
}
