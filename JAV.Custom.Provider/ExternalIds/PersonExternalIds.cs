using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace JAV.Custom.Provider.ExternalIds;

public abstract class PersonExternalId : IExternalId
{
    protected PersonExternalId(string providerName, string key, string urlFormatString = "")
    {
        ProviderName = providerName;
        Key = key;
        UrlFormatString = urlFormatString;
    }

    public string Name => ProviderName;
    public string ProviderName { get; }
    public string Key { get; }
    public string UrlFormatString { get; }
    public bool Supports(IHasProviderIds item) => item is Person;
}

public sealed class CustomProviderId() : PersonExternalId(Plugin.ProviderName, Plugin.ProviderId);
public sealed class CustomId1() : PersonExternalId(
    Plugin.Instance.Configuration.CustomId1Name,
    "JAVCustomId1",
    Plugin.Instance.Configuration.CustomId1Url);
public sealed class CustomId2() : PersonExternalId(
    Plugin.Instance.Configuration.CustomId2Name,
    "JAVCustomId2",
    Plugin.Instance.Configuration.CustomId2Url);

public sealed class AvLeagueId() : PersonExternalId("AV-LEAGUE", "AV-LEAGUE", "https://www.av-league.com/actress/{0}");
public sealed class XsListId() : PersonExternalId("XsList", "XsList", "https://xslist.org/en/model/{0}.html");
public sealed class MinnanoAvId() : PersonExternalId("Minnano-AV", "Minnano-AV", "https://www.minnano-av.com/actress{0}.html");
public sealed class GfriendsId() : PersonExternalId("Gfriends", "Gfriends");
public sealed class JavLibraryIdolId() : PersonExternalId("JavLibrary Idol", "JavLibraryIdol", "https://www.javlibrary.com/en/vl_star.php?s={0}");
public sealed class JapanHdvIdolId() : PersonExternalId("JapanHDV Idol", "JapanHDVIdol");
public sealed class SextbActressId() : PersonExternalId("Sextb JAV Actress ID", "SextbActress", "https://sextb.net/actress/{0}");
public sealed class JavDbId() : PersonExternalId("Javdb", "JavdbActor", "https://javdb.com/actors/{0}.html");
public sealed class SupjavId() : PersonExternalId("Supjav", "SupjavActor", "https://supjav.com/category/cast/{0}");
public sealed class PubjavId() : PersonExternalId("Pubjav", "PubjavActor", "https://pubjav.com/tag/{0}/");
public sealed class SexctId() : PersonExternalId("Sexct", "SexctActor");
public sealed class XId() : PersonExternalId("X", "XActor", "https://x.com/{0}");
public sealed class OfficialWebsiteId() : PersonExternalId("Official website", "OfficialWebsite");
public sealed class InstagramId() : PersonExternalId("Instagram", "InstagramActor", "https://instagram.com/{0}/");
public sealed class TwitterId() : PersonExternalId("Twitter", "TwitterActor", "https://twitter.com/{0}/");
public sealed class StashId() : PersonExternalId("Stash", "StashActor", "http://localhost:9999/performers/{0}");

// Preserve Provider Ids Extender keys already stored in the Emby database.
public sealed class LegacySupjavId() : PersonExternalId("Superjav", "superjav", "https://supjav.com/category/cast/{0}");
public sealed class LegacyJavDbId() : PersonExternalId("Javdb", "javdb", "https://javdb.com/actors/{0}.html");
public sealed class LegacyInstagramId() : PersonExternalId("Instagram", "i.", "https://instagram.com/{0}");
public sealed class LegacyTwitterId() : PersonExternalId("Twitter", "t.", "https://twitter.com/{0}");
public sealed class LegacyJavLibraryIdolId() : PersonExternalId("JavLibrary Idol", "jli.", "https://www.javlibrary.com/en/vl_star.php?s={0}");
public sealed class LegacySextbId() : PersonExternalId("Sextb JAV Actress ID", "stb.", "https://sextb.net/actress/{0}");
public sealed class LegacyXsListId() : PersonExternalId("XsList", "xslist1", "https://xslist.org/en/model/{0}.html");
public sealed class LegacyStashId() : PersonExternalId("Stash", "stash", "http://localhost:9999/performers/{0}");
public sealed class MovieBotId() : PersonExternalId("MovieBot Person", "MovieBotPerson");
public sealed class Data18Id() : PersonExternalId("Data 18 Person", "Data18Person");
public sealed class ThePornMapId() : PersonExternalId("ThePornMap Person", "ThePornMapPerson");
public sealed class HotMoviesId() : PersonExternalId("Hot Movies Person", "HotMoviesPerson");
public sealed class ClassicPornId() : PersonExternalId("The Classic Porn Person", "ClassicPornPerson");
public sealed class XappieId() : PersonExternalId("Xappie Person", "XappiePerson");
public sealed class UralroverId() : PersonExternalId("Uralrover Person", "UralroverPerson");
public sealed class BluRayComId() : PersonExternalId("BluRayCom Person", "BluRayComPerson");
public sealed class KinoriumId() : PersonExternalId("kinorium Person", "KinoriumPerson");
public sealed class PoppornId() : PersonExternalId("Popporn Person", "PoppornPerson");
public sealed class IafdId() : PersonExternalId("IAFD Performer ID", "IAFDPerformer");
public sealed class PornMakiId() : PersonExternalId("Porn Maki Person ID", "PornMakiPerson");
public sealed class MyPersonApiId() : PersonExternalId("MyPersonApi", "MyPersonApi");
public sealed class MediaMetaAboutId() : PersonExternalId("Media and Meta About Page", "MediaMetaAbout");
public sealed class AlsoKnownAsId() : PersonExternalId("Also known as", "JAVAlsoKnownAs");
public sealed class DebutDateId() : PersonExternalId("Debut date", "JAVDebutDate");
public sealed class DebutTitleId() : PersonExternalId("Debut title", "JAVDebutTitle");
public sealed class MeasurementsId() : PersonExternalId("Measurements", "JAVMeasurements");
public sealed class CupSizeId() : PersonExternalId("Cup Size", "JAVCupSize");
public sealed class AvActivityId() : PersonExternalId("AV Activity", "JAVActivity");
public sealed class SignId() : PersonExternalId("Sign", "JAVSign");
public sealed class BloodTypeId() : PersonExternalId("Blood Type", "JAVBloodType");
public sealed class HeightId() : PersonExternalId("Height", "JAVHeight");
public sealed class AgencyId() : PersonExternalId("Agency", "JAVAgency");
public sealed class HobbiesId() : PersonExternalId("Hobbies and special skills", "JAVHobbies");
public sealed class AppearancePeriodId() : PersonExternalId("AV appearance period", "JAVAppearancePeriod");
public sealed class EmbyMovieCountId() : PersonExternalId("Emby movie count", "JAVEmbyMovieCount");
public sealed class JavDbMovieCountId() : PersonExternalId("JavDB movie count", "JAVJavDbMovieCount");
