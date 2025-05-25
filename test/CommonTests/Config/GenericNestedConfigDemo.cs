using Microsoft.Extensions.Options;

namespace MarcRocNy.Common.Config;

/// <summary>
/// Demo of parent class that requires a single conventionally-configured sub-service.
/// </summary>
public class GenericNestedConfig_SingleUse
{
    public record SubServiceSettings<TParent> (
        string Name = "",
        bool Active = true
        ) : ISettings
        where TParent : ISettingPointer
    {
        /// <summary>
        /// This is the "meat" of the convention: The hierarchy is built thusly.
        /// </summary>
        /// <remarks>
        /// This is a demonstration of a VSA-type breakdown of service configuration. The consumer's
        /// subservices would be grouped under the service's own configuration, enforced by the
        /// order-composition of the section name. 
        /// <para/>
        /// However, the opposite may be desired for whatever reason, even just aesthetics. That's okay. The 
        /// desired relationship can be acheived simply by changing the section-path composition. In fact, we're
        /// not just limited to this. Any number of <see cref="ISettingPointer"/> type-parameters or other logical
        /// manipulations could occur in the path composition.
        /// <para/>
        /// But take a breath. This could easily get too-cute-by-half and undercut the entire point of a smoothly
        /// predictable _conventional_ configuration. If the convention is too complicated, or even twee, the plot
        /// is lost. The whole point is for the connection from config to Settings to Service to be relatively
        /// obvious. KIS.
        /// <para/>
        /// (I don't think you're stupid.)
        /// </remarks>
        public static string SectionName => $"{TParent.SectionName}:subservice";
    }

    public interface ISubService<TParent>
        where TParent : ISettingPointer
    {
        string ProcessString(string toProcess);
    }

    public class SubService<TParent>(in IOptions<SubServiceSettings<TParent>> settings)
        : ISubService<TParent>
        where TParent : ISettingPointer
    {
        private readonly SubServiceSettings<TParent> _settings = settings.Value;

        public string ProcessString(string toProcess)
            => _settings.Active
            ? $"{toProcess} plus {_settings.Name}"
            : $"disabled {toProcess}";
    }

    /// <summary>
    /// In this simple case <see cref="ParentService"/> has no settings of its own, it merely
    /// consumes the conventionally configured <see cref="SubService"/>.
    /// </summary>
    public class ParentService(in ISubService<ParentService> subService) : ISettingPointer
    {
        public static string SectionName => "parent";

        private readonly ISubService<ParentService> _subService = subService;

        public string UncreativelyCallSubProcess(string incoming)
            => _subService.ProcessString(incoming);
    }

    // TODO: add some demo-tests of simple nested config
}
