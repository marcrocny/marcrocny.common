using Microsoft.Extensions.Options;

namespace MarcRocNy.Common.Config;

/// <summary>
/// Demo of parent class that requires a single 
/// </summary>
public class GenericNestedConfig_SingleUse
{
    //TODO: refine and complete GenericNestedConfigDemos

    public record ParentSettings : ISettingPointer
    {
        public static string SectionName => "parent";
    }

    public class ParentService : ISettingPointer
    {
        public static string SectionName { get; } = ParentSettings.SectionName;
    }

}
