using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;
using ASFEnhance.Data;
using ASFEnhance.Data.Common;
using ASFEnhance.Data.IAccountCartService;
using ASFEnhance.Data.Plugin;
using ASFEnhance.Data.WebApi;
using System.Net;

namespace ASFEnhance.Cart;

/// <summary>
/// 网络请求
/// </summary>
public static class WebRequest
{
    /// <summary>
    ///     读取当前购物车
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    public static async Task<GetCartResponse?> GetAccountCart(Bot bot)
    {
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var userCountry = bot.GetUserCountryCode();
        var request = new Uri(SteamApiURL,
            $"/IAccountCartService/GetCart/v1/?access_token={token}&user_country={userCountry}");
        var response = await bot.ArchiWebHandler
            .UrlGetToJsonObjectWithSession<AbstractResponse<GetCartResponse>>(request, referer: SteamStoreURL)
            .ConfigureAwait(false);
        return response?.Content?.Response;
    }

    /// <summary>
    ///     批量添加购物车项目
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameIds"></param>
    /// <param name="isPrivate"></param>
    /// <param name="giftInfo"></param>
    /// <returns></returns>
    /// <exception cref="AccessTokenNullException"></exception>
    internal static async Task<AddItemsToCartResponse?> AddItemsToAccountsCart(Bot bot, List<SteamGameId> gameIds,
        bool isPrivate, GiftInfoData? giftInfo)
    {
        var items = gameIds.Select(x => new AddItemsToCartRequest.ItemData
        {
            PackageId = x.Type == ESteamGameIdType.Sub ? x.Id : null,
            BundleId = x.Type == ESteamGameIdType.Bundle ? x.Id : null,
            GIftInfo = giftInfo,
            Flags = new FlagsData { IsPrivate = isPrivate, IsGift = giftInfo != null }
        });

        var payload = new AddItemsToCartRequest
        {
            Items = items.ToList(),
            UserCountry = bot.GetUserCountryCode(),
            Navdata = new NavdataData
            {
                Domain = "store.steampowered.com",
                Controller = "default",
                Method = "default",
                SubMethod = "",
                Feature = "spotlight",
                Depth = 1,
                CountryCode = Langs.CountryCode,
                WebKey = 0,
                IsClient = false,
                CuratorData = new CuratorData { ClanId = null, ListId = null },
                IsLikelyBot = false,
                IsUtm = false
            }
        };

        var json = payload.ToJsonText();
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var request = new Uri(SteamApiURL, $"/IAccountCartService/AddItemsToCart/v1/?access_token={token}");
        var data = new Dictionary<string, string> { { "input_json", json } };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObject<AbstractResponse<AddItemsToCartResponse>>(request, data, SteamStoreURL)
            .ConfigureAwait(false);
        return response?.Content?.Response;
    }

    /// <summary>
    ///     添加购物车项目
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="AccessTokenNullException"></exception>
    public static async Task<AddItemsToCartResponse?> AddItemsToAccountCart(Bot bot,
        List<AddItemsToCartRequest.ItemData> items)
    {
        var payload = new AddItemsToCartRequest
        {
            Items = items,
            UserCountry = bot.GetUserCountryCode(),
            Navdata = new NavdataData
            {
                Domain = "store.steampowered.com",
                Controller = "default",
                Method = "default",
                SubMethod = "",
                Feature = "spotlight",
                Depth = 1,
                CountryCode = Langs.CountryCode,
                WebKey = 0,
                IsClient = false,
                CuratorData = new CuratorData { ClanId = null, ListId = null },
                IsLikelyBot = false,
                IsUtm = false
            }
        };

        var json = payload.ToJsonText();
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var request = new Uri(SteamApiURL, $"/IAccountCartService/AddItemsToCart/v1/?access_token={token}");
        var data = new Dictionary<string, string> { { "input_json", json } };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObject<AbstractResponse<AddItemsToCartResponse>>(request, data, SteamStoreURL)
            .ConfigureAwait(false);
        return response?.Content?.Response;
    }

    /// <summary>
    ///     修改购物车项目
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="lineItemId"></param>
    /// <param name="isPrivate"></param>
    /// <param name="giftInfo"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="AccessTokenNullException"></exception>
    internal static async Task<AddItemsToCartResponse?> ModifyLineItemsOfAccountCart(Bot bot, ulong lineItemId,
        bool isPrivate, GiftInfoData? giftInfo)
    {
        var payload = new ModifyLineItemRequest
        {
            LineItemId = lineItemId,
            UserCountry = bot.GetUserCountryCode(),
            GiftInfo = giftInfo,
            Flags = new FlagsData { IsPrivate = isPrivate, IsGift = giftInfo != null }
        };

        var json = payload.ToJsonText();
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var request = new Uri(SteamApiURL, $"/IAccountCartService/ModifyLineItem/v1/?access_token={token}");
        var data = new Dictionary<string, string> { { "input_json", json } };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObject<AbstractResponse<AddItemsToCartResponse>>(request, data, SteamStoreURL)
            .ConfigureAwait(false);
        return response?.Content?.Response;
    }

    /// <summary>
    ///     移除购物车项目
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="lineItemId"></param>
    /// <returns></returns>
    /// <exception cref="AccessTokenNullException"></exception>
    public static async Task<AddItemsToCartResponse?> RemoveItemFromAccountCart(Bot bot, ulong lineItemId)
    {
        var payload = new ModifyLineItemRequest { LineItemId = lineItemId, UserCountry = bot.GetUserCountryCode() };

        var json = payload.ToJsonText();
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var request = new Uri(SteamApiURL, $"/IAccountCartService/RemoveItemFromCart/v1/?access_token={token}");
        var data = new Dictionary<string, string> { { "input_json", json } };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObject<AbstractResponse<AddItemsToCartResponse>>(request, data, SteamStoreURL)
            .ConfigureAwait(false);
        return response?.Content?.Response;
    }

    /// <summary>
    ///     清空当前购物车
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    public static async Task<bool?> ClearAccountCart(Bot bot)
    {
        var token = bot.AccessToken ?? throw new AccessTokenNullException(bot);
        var request = new Uri(SteamApiURL, $"/IAccountCartService/DeleteCart/v1/?access_token={token}");

        var response = await bot.ArchiWebHandler.UrlPostToJsonObject<AbstractResponse>(request, null, SteamStoreURL)
            .ConfigureAwait(false);
        return response?.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    ///     读取购物车可用区域信息
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static async Task<string?> CartGetCountries(Bot bot)
    {
        var request = new Uri(SteamStoreURL, "/cart/");

        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);

        return HtmlParser.ParseCartCountries(response);
    }

    /// <summary>
    ///     结算购物车
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="asGift"></param>
    /// <returns></returns>
    public static async Task<HtmlDocumentResponse?> CheckOut(Bot bot)
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/?accountcart=1");
        var referer = new Uri(SteamStoreURL, "/cart/");

        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request, referer: referer)
            .ConfigureAwait(false);

        if (response == null)
        {
            bot.ArchiLogger.LogNullError(nameof(response));
            return null;
        }

        return response;
    }

    /// <summary>
    ///     初始化付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    public static async Task<InitTransactionResponse?> InitTransaction(Bot bot, AddressConfig? address = null)
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/inittransaction/");
        var referer = new Uri(SteamCheckoutURL, "/checkout/");

        var data = new Dictionary<string, string>(4, StringComparer.Ordinal)
        {
            { "gidShoppingCart", "-1" },
            { "gidReplayOfTransID", "-1" },
            { "bUseAccountCart", "1" },
            { "PaymentMethod", "steamaccount" },
            { "abortPendingTransactions", "0" },
            { "bHasCardInfo", "0" },
            { "CardNumber", "" },
            { "CardExpirationYear", "" },
            { "CardExpirationMonth", "" },
            { "FirstName", address?.FirstName ?? "" },
            { "LastName", address?.LastName ?? address?.FirstName ?? "" },
            { "Address", address?.Address ?? "" },
            { "AddressTwo", "" },
            { "Country", address?.Country ?? "" },
            { "City", address?.City ?? "" },
            { "State", address?.State ?? "" },
            { "PostalCode", address?.PostCode ?? "" },
            { "Phone", "" },
            { "ShippingFirstName", "" },
            { "ShippingLastName", "" },
            { "ShippingAddress", "" },
            { "ShippingAddressTwo", "" },
            { "ShippingCountry", address?.Country ?? "" },
            { "ShippingCity", "" },
            { "ShippingState", "" },
            { "ShippingPostalCode", "" },
            { "ShippingPhone", "" },
            { "bIsGift", "0" },
            { "GifteeAccountID", "" },
            { "GifteeEmail", "" },
            { "GifteeName", "" },
            { "GiftMessage", "" },
            { "Sentiment", "" },
            { "Signature", "" },
            { "ScheduledSendOnDate", "0" },
            { "BankCode", "" },
            { "BankIBAN", "" },
            { "BankBIC", "" },
            { "TPBankID", "" },
            { "BankAccountID", "" },
            { "bSaveBillingAddress", "1" },
            { "gidPaymentID", "" },
            { "bUseRemainingSteamAccount", "1" },
            { "bPreAuthOnly", "0" }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObjectWithSession<InitTransactionResponse>(request, data: data, referer: referer)
            .ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    ///     取消付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="transid"></param>
    /// <returns></returns>
    public static async Task<BaseResultResponse?> CancelTransaction(Bot bot, string transid)
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/canceltransaction/");
        var referer = new Uri(SteamCheckoutURL, "/checkout/");

        var data = new Dictionary<string, string>(4, StringComparer.Ordinal) { { "transid", transid } };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObjectWithSession<BaseResultResponse>(request, data: data, referer: referer)
            .ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    ///     获取购物车总价格
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="transId"></param>
    /// <returns></returns>
    public static async Task<FinalPriceResponse?> GetFinalPrice(Bot bot, string transId)
    {
        var request = new Uri(SteamCheckoutURL,
            $"/checkout/getfinalprice/?count=1&transid={transId}&purchasetype=self&microtxnid=-1&cart=-1&gidReplayOfTransID=-1");
        var referer = new Uri(SteamCheckoutURL, "/checkout/");

        var response = await bot.ArchiWebHandler
            .UrlGetToJsonObjectWithSession<FinalPriceResponse>(request, referer: referer).ConfigureAwait(false);

        return response?.Content;
    }

    /// <summary>
    ///     完成付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="transId"></param>
    /// <returns></returns>
    public static async Task<TransactionStatusResponse?> FinalizeTransaction(Bot bot, string transId)
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/finalizetransaction/");
        var referer = new Uri(SteamCheckoutURL, "/checkout/");

        var data = new Dictionary<string, string>(3, StringComparer.Ordinal)
        {
            { "transid", transId },
            { "CardCVV2", "" },
            {
                "browserInfo",
                @"{""language"":""zh-CN"",""javaEnabled"":""false"",""colorDepth"":24,""screenHeight"":1080,""screenWidth"":1920}"
            }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObjectWithSession<FinalizeTransactionResponse>(request, data: data, referer: referer)
            .ConfigureAwait(false);

        request = new Uri(SteamCheckoutURL, $"/checkout/transactionstatus/?count=1&transid={transId}");

        var response2 = await bot.ArchiWebHandler
            .UrlGetToJsonObjectWithSession<TransactionStatusResponse>(request, referer: referer).ConfigureAwait(false);

        if (response?.Content == null)
        {
            bot.ArchiLogger.LogNullError(nameof(response));
            return null;
        }

        if (response2?.Content == null)
        {
            bot.ArchiLogger.LogNullError(nameof(response2));
            return null;
        }

        return response2.Content;
    }

    /// <summary>
    ///     获取数字礼品卡可用面额
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static async Task<List<DigitalGiftCardOption>?> GetDigitalGiftCardOptions(Bot bot)
    {
        var request = new Uri(SteamStoreURL, "/digitalgiftcards/selectgiftcard");
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);

        return HtmlParser.ParseDigitalGiftCardOptions(response);
    }

    /// <summary>
    ///     提交礼品卡支付
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    internal static async Task<HtmlDocumentResponse?> SubmitGiftCard(Bot bot, uint amount)
    {
        var request = new Uri(SteamStoreURL, "/digitalgiftcards/submitgiftcard");
        var referer = new Uri(SteamStoreURL, "/digitalgiftcards/selectgiftcard");

        var data = new Dictionary<string, string>(4, StringComparer.Ordinal)
        {
            { "action", "add_to_cart" },
            { "currency", bot.WalletCurrency.ToString() },
            { "amount", amount.ToString() }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToHtmlDocumentWithSession(request, data: data, referer: referer,
                session: ArchiWebHandler.ESession.PascalCase).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    ///     初始化礼品卡付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="steamId32"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    [Obsolete("未使用")]
    internal static async Task<PurchaseResponse?> InitTransactionDigicalCard(Bot bot, ulong steamId32,
        string method = "alipay")
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/inittransaction/");

        var shoppingCartId =
            bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookieValue(new Uri(SteamCheckoutURL, "/checkout/"),
                "beginCheckoutCart");

        if (string.IsNullOrEmpty(shoppingCartId))
        {
            if (string.IsNullOrEmpty(shoppingCartId))
            {
                bot.ArchiLogger.LogNullError(nameof(shoppingCartId));
                return null;
            }
        }

        var referer = new Uri(SteamCheckoutURL, $"/checkout?cart={shoppingCartId}&purchasetype=gift");

        var data = new Dictionary<string, string>(11, StringComparer.Ordinal)
        {
            { "gidShoppingCart", "-1" },
            { "gidReplayOfTransID", "-1" },
            { "PaymentMethod", method },
            { "abortPendingTransactions", "0" },
            { "bHasCardInfo", "0" },
            { "CardNumber", "" },
            { "CardExpirationYear", "" },
            { "CardExpirationMonth", "" },
            { "FirstName", "" },
            { "LastName", "" },
            { "Address", "" },
            { "AddressTwo", "" },
            { "Country", "CN" },
            { "City", "" },
            { "State", "" },
            { "PostalCode", "" },
            { "Phone", "" },
            { "ShippingFirstName", "" },
            { "ShippingLastName", "" },
            { "ShippingAddress", "" },
            { "ShippingAddressTwo", "" },
            { "ShippingCountry", "CN" },
            { "ShippingCity", "" },
            { "ShippingState", "" },
            { "ShippingPostalCode", "" },
            { "ShippingPhone", "" },
            { "bIsGift", "1" },
            { "GifteeAccountID", steamId32.ToString() },
            { "GifteeEmail", "" },
            { "GifteeName", Langs.GifteeName },
            {
                "GiftMessage", string.Format(Langs.GiftMessage, nameof(ASFEnhance), MyVersion.Major, MyVersion.Minor,
                    MyVersion.Build, MyVersion.Revision)
            },
            { "Sentiment", "祝你好运" },
            { "Signature", string.Format(Langs.GiftSignature, nameof(ASFEnhance)) },
            { "ScheduledSendOnDate", "0" },
            { "BankAccount", "" },
            { "BankCode", "" },
            { "BankIBAN", "" },
            { "BankBIC", "" },
            { "TPBankID", "" },
            { "BankAccountID", "" },
            { "bSaveBillingAddress", "1" },
            { "gidPaymentID", "" },
            { "bUseRemainingSteamAccount", "0" },
            { "bPreAuthOnly", "0" }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObjectWithSession<PurchaseResponse>(request, data: data, referer: referer)
            .ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    ///     提交充值卡支付
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    internal static async Task<HtmlDocumentResponse?> SubmitAddFunds(Bot bot, long amount)
    {
        var request = new Uri(SteamStoreURL, "/steamaccount/addfundssubmit");
        var referer = new Uri(SteamStoreURL, "/steamaccount/addfunds");

        var data = new Dictionary<string, string>(4, StringComparer.Ordinal)
        {
            { "action", "add_to_cart" },
            { "currency", bot.WalletCurrency.ToString() },
            { "amount", amount.ToString() },
            { "mtreturnurl", "" }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToHtmlDocumentWithSession(request, data: data, referer: referer,
                session: ArchiWebHandler.ESession.CamelCase).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    ///     初始化充值卡付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gidShoppingCart"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    internal static async Task<PurchaseResponse?> InitTransactionAddFunds(Bot bot, string gidShoppingCart,
        string method = "alipay")
    {
        var request = new Uri(SteamCheckoutURL, "/checkout/inittransaction/");
        var referer = new Uri(SteamCheckoutURL, $"/checkout?cart={gidShoppingCart}&microtxn=-1");
        var country = bot.GetUserCountryCode();
        var data = new Dictionary<string, string>(11, StringComparer.Ordinal)
        {
            { "gidShoppingCart", gidShoppingCart },
            { "gidReplayOfTransID", "-1" },
            { "bUseAccountCart", "0" },
            { "PaymentMethod", method },
            { "abortPendingTransactions", "0" },
            { "bHasCardInfo", "0" },
            { "CardNumber", "" },
            { "CardExpirationYear", "" },
            { "CardExpirationMonth", "" },
            { "FirstName", "" },
            { "LastName", "" },
            { "Address", "" },
            { "AddressTwo", "" },
            { "Country", country },
            { "City", "" },
            { "State", "" },
            { "PostalCode", "" },
            { "Phone", "" },
            { "ShippingFirstName", "" },
            { "ShippingLastName", "" },
            { "ShippingAddress", "" },
            { "ShippingAddressTwo", "" },
            { "ShippingCountry", country },
            { "ShippingCity", "" },
            { "ShippingState", "" },
            { "ShippingPostalCode", "" },
            { "ShippingPhone", "" },
            { "bIsGift", "0" },
            { "GifteeAccountID", "0" },
            { "GifteeEmail", "" },
            { "GifteeName", "" },
            { "GiftMessage", "" },
            { "Sentiment", "" },
            { "Signature", "" },
            { "ScheduledSendOnDate", "0" },
            { "BankAccount", "" },
            { "BankCode", "" },
            { "BankIBAN", "" },
            { "BankBIC", "" },
            { "TPBankID", "" },
            { "BankAccountID", "" },
            { "bSaveBillingAddress", "1" },
            { "gidPaymentID", "" },
            { "bUseRemainingSteamAccount", "0" },
            { "bPreAuthOnly", "0" }
        };

        var response = await bot.ArchiWebHandler
            .UrlPostToJsonObjectWithSession<PurchaseResponse>(request, data: data, referer: referer)
            .ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    ///     充值卡付款
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gidShoppingCart"></param>
    /// <param name="transId"></param>
    /// <returns></returns>
    internal static async Task<TransactionStatusResponse?> TransactionStatusAddFunds(Bot bot, string gidShoppingCart,
        string transId)
    {
        var request = new Uri(SteamCheckoutURL, $"/checkout/transactionstatus/?count=1&transid={transId}");
        var referer = new Uri(SteamCheckoutURL, $"/checkout?cart={gidShoppingCart}&microtxn=-1");

        var response = await bot.ArchiWebHandler
            .UrlGetToJsonObjectWithSession<TransactionStatusResponse>(request, referer: referer).ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    ///     充值卡结算, 跳转外部支付
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gidShoppingCart"></param>
    /// <param name="transId"></param>
    /// <returns></returns>
    internal static async Task<ExternalLinkCheckoutPayload?> CheckoutExternalLinkAddFunds(Bot bot,
        string gidShoppingCart, string transId)
    {
        var request = new Uri(SteamCheckoutURL, $"/checkout/externallink/?transid={transId}");
        var referer = new Uri(SteamCheckoutURL, $"/checkout?cart={gidShoppingCart}&microtxn=-1");

        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request, referer: referer)
            .ConfigureAwait(false);

        var form = response?.Content?.QuerySelector("form#externalForm");
        if (form == null)
        {
            return null;
        }

        var formUri = form.GetAttribute("action");
        if (!Uri.TryCreate(formUri, UriKind.Absolute, out var uri))
        {
            return null;
        }

        Dictionary<string, string> payload = [];
        var inputs = form.QuerySelectorAll("input");
        foreach (var input in inputs)
        {
            var name = input.GetAttribute("name");
            var value = input.GetAttribute("value");
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                continue;
            }

            payload.Add(name, value);
        }

        var result = new ExternalLinkCheckoutPayload(uri, payload);
        return result;
    }

    internal static async Task<Uri?> GetRealExternalPaymentLink(Bot bot, ExternalLinkCheckoutPayload payload)
    {
        var response = await bot.ArchiWebHandler.UrlPostToHtmlDocument(payload.FormUrl, payload.Payload)
            .ConfigureAwait(false);
        return response?.FinalUri;
    }
}