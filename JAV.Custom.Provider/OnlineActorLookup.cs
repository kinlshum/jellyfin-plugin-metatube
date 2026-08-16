using System.Net.Http.Headers;
using System.Text.Json;
using JAV.Custom.Provider.Models;
using MediaBrowser.Model.Logging;

namespace JAV.Custom.Provider;

internal static class OnlineActorLookup
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(90) };
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<IReadOnlyList<RemoteActor>> Lookup(string query, ILogger logger, CancellationToken cancellationToken)
    {
        var configuration = Plugin.Instance.Configuration;
        if (!configuration.EnableOnlineLookup || string.IsNullOrWhiteSpace(query)) return Array.Empty<RemoteActor>();

        try
        {
            var searchUrl = $"{configuration.MetaTubeServer.TrimEnd('/')}/v1/actors/search?q={Uri.EscapeDataString(query)}&fallback=true";
            var search = await Get<ApiEnvelope<List<RemoteActorSearch>>>(searchUrl, configuration.MetaTubeToken, cancellationToken).ConfigureAwait(false);
            var sourceOrder = configuration.ActorSourceOrder.Split(',').Select(value => value.Trim()).Where(value => value.Length > 0).ToList();
            var selected = sourceOrder.Select(provider => search.Data?
                    .FirstOrDefault(actor => string.Equals(actor.Provider, provider, StringComparison.OrdinalIgnoreCase)
                                             && Matches(actor, query)))
                .Where(actor => actor is not null)
                .Cast<RemoteActorSearch>()
                .ToList();

            var tasks = selected.Select(actor => Get<ApiEnvelope<RemoteActor>>(
                $"{configuration.MetaTubeServer.TrimEnd('/')}/v1/actors/{Uri.EscapeDataString(actor.Provider)}/{Uri.EscapeDataString(actor.Id)}?lazy=false",
                configuration.MetaTubeToken,
                cancellationToken));
            var details = await Task.WhenAll(tasks).ConfigureAwait(false);
            return details.Where(envelope => envelope.Data is not null).Select(envelope => envelope.Data!).ToList();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.ErrorException("Online actor lookup failed for " + query, exception);
            return Array.Empty<RemoteActor>();
        }
    }

    private static bool Matches(RemoteActorSearch actor, string query) =>
        string.Equals(actor.Name, query, StringComparison.OrdinalIgnoreCase)
        || actor.Aliases.Any(alias => string.Equals(alias, query, StringComparison.OrdinalIgnoreCase));

    private static async Task<T> Get<T>(string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("JAV_CUSTOM_PROVIDER/1.0");
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false))!;
    }
}
