namespace MarcRocNy.Common.Config;

/// <summary>
/// Establishes a pattern for settings (options) objects. 
/// </summary>
/// <remarks>
/// This can be made compile-conditional for net8.0 only, to be used alongside a conventional 
/// `public const string SectionName = ..` pattern; see notes on <see cref="ConfigurationExt"/>.
/// </remarks>
public interface ISettingPointer
{
    /// <summary>
    /// The section name. Hierarchy-delimited by colon (`:`).
    /// </summary>
    static abstract string SectionName { get; }
}