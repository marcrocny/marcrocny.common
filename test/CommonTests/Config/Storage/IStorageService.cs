using System.IO;
using System.Threading.Tasks;

namespace MarcRocNy.Common.Config.Storage;

/// <summary>
/// The primary service description, that allows a given instance to be passed even if it is
/// parent-specific.
/// </summary>
public interface IStorageService
{
    Task Save(Stream stream, string fileName);

    Task<Stream?> Open(string fileName);

    // etc.
}

public interface IStorageService<TParent> : IStorageService
    where TParent : ISettingPointer // needed?
{ }


