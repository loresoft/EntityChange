namespace EntityChange;

/// <summary>
/// Marks a partial class deriving from <see cref="EntityComparer{T}"/> for source generation.
/// The source generator will implement the comparison logic for the target entity type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateComparerAttribute : Attribute;
