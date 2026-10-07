namespace Prepstack.Application.Common;

/// <summary>
/// Generates document ids. Implemented in Infrastructure so Application never takes a
/// dependency on the storage provider's id format (architecture.md: documents use ObjectId).
/// </summary>
public interface IIdGenerator
{
    string NewId();
}
