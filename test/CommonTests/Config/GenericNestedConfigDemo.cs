using Microsoft.Extensions.Options;

namespace MarcRocNy.Common.Config;

public class GenericNestedConfigDemos
{
    //TODO: refine and complete GenericNestedConfigDemos

    public record StorageSettings<TParent>(
        string Type = ""
        ) : ISettings
        where TParent : ISettings
    {
        public static string SectionName { get; } = $"{TParent.SectionName}:storage";
    }

    public class StorageService<TParent>(in IOptions<StorageSettings<TParent>> settings)
        where TParent : ISettings
    {
        private readonly StorageSettings<TParent> _settings = settings.Value;
    }

    public record ParentSettings : ISettings
    {
        public static string SectionName => "parent";
    }

    public class ParentService : ISettings
    {
        public static string SectionName { get; } = ParentSettings.SectionName;
    }

}
