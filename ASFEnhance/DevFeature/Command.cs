using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using System.Text;

namespace ASFEnhance.DevFeature;

internal static class Command
{
    /// <summary>
    /// 获取商店Cookies
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static string? ResponseGetCookies(Bot bot)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            return bot.FormatBotResponse(Strings.BotNotConnected);
        }

        var response = new StringBuilder();

        response.AppendLine(Langs.MultipleLineResult);
        response.AppendLine(Langs.ClientCookies);

        var cc = bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookies(SteamStoreURL);

        foreach (var c in cc.ToList())
        {
            response.AppendLineFormat(Langs.CookieItem, c.Name, c.Value);
        }

        return bot.FormatBotResponse(response.ToString());
    }

    /// <summary>
    /// 获取商店Cookies (多个 bot)
    /// </summary>
    /// <param name="botNames"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static async Task<string?> ResponseGetCookies(string botNames)
    {
        if (string.IsNullOrEmpty(botNames))
        {
            throw new ArgumentNullException(nameof(botNames));
        }

        var bots = Bot.GetBots(botNames);

        if ((bots == null) || (bots.Count == 0))
        {
            return FormatStaticResponse(Strings.BotNotFound, botNames);
        }

        var results = await Utilities.InParallel(bots.Select(bot => Task.Run(() => ResponseGetCookies(bot)))).ConfigureAwait(false);

        var responses = new List<string?>(results.Where(result => !string.IsNullOrEmpty(result)));

        return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
    }

    /// <summary>
    /// 获取Bot AccessToken
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static Task<string?> ResponseGetAccessToken(Bot bot)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            return Task.FromResult<string?>(bot.FormatBotResponse(Strings.BotNotConnected));
        }

        var accessToken = bot.AccessToken;

        bool success = !string.IsNullOrEmpty(accessToken);

        return Task.FromResult<string?>(bot.FormatBotResponse(success ? accessToken! : string.Format(Langs.FetchDataFailed, nameof(accessToken))));
    }

    /// <summary>
    /// 获取Bot AccessToken (多个 bot)
    /// </summary>
    /// <param name="botNames"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static async Task<string?> ResponseGetAccessToken(string botNames)
    {
        if (string.IsNullOrEmpty(botNames))
        {
            throw new ArgumentNullException(nameof(botNames));
        }

        var bots = Bot.GetBots(botNames);

        if ((bots == null) || (bots.Count == 0))
        {
            return FormatStaticResponse(Strings.BotNotFound, botNames);
        }

        var results = await Utilities.InParallel(bots.Select(bot => Task.Run(() => ResponseGetAccessToken(bot)))).ConfigureAwait(false);

        var responses = new List<string?>(results.Where(result => !string.IsNullOrEmpty(result)));

        return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
    }
}
