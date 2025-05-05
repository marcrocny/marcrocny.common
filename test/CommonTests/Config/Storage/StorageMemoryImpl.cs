using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MarcRocNy.Common.Config.Storage;

public class StorageMemoryImpl(in StorageSettings settings) : IStorageService
{
    private readonly static Dictionary<string, MemoryStream> files = [];

    private readonly StorageSettings _settings = settings;

    public string FullPath(string fileName) => Path.Combine(_settings.BasePath, fileName);

    public Task<Stream?> Open(string fileName)
    {
        if (!files.TryGetValue(FullPath(fileName), out var stream)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(stream);
    }

    public async Task Save(Stream stream, string fileName)
    {
        MemoryStream mem = new();
        await stream.CopyToAsync(mem);
        files[FullPath(fileName)] = mem;
    }
}

public class StorageMemoryImpl<TParent>(in IOptions<StorageSettings<TParent>> settings)
    : StorageMemoryImpl(settings.Value)
    where TParent : ISettingPointer
{
}
