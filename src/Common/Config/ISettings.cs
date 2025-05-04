namespace MarcRocNy.Common.Config;

/// <summary>
/// Establishes a pattern for settings (options) objects. 
/// </summary>
/// <remarks>
/// This can be made compile-conditional for net8.0 only, or even omitted and the pattern just
/// conventional. It's actually kind of nice that way, still quite clean. The reflection only
/// happens at startup, and 
/// </remarks>
public interface ISettings
{
    /// <summary>
    /// The section name. Hierarchy-delimited by colon (`:`).
    /// </summary>
    static abstract string SectionName { get; }
}

// TODO: refine or remove
//public interface ISettingsPointer
//{
//    //static abstract string SectionName { get; }
//}

//public interface ISettingsPointer<TSettings> : ISettingsPointer where TSettings : ISettings
//{
//    static string SectionName { get; } = TSettings.SectionName;
//}
