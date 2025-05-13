using ArchiSteamFarm.Core;
using ArchiSteamFarm.NLog;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web;
using ArchiSteamFarm.Web.Responses;
using ASFEnhance.Data.Plugin;
using ProtoBuf;
using SteamKit2;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;

namespace ASFEnhance;

/// <summary>
/// 工具类
/// </summary>
public static class Utils
{
    /// <summary>
    /// 插件配置
    /// </summary>
    public static PluginConfig Config { get; set; } = new();

    /// <summary>
    /// 自定义区域
    /// </summary>
    public static ConcurrentDictionary<Bot, string?> CustomUserCountry { get; } = [];

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatStaticResponse(string message)
    {
        return $"<ASFE> {message}";
    }

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatStaticResponse(string message, params object?[] args)
    {
        return FormatStaticResponse(string.Format(message, args));
    }

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatBotResponse(this Bot bot, string message)
    {
        return $"<{bot.BotName}> {message}";
    }

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatBotResponse(this Bot bot, string message, params object?[] args)
    {
        return bot.FormatBotResponse(string.Format(message, args));
    }

    internal static StringBuilder AppendLineFormat(this StringBuilder sb, string format, params object?[] args)
    {
        return sb.AppendLine(string.Format(format, args));
    }

    /// <summary>
    /// 获取个人资料链接
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static async Task<string?> GetProfileLink(this Bot bot)
    {
        return await bot.ArchiWebHandler.GetAbsoluteProfileURL(true).ConfigureAwait(false);
    }

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong SteamId2Steam32(ulong steamId)
    {
        return IsSteam32ID(steamId) ? steamId : steamId - 0x110000100000000;
    }

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong Steam322SteamId(ulong steamId)
    {
        return IsSteam32ID(steamId) ? steamId + 0x110000100000000 : steamId;
    }

    internal static bool IsSteam32ID(ulong id)
    {
        return id <= 0xFFFFFFFF;
    }

    /// <summary>
    /// 匹配Steam商店Id
    /// </summary>
    /// <param name="query"></param>
    /// <param name="validType"></param>
    /// <param name="defaultType"></param>
    /// <returns></returns>
    internal static List<SteamGameId> FetchGameIds(string query, ESteamGameIdType validType, ESteamGameIdType defaultType)
    {
        var result = new List<SteamGameId>();
        var entries = query.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in entries)
        {
            uint gameId;
            string strType;
            int index = entry.IndexOf('/', StringComparison.Ordinal);

            if ((index > 0) && (entry.Length > index + 1))
            {
                if (!uint.TryParse(entry[(index + 1)..], out gameId) || (gameId == 0))
                {
                    result.Add(new(entry, ESteamGameIdType.Error, 0));
                    continue;
                }

                strType = entry[..index];
            }
            else if (uint.TryParse(entry, out gameId) && (gameId > 0))
            {
                result.Add(new(entry, defaultType, gameId));
                continue;
            }
            else
            {
                result.Add(new(entry, ESteamGameIdType.Error, 0));
                continue;
            }

            ESteamGameIdType type = strType.ToUpperInvariant() switch
            {
                "A" or "APP" => ESteamGameIdType.App,
                "S" or "SUB" => ESteamGameIdType.Sub,
                "B" or "BUNDLE" => ESteamGameIdType.Bundle,
                _ => ESteamGameIdType.Error,
            };

            if (validType.HasFlag(type))
            {
                result.Add(new(entry, type, gameId));
            }
            else
            {
                result.Add(new(entry, ESteamGameIdType.Error, 0));
            }
        }
        return result;
    }

    /// <summary>
    /// 获取SessionId
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static string? FetchSessionId(Bot bot)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            return null;
        }
        var cc = bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookies(SteamCommunityURL);
        var sessionId = cc["sessionid"];
        return sessionId?.Value;
    }

    /// <summary>
    /// 绕过年龄检查
    /// </summary>
    /// <param name="webHandler"></param>
    internal static void BypassAgeCheck(this ArchiWebHandler webHandler)
    {
        var cookieContainer = webHandler.WebBrowser.CookieContainer;
        if (string.IsNullOrEmpty(cookieContainer.GetCookieValue(SteamStoreURL, "birthtime")))
        {
            cookieContainer.Add(new System.Net.Cookie("birthtime", "0", "/", $".{SteamStoreURL.Host}"));
        }
    }

    /// <summary>
    /// 获取版本号
    /// </summary>
    public static Version MyVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0.0.0.0");

    /// <summary>
    /// 获取ASF版本
    /// </summary>
    internal static Version ASFVersion => typeof(ASF).Assembly.GetName().Version ?? new Version("0.0.0.0");

    /// <summary>
    /// 获取插件所在路径
    /// </summary>
    internal static string MyLocation => Assembly.GetExecutingAssembly().Location;

    /// <summary>
    /// 获取插件所在文件夹路径
    /// </summary>
    internal static string MyDirectory => Path.GetDirectoryName(MyLocation) ?? ".";

    /// <summary>
    /// Steam商店链接
    /// </summary>
    internal static Uri SteamStoreURL => ArchiWebHandler.SteamStoreURL;

    /// <summary>
    /// Steam社区链接
    /// </summary>
    internal static Uri SteamCommunityURL => ArchiWebHandler.SteamCommunityURL;

    /// <summary>
    /// SteamAPI链接
    /// </summary>
    internal static Uri SteamApiURL => new("https://api.steampowered.com");

    /// <summary>
    /// Steam结算链接
    /// </summary>
    internal static Uri SteamCheckoutURL => ArchiWebHandler.SteamCheckoutURL;

    internal static Uri SteamHelpURL => ArchiWebHandler.SteamHelpURL;

    /// <summary>
    /// 日志
    /// </summary>
    public static ArchiLogger ASFLogger => ASF.ArchiLogger;

    /// <summary>
    /// 布尔转换为 char
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    internal static char Bool2Str(bool b) => b ? '√' : '×';
    internal static char ToStr(this bool b) => Bool2Str(b);

    /// <summary>
    /// 跳过参数获取Bot名称
    /// </summary>
    /// <param name="args"></param>
    /// <param name="skipStart"></param>
    /// <param name="skipEnd"></param>
    /// <returns></returns>
    internal static string SkipBotNames(string[] args, int skipStart, int skipEnd)
    {
        return string.Join(',', args[skipStart..(args.Length - skipEnd)]);
    }

    /// <summary>
    /// 命令是否被禁用
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    internal static bool IsCmdDisabled(string cmd)
    {
        return Config.DisabledCmds?.Contains(cmd) == true;
    }

    /// <summary>
    /// Protobuf 编码
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="payload"></param>
    /// <returns></returns>
    internal static string ProtoBufEncode<T>(T payload)
    {
        var ms = new MemoryStream();
        Serializer.Serialize(ms, payload);
        var enc = Convert.ToBase64String(ms.ToArray());
        return enc;
    }

    /// <summary>
    /// 逗号分隔符
    /// </summary>
    internal static readonly char[] SeparatorDot = [','];

    /// <summary>
    /// 加号分隔符
    /// </summary>
    internal static readonly char[] SeparatorPlus = ['+'];

    /// <summary>
    /// 逗号空格分隔符
    /// </summary>
    internal static readonly char[] SeparatorDotSpace = [',', ' '];

    /// <summary>
    /// 需要编码的URL字符串
    /// </summary>
    private static readonly ReadOnlyDictionary<char, string> CharacterEncodings = new Dictionary<char, string>() {
        {' ', "%20"}, {'!', "%21"}, {'\"', "%22"}, {'#', "%23"},  {'$', "%24"},
        {'%', "%25"}, {'&', "%26"}, {'\'', "%27"}, {'(', "%28"},  {')', "%29"},
        {'*', "%2A"}, {'+', "%2B"}, {',', "%2C"},  {'-', "%2D"},  {'.', "%2E"},
        {'/', "%2F"}, {':', "%3A"}, {';', "%3B"},  {'<', "%3C"},  {'=', "%3D"},
        {'>', "%3E"}, {'@', "%40"}, {'[', "%5B"},  {'\\', "%5C"}, {']', "%5D"},
        {'^', "%5E"}, {'_', "%5F"}, {'`', "%60"},  {'{', "%7B"},  {'|', "%7C"},
        {'}', "%7D"}, {'~', "%7E"},
    }.AsReadOnly();

    /// <summary>
    /// URL编码
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    internal static string UrlEncode(string url)
    {
        var encodedUrl = new StringBuilder();
        foreach (var c in url)
        {
            if (CharacterEncodings.TryGetValue(c, out var str))
            {
                encodedUrl.Append(str);
            }
            else
            {
                encodedUrl.Append(c);
            }
        }
        return encodedUrl.ToString();
    }

    public static Task<ObjectResponse<T>?> UrlPostToJsonObject<T>(this ArchiWebHandler handler,
        Uri request,
        IDictionary<string, string>? data = null,
        Uri? referer = null,
        WebBrowser.ERequestOptions requestOptions = WebBrowser.ERequestOptions.None,
        bool checkSessionPreemptively = true,
        byte maxTries = WebBrowser.MaxTries,
        int rateLimitingDelay = 0,
        bool allowSessionRefresh = true,
        CancellationToken cancellationToken = default)
    {
        return handler.UrlPostToJsonObjectWithSession<T>(request, null, data, referer, requestOptions, ESession.None, checkSessionPreemptively, maxTries, rateLimitingDelay, allowSessionRefresh, cancellationToken);
    }

    public static Task<HtmlDocumentResponse?> UrlPostToHtmlDocument(this ArchiWebHandler handler,
        Uri request,
        IDictionary<string, string>? data = null,
        Uri? referer = null,
        WebBrowser.ERequestOptions requestOptions = WebBrowser.ERequestOptions.None,
        bool checkSessionPreemptively = true,
        byte maxTries = WebBrowser.MaxTries,
        int rateLimitingDelay = 0,
        bool allowSessionRefresh = true,
        CancellationToken cancellationToken = default)
    {
        return handler.UrlPostToHtmlDocumentWithSession(request, null, data, referer, requestOptions, ESession.None, checkSessionPreemptively, maxTries, rateLimitingDelay, allowSessionRefresh, cancellationToken);
    }

    public static Task<bool> UrlPost(this ArchiWebHandler handler,
        Uri request,
        IDictionary<string, string>? data = null,
        Uri? referer = null,
        WebBrowser.ERequestOptions requestOptions = WebBrowser.ERequestOptions.None,
        bool checkSessionPreemptively = true,
        byte maxTries = WebBrowser.MaxTries,
        int rateLimitingDelay = 0,
        bool allowSessionRefresh = true,
        CancellationToken cancellationToken = default)
    {
        return handler.UrlPostWithSession(request, null, data, referer, requestOptions, ESession.None, checkSessionPreemptively, maxTries, rateLimitingDelay, allowSessionRefresh, cancellationToken);
    }

    internal static string GetGifteeProfile(ulong accountId)
    {
        ulong steam32;
        ulong steam64;

        if (IsSteam32ID(accountId))
        {
            steam32 = accountId;
            steam64 = Steam322SteamId(accountId);
        }
        else
        {
            steam32 = SteamId2Steam32(accountId);
            steam64 = accountId;
        }

        if (Bot.BotsReadOnly != null)
        {
            foreach (var bot in Bot.BotsReadOnly.Values)
            {
                if (bot.SteamID == steam64)
                {
                    return string.Format("{0} ({1})", bot.BotName, steam64);
                }
            }
        }

        return string.Format("{0} ({1})", steam32, steam64);
    }

    internal static string DefaultOrCurrentLanguage => Config.DefaultLanguage ?? Langs.Language;

    /// <summary>
    /// 货币代码转国家代码
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static string GetUserCountryCode(this Bot bot)
    {
        if (CustomUserCountry.TryGetValue(bot, out var code) && !string.IsNullOrEmpty(code))
        {
            return code;
        }

        return bot.WalletCurrency switch
        {
            ECurrencyCode.USD => "US",
            ECurrencyCode.GBP => "GB",
            ECurrencyCode.EUR => "EU",
            ECurrencyCode.CHF => "CH",
            ECurrencyCode.RUB => "RU",
            ECurrencyCode.PLN => "PL",
            ECurrencyCode.BRL => "BR",
            ECurrencyCode.JPY => "JP",
            ECurrencyCode.NOK => "NO",
            ECurrencyCode.IDR => "ID",
            ECurrencyCode.MYR => "MY",
            ECurrencyCode.PHP => "PH",
            ECurrencyCode.SGD => "SG",
            ECurrencyCode.THB => "TH",
            ECurrencyCode.VND => "VN",
            ECurrencyCode.KRW => "KR",
            ECurrencyCode.TRY => "TR",
            ECurrencyCode.UAH => "UA",
            ECurrencyCode.MXN => "MX",
            ECurrencyCode.CAD => "CA",
            ECurrencyCode.AUD => "CX",
            ECurrencyCode.NZD => "CK",
            ECurrencyCode.CNY => "CN",
            ECurrencyCode.INR => "IN",
            ECurrencyCode.CLP => "CL",
            ECurrencyCode.PEN => "PE",
            ECurrencyCode.COP => "CO",
            ECurrencyCode.ZAR => "ZA",
            ECurrencyCode.HKD => "HK",
            ECurrencyCode.TWD => "TW",
            ECurrencyCode.SAR => "SA",
            ECurrencyCode.AED => "AE",
            ECurrencyCode.ARS => "AR",
            ECurrencyCode.ILS => "IL",
            ECurrencyCode.BYN => "BY",
            ECurrencyCode.KZT => "KZ",
            ECurrencyCode.KWD => "KW",
            ECurrencyCode.QAR => "QA",
            ECurrencyCode.CRC => "CT",
            ECurrencyCode.UYU => "UY",
            ECurrencyCode.BGN => "BG",
            ECurrencyCode.HRK => "HR",
            ECurrencyCode.CZK => "CZ",
            ECurrencyCode.DKK => "DK",
            ECurrencyCode.HUF => "HU",
            ECurrencyCode.RON => "RO",
            _ => Langs.CountryCode,
        };
    }

    /// <summary>
    /// 统计
    /// </summary>
    /// <param name="_"></param>
    internal static async void StatisticCallback(object? _)
    {
        try
        {
            var request = new Uri("https://asfe.chrxw.com/asfenhace");
            if (_Adapter_.ExtensionCore.HasSubModule)
            {
                List<string> names = ["asfenhance"];
                foreach (var (subModules, _) in _Adapter_.ExtensionCore.SubModules)
                {
                    names.Add(subModules.ToLowerInvariant());
                }
                request = new Uri(request, string.Join('+', names));
            }

            await ASF.WebBrowser!.UrlGetToHtmlDocument(request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ASFLogger.LogGenericException(ex);
        }
    }

    /// <summary>
    /// 领取物品
    /// </summary>
    /// <param name="_"></param>
    internal static async void ClaimItemCallback(object? _)
    {
        try
        {
            var bots = Bot.GetBots(Config.AutoClaimItemBotNames!);
            if (bots == null || bots.Count == 0)
            {
                return;
            }
            foreach (var bot in bots)
            {
                var result = await Event.Command.ResponseClaimItem(bot).ConfigureAwait(false);
                ASFLogger.LogGenericInfo(result ?? "Null");
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            ASFLogger.LogGenericException(ex);
        }
    }
}
