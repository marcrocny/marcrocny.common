namespace MarcRocNy.Common.Config;

/// <summary>
/// Establishes a pattern for settings (options) objects. 
/// </summary>
/// <remarks>
/// The more-generic version of <see cref="ISettings"/>. This can be placed on any service that consumes another
/// conventionally-configured, injectable service, even if that service has no other configuration of its own.
/// </remarks>
public interface ISettingPointer
{
    /// <summary>
    /// The section name. Hierarchy-delimited by colon (`:`).
    /// </summary>
    static abstract string SectionName { get; }
}

/// <summary>
/// Establishes a pattern for settings (options) objects; a <see cref="ISettingPointer"/> that points to itself.
/// </summary>
/// <remarks>
/// Pointing to oneself brings the added benefit of conventional configuration through some simple startup-time
/// extensions in <see cref="ConfigurationExt"/>.
/// This can be made compile-conditional for net7.0+ only. 
/// For pre-7.0, it can be used alongside a conventional 
/// `public const string SectionName = ..` pattern; see notes on <see cref="ConfigurationExt"/>.
/// </remarks>
public interface ISettings : ISettingPointer { }