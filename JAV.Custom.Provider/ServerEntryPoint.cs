using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;

namespace JAV.Custom.Provider;

public sealed class ServerEntryPoint : IServerEntryPoint
{
    private readonly IProviderManager _providerManager;
    private readonly ILogger _logger;

    public ServerEntryPoint(IProviderManager providerManager, ILogManager logManager)
    {
        _providerManager = providerManager;
        _logger = logManager.GetLogger(Plugin.ProviderName);
    }

    public void Run()
    {
        var imageProviders = _providerManager.ImageProviders.Select(provider => provider.Name).Distinct().ToArray();
        _logger.Info("JAV_CUSTOM_PROVIDER started. Actor image provider registered: {0}",
            imageProviders.Contains(Plugin.ProviderName, StringComparer.OrdinalIgnoreCase));
        var metadataProviders = _providerManager.GetEnabledMetadataProviders(new Person(), new LibraryOptions())
            .Select(provider => provider.Name).Distinct().ToArray();
        _logger.Info("JAV_CUSTOM_PROVIDER actor metadata registered: {0}. Enabled person providers: {1}",
            metadataProviders.Contains(Plugin.ProviderName, StringComparer.OrdinalIgnoreCase),
            string.Join(", ", metadataProviders));
    }

    public void Dispose()
    {
    }
}
