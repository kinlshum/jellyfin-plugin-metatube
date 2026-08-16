using JAV.Custom.Provider.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System.Net;

namespace JAV.Custom.Provider.Providers;

public sealed class ActorProvider : ProviderBase, IRemoteMetadataProvider<Person, PersonLookupInfo>, IHasOrder
{
    private readonly ILogger _logger;

    public ActorProvider(ILogManager logManager)
        : base(logManager.GetLogger(Plugin.ProviderName))
    {
        _logger = logManager.GetLogger(Plugin.ProviderName);
    }

    public string Name => Plugin.ProviderName;
    public int Order => 2000;

    public async Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var records = RecordStore.Load(_logger);
        _logger.Info("JAV_CUSTOM_PROVIDER search: {0}; loaded records: {1}", info.Name, records.Count);
        info.ProviderIds.TryGetValue(Plugin.ProviderId, out var id);
        var record = RecordStore.FindById(records, id) ?? RecordStore.Search(records, info.Name).FirstOrDefault();
        var transient = record is null;
        record ??= CreateLookupRecord(info, id);
        if (record is null) return new MetadataResult<Person>();

        var merged = Clone(record);
        var remoteActors = await OnlineActorLookup.Lookup(record, _logger, cancellationToken).ConfigureAwait(false);
        if (transient && remoteActors.Count == 0) return new MetadataResult<Person>();
        MergeOnlineData(merged, remoteActors);

        var person = new Person
        {
            Name = merged.Name,
            Overview = FormatOverview(merged),
            ProductionLocations = BuildLocations(merged)
        };
        if (DateTime.TryParse(merged.Birthday, out var birthday))
        {
            person.PremiereDate = birthday;
            person.ProductionYear = birthday.Year;
        }
        if (DateTime.TryParse(merged.DeathDate, out var deathDate)) person.EndDate = deathDate;

        person.Tags = merged.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        person.SetProviderId(Plugin.ProviderId, record.Id);
        foreach (var externalId in merged.ExternalIds.Where(pair => !string.IsNullOrWhiteSpace(pair.Value)))
        {
            var key = string.Equals(externalId.Key, "StashActor", StringComparison.OrdinalIgnoreCase)
                ? "Stash"
                : externalId.Key;
            person.SetProviderId(key, externalId.Value);
        }
        SetCustomFields(person, merged);

        return new MetadataResult<Person> { Item = person, HasMetadata = true };
    }

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo info, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var records = RecordStore.Load(_logger);
        info.ProviderIds.TryGetValue(Plugin.ProviderId, out var id);
        var matches = string.IsNullOrWhiteSpace(id)
            ? RecordStore.Search(records, info.Name)
            : records.Where(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

        if (!matches.Any() && !string.IsNullOrWhiteSpace(info.Name))
        {
            var dynamicResult = new RemoteSearchResult
            {
                Name = $"[{Plugin.ProviderName}] {info.Name}",
                SearchProviderName = Name
            };
            dynamicResult.SetProviderId(Plugin.ProviderId, "lookup:" + info.Name.Trim());
            return Task.FromResult(new[] { dynamicResult }.AsEnumerable());
        }

        var results = matches.Select(record =>
        {
            var result = new RemoteSearchResult
            {
                Name = $"[{Plugin.ProviderName}] {record.Name}",
                SearchProviderName = Name,
                ImageUrl = record.ImageUrls.FirstOrDefault() ?? string.Empty,
                PremiereDate = DateTime.TryParse(record.Birthday, out var birthday) ? birthday : null,
                ProductionYear = DateTime.TryParse(record.Birthday, out birthday) ? birthday.Year : null
            };
            result.SetProviderId(Plugin.ProviderId, record.Id);
            return result;
        }).ToList();
        _logger.Info("JAV_CUSTOM_PROVIDER search results: {0}", results.Count);
        return Task.FromResult(results.AsEnumerable());
    }

    private static string[] BuildLocations(ActorRecord record) =>
        new[] { record.PlaceOfBirth, record.Nationality }.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToArray();

    private static ActorRecord? CreateLookupRecord(PersonLookupInfo info, string? id)
    {
        var name = !string.IsNullOrWhiteSpace(info.Name)
            ? info.Name.Trim()
            : id?.StartsWith("lookup:", StringComparison.OrdinalIgnoreCase) == true ? id[7..].Trim() : string.Empty;
        if (name.Length == 0) return null;

        var record = new ActorRecord
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id : "lookup:" + name,
            Name = name,
            Aliases = new List<string> { name }
        };
        foreach (var providerId in info.ProviderIds.Where(pair => !string.IsNullOrWhiteSpace(pair.Value)))
            record.ExternalIds[providerId.Key] = providerId.Value;
        return record;
    }

    private static string FormatOverview(ActorRecord record)
    {
        var parts = new List<string>();
        if (DateTime.TryParse(record.DebutDate, out var debutDate))
            parts.Add($"Debut: {debutDate:MMMM d, yyyy}{FormatAge(record.DebutAge)}");
        if (DateTime.TryParse(record.Birthday, out var birthday))
            parts.Add($"Born: {birthday:MMMM d, yyyy} ({AgeOn(birthday, DateTime.Today)} years old)");
        if (record.EmbyMovieCount.HasValue || record.JavDbMovieCount.HasValue)
            parts.Add($"{record.EmbyMovieCount?.ToString() ?? "?"} / {record.JavDbMovieCount?.ToString() ?? "?"} movie(s)");
        var lines = new List<string> { string.Join(" | ", parts) };
        AddLine(lines, "Also known as", string.Join(", ", record.Aliases.Distinct(StringComparer.OrdinalIgnoreCase)));
        AddLine(lines, "Measurements", record.Measurements);
        AddLine(lines, "Cup Size", record.CupSize);
        AddLine(lines, "AV Activity", record.AvActivity);
        AddLine(lines, "Sign", record.Sign);
        AddLine(lines, "Blood Type", record.BloodType);
        AddLine(lines, "Height", record.Height);
        AddLine(lines, "Nationality", record.Nationality);
        AddLine(lines, "Place of birth", record.PlaceOfBirth);
        AddLine(lines, "Agency", record.Agency);
        AddLine(lines, "Hobbies and special skills", record.Hobbies);
        AddLine(lines, "AV appearance period", record.AvAppearancePeriod);
        AddLine(lines, "Debut work", record.DebutTitle);
        AddUrlLine(lines, "Blog", FindUrl(record, "Blog", "X"));
        AddUrlLine(lines, "Official website", FindUrl(record, "Official website"));
        if (record.Tags.Count > 0) AddLine(lines, "Tags", string.Join(" ", record.Tags.Distinct(StringComparer.OrdinalIgnoreCase)));

        var links = BuildLinks(record).ToList();
        if (links.Count > 0) lines.Add($"Links: {string.Join(", ", links)}");
        return string.Join("\n<br>\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static string FormatAge(int? age) => age.HasValue ? $" ({age.Value} years old)" : string.Empty;

    private static int AgeOn(DateTime birthday, DateTime date)
    {
        var age = date.Year - birthday.Year;
        if (birthday.Date > date.AddYears(-age).Date) age--;
        return age;
    }

    private static string FindUrl(ActorRecord record, params string[] names)
    {
        foreach (var name in names)
        {
            if (record.Urls.TryGetValue(name, out var url) && !string.IsNullOrWhiteSpace(url)) return url;
        }
        return string.Empty;
    }

    private static void SetCustomFields(Person person, ActorRecord record)
    {
        var fields = new Dictionary<string, string?>
        {
            ["JAVAlsoKnownAs"] = string.Join(", ", record.Aliases.Distinct(StringComparer.OrdinalIgnoreCase)),
            ["JAVDebutDate"] = record.DebutDate,
            ["JAVDebutTitle"] = record.DebutTitle,
            ["JAVMeasurements"] = record.Measurements,
            ["JAVCupSize"] = record.CupSize,
            ["JAVActivity"] = record.AvActivity,
            ["JAVSign"] = record.Sign,
            ["JAVBloodType"] = record.BloodType,
            ["JAVHeight"] = record.Height,
            ["JAVAgency"] = record.Agency,
            ["JAVHobbies"] = record.Hobbies,
            ["JAVAppearancePeriod"] = record.AvAppearancePeriod,
            ["JAVEmbyMovieCount"] = record.EmbyMovieCount?.ToString(),
            ["JAVJavDbMovieCount"] = record.JavDbMovieCount?.ToString()
        };
        foreach (var field in fields.Where(field => !string.IsNullOrWhiteSpace(field.Value)))
            person.SetProviderId(field.Key, field.Value!);
    }

    private static ActorRecord Clone(ActorRecord record) => new()
    {
        Id = record.Id, Name = record.Name, Aliases = record.Aliases.ToList(), Birthday = record.Birthday,
        DeathDate = record.DeathDate, DebutDate = record.DebutDate, DebutAge = record.DebutAge,
        DebutTitle = record.DebutTitle, EmbyMovieCount = record.EmbyMovieCount, JavDbMovieCount = record.JavDbMovieCount,
        PlaceOfBirth = record.PlaceOfBirth, Nationality = record.Nationality, Measurements = record.Measurements,
        CupSize = record.CupSize, AvActivity = record.AvActivity, Sign = record.Sign, BloodType = record.BloodType,
        Height = record.Height, Agency = record.Agency, Hobbies = record.Hobbies,
        AvAppearancePeriod = record.AvAppearancePeriod, Overview = record.Overview, Tags = record.Tags.ToList(),
        ExternalIds = new Dictionary<string, string>(record.ExternalIds, StringComparer.OrdinalIgnoreCase),
        Urls = new Dictionary<string, string>(record.Urls, StringComparer.OrdinalIgnoreCase), ImageUrls = record.ImageUrls.ToList()
    };

    private static void MergeOnlineData(ActorRecord record, IEnumerable<RemoteActor> actors)
    {
        foreach (var actor in actors)
        {
            record.Aliases = record.Aliases.Concat(actor.Aliases).Append(actor.Name).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            record.ImageUrls = record.ImageUrls.Concat(actor.Images).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToList();
            if (string.IsNullOrWhiteSpace(record.Birthday) && actor.Birthday.Year > 1) record.Birthday = actor.Birthday.ToString("yyyy-MM-dd");
            if (string.IsNullOrWhiteSpace(record.DebutDate) && actor.DebutDate.Year > 1) record.DebutDate = actor.DebutDate.ToString("yyyy-MM-dd");
            record.BloodType = First(record.BloodType, actor.BloodType);
            record.CupSize = First(record.CupSize, actor.CupSize);
            record.Measurements = First(record.Measurements, actor.Measurements);
            record.Nationality = First(record.Nationality, actor.Nationality);
            record.Height = First(record.Height, actor.Height > 0 ? $"{actor.Height}cm" : string.Empty);
            record.Hobbies = First(record.Hobbies, string.Join(", ", new[] { actor.Hobby, actor.Skill }.Where(value => !string.IsNullOrWhiteSpace(value))));
            if (!string.IsNullOrWhiteSpace(actor.Homepage)) record.Urls.TryAdd(actor.Provider, actor.Homepage);
            record.ExternalIds.TryAdd(actor.Provider, actor.Id);
            if (!record.ExternalIds.ContainsKey("MetaTube")) record.ExternalIds["MetaTube"] = $"{actor.Provider}:{actor.Id}";
        }
    }

    private static string First(string current, string candidate) => string.IsNullOrWhiteSpace(current) ? candidate : current;

    private static void AddLine(ICollection<string> lines, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) lines.Add($"{WebUtility.HtmlEncode(label)}: {WebUtility.HtmlEncode(value)}");
    }

    private static void AddUrlLine(ICollection<string> lines, string label, string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            lines.Add($"{WebUtility.HtmlEncode(label)}: <a href=\"{WebUtility.HtmlEncode(value)}\">{WebUtility.HtmlEncode(value)}</a>");
    }

    private static IEnumerable<string> BuildLinks(ActorRecord record)
    {
        var links = new Dictionary<string, string>(record.Urls, StringComparer.OrdinalIgnoreCase);
        var formats = new Dictionary<string, (string Label, string Format)>(StringComparer.OrdinalIgnoreCase)
        {
            ["AV-LEAGUE"] = ("AV-LEAGUE", "https://www.av-league.com/actress/{0}.html"),
            ["XsList"] = ("XsList", "https://xslist.org/en/model/{0}.html"),
            ["Minnano-AV"] = ("Minnano-AV", "https://www.minnano-av.com/actress{0}.html"),
            ["SupjavActor"] = ("Superjav", "https://supjav.com/category/cast/{0}"),
            ["JavdbActor"] = ("Javdb", "https://javdb.com/actors/{0}.html"),
            ["InstagramActor"] = ("Instagram", "https://instagram.com/{0}/"),
            ["JavLibraryIdol"] = ("JavLibrary Idol", "https://www.javlibrary.com/en/vl_star.php?s={0}"),
            ["MetaTube"] = ("MetaTube", $"{Plugin.Instance.Configuration.MetaTubeServer.TrimEnd('/')}?redirect={{0}}"),
            ["SextbActress"] = ("Sextb", "https://sextb.net/actress/{0}"),
            ["StashActor"] = ("Stash", "http://localhost:9999/performers/{0}"),
            ["TwitterActor"] = ("X (Twitter)", "https://twitter.com/{0}/"),
            ["PubjavActor"] = ("Pubjav", "https://pubjav.com/tag/{0}/")
        };
        foreach (var externalId in record.ExternalIds)
        {
            if (formats.TryGetValue(externalId.Key, out var definition) && !string.IsNullOrWhiteSpace(externalId.Value))
                links.TryAdd(definition.Label, definition.Format.Replace("{0}", Uri.EscapeDataString(externalId.Value)));
        }

        foreach (var link in links.Where(link => Uri.TryCreate(link.Value, UriKind.Absolute, out var uri)
                                                  && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            yield return $"<a href=\"{WebUtility.HtmlEncode(link.Value)}\">{WebUtility.HtmlEncode(link.Key)}</a>";
    }
}
