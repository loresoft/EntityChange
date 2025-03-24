using System.Text.Json.Serialization;

namespace EntityChange;

/// <summary>
/// Provides metadata about a set of types that is relevant to JSON serialization.
/// </summary>
[JsonSerializable(typeof(ChangeRecord))]
[JsonSerializable(typeof(List<ChangeRecord>))]
[JsonSerializable(typeof(IReadOnlyCollection<ChangeRecord>))]
[JsonSerializable(typeof(IEnumerable<ChangeRecord>))]
public partial class EntityChangeSerializerContext : JsonSerializerContext
{ }
