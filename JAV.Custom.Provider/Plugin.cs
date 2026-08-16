using JAV.Custom.Provider.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;

namespace JAV.Custom.Provider;

public sealed class Plugin : BasePluginSimpleUI<PluginConfiguration>
{
    public const string ProviderName = "JAV_CUSTOM_PROVIDER";
    public const string ProviderId = "JAV_CUSTOM_PROVIDER";

    public Plugin(IApplicationHost applicationHost) : base(applicationHost)
    {
        Instance = this;
    }

    public static Plugin Instance { get; private set; } = null!;
    public override string Name => ProviderName;
    public override string Description => "Private actor metadata, aliases, URLs, images, and external IDs.";
    public override Guid Id => Guid.Parse("799a70be-0e93-45fc-8ec4-a4aac82e6471");
    public PluginConfiguration Configuration => GetOptions();
}
