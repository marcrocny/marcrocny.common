using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MarcRocNy.Common.Config.Storage;

/// <summary>
/// This concrete settings class establishes the configuration parameters.
/// </summary>
public record StorageSettings(
    StorageSettings.Backplane Type = StorageSettings.Backplane.InMemory,
    string BasePath = "",
    string? Credentials = null
    )
{
    public enum Backplane
    {
        InMemory,
        FileSystem,
        Cloud,
        Sftp,
    }
}

/// <summary>
/// The generic version pulls in the <see cref="ISettingPointer"/> pattern for nested configuration.
/// </summary>
/// <typeparam name="TParent"></typeparam>
public record StorageSettings<TParent> : StorageSettings, ISettingPointer
    where TParent : ISettingPointer
{
    public static string SectionName { get; } = $"{TParent.SectionName}:storage";
}


