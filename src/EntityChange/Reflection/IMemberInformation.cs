#nullable disable

using System.Reflection;

namespace EntityChange.Reflection;

/// <summary>
/// An interface for member information
/// </summary>
public interface IMemberInformation
{
    /// <summary>
    /// Gets the type of the member.
    /// </summary>
    /// <value>The type of the member.</value>
    Type MemberType { get; }

    /// <summary>
    /// Gets the member info.
    /// </summary>
    /// <value>The member info.</value>
    MemberInfo MemberInfo { get; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>The name of the member.</value>
    string Name { get; }

    /// <summary>
    /// Gets the database column name that a property is mapped to
    /// </summary>
    /// <value>
    /// The database column name that a property is mapped to
    /// </value>
    string Column { get; }

    /// <summary>
    /// Gets a value indicating that this property is the unique identify for the entity
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property is a primary key; otherwise, <c>false</c>.
    /// </value>
    bool IsKey { get; }

    /// <summary>
    /// Gets a value indicating that this property should be excluded from database mapping
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property should be excluded from database mapping; otherwise, <c>false</c>.
    /// </value>
    bool IsNotMapped { get; }

    /// <summary>
    /// Gets a value indicating that this property is database generated
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property is database generated; otherwise, <c>false</c>.
    /// </value>
    bool IsDatabaseGenerated { get; }

    /// <summary>
    /// Gets a value indicating that this property participates in optimistic concurrency check
    /// </summary>
    /// <value>
    ///   <c>true</c> if this property participates in optimistic concurrency check; otherwise, <c>false</c>.
    /// </value>
    bool IsConcurrencyCheck { get; }

    /// <summary>
    /// Gets a value indicating the name of the associated navigation property or associated foreign key(s)
    /// </summary>
    /// <value>
    ///   A value indicating the name of the associated navigation property or associated foreign key(s)
    /// </value>
    string ForeignKey { get; }

    /// <summary>
    /// Gets a value indicating whether this member has getter.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this member has getter; otherwise, <c>false</c>.
    /// </value>
    bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this member has setter.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this member has setter; otherwise, <c>false</c>.
    /// </value>
    bool HasSetter { get; }
}
