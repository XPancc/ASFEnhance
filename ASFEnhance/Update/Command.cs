using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ASFEnhance.Data;
using ASFEnhance.Data.Plugin;
using ASFEnhance.Explorer;
using System.Collections.Frozen;
using System.Text;
using static ArchiSteamFarm.Storage.GlobalConfig;

namespace ASFEnhance.Update;

internal static class Command
{
    /// <summary>
    /// 获取已安装的插件列表
    /// </summary>
    /// <returns></returns>
    internal static string? ResponsePluginList()
    {
        FrozenSet<IPlugin>? activePlugins;
        try
        {
            activePlugins = typeof(PluginsCore).GetStaticPrivateProperty<FrozenSet<IPlugin>>("ActivePlugins");
        }
        catch (Exception ex)
        {
            ASFLogger.LogGenericException(ex);
            return FormatStaticResponse(Langs.SubModuleLoadFailed);
        }

        if (activePlugins != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(FormatStaticResponse(Langs.MultipleLineResult));
            sb.AppendLineFormat(Langs.PluginListTips, activePlugins.Count);

            var subModules = new Dictionary<string, SubModuleInfo>();
            foreach (var subModule in _Adapter_.ExtensionCore.SubModules.Values)
            {
                subModules.TryAdd(subModule.PluginName, subModule);
            }

            var index = 1;
            foreach (var plugin in activePlugins)
            {
                if (plugin.Name == "ASFEnhance")
                {
                    sb.AppendLineFormat(Langs.PluginListItem, index++, plugin.Name, plugin.Version, "ASFE");
                }
                else if (subModules.TryGetValue(plugin.Name, out var subModule))
                {
                    sb.AppendLineFormat(Langs.PluginListItem, index++, subModule.PluginName, subModule.PluginVersion, subModule.CmdPrefix ?? "---");
                }
                else
                {
                    sb.AppendLineFormat(Langs.PluginListItem2, index++, plugin.Name, plugin.Version);
                }
            }

            return sb.ToString();
        }
        else
        {
            //大概不可能会执行到这里
            return FormatStaticResponse(Langs.SubModuleNoModule);
        }
    }

    /// <summary>
    /// 获取插件最新版本
    /// </summary>
    /// <param name="pluginNames"></param>
    /// <returns></returns>
    internal static async Task<string?> ResponseGetPluginLatestVersion(string? pluginNames = null)
    {
        var entries = pluginNames?.ToUpperInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);

        var tasks = new List<Task<PluginUpdateResponse>>();

        if (entries?.Length > 0)
        {
            foreach (var entry in entries)
            {
                if (entry == "ASFE" || entry == "ASFENHANCE")
                {
                    tasks.Add(WebRequest.GetPluginReleaseNote("ASFEnhance", MyVersion, "ASFEnhance"));
                }
                else if (_Adapter_.ExtensionCore.SubModules.TryGetValue(entry, out var subModule))
                {
                    tasks.Add(WebRequest.GetPluginReleaseNote(subModule.PluginName, subModule.PluginVersion, subModule.RepoName));
                }
            }
        }
        else
        {
            tasks.Add(WebRequest.GetPluginReleaseNote("ASFEnhance", MyVersion, "ASFEnhance"));
            foreach (var subModule in _Adapter_.ExtensionCore.SubModules.Values)
            {
                tasks.Add(WebRequest.GetPluginReleaseNote(subModule.PluginName, subModule.PluginVersion, subModule.RepoName));
            }
        }

        if (tasks.Count == 0)
        {
            return FormatStaticResponse(Langs.UpdateFailedPluginNotFound, pluginNames);
        }

        var results = await Utilities.InParallel(tasks).ConfigureAwait(false);

        var sb = new StringBuilder();
        sb.AppendLine(FormatStaticResponse(Langs.UpdatePluginUpdateInfo));

        foreach (var info in results)
        {
            sb.AppendLine(Static.Line);
            sb.AppendLineFormat(Langs.UpdatePluginListItemName, info.PluginName, info.Tips);
            sb.AppendLineFormat(Langs.UpdatePluginListItemVersion, info.CurrentVersion, info.OnlineVersion?.ToString() ?? "-.-.-.-");
            if (!string.IsNullOrEmpty(info.ReleaseNote))
            {
                sb.AppendLineFormat(Langs.UpdatePluginListItemReleaseNote, info.ReleaseNote);
            }
        }

        sb.AppendLine(Static.Line);
        sb.AppendLine(Langs.UpdatePluginVersionTips);

        return sb.ToString();
    }

    /// <summary>
    /// 更新插件版本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="access"></param>
    /// <param name="pluginNames"></param>
    /// <returns></returns>
    internal static async Task<string?> ResponsePluginUpdate(Bot bot, EAccess access, string? pluginNames = null)
    {
        var entries = pluginNames?.ToUpperInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);

#if DEBUG
        var channel = nameof(EUpdateChannel.PreRelease);
#else
        var channel = nameof(EUpdateChannel.Stable);
#endif

        List<string> plugins = [];

        if (entries?.Length > 0)
        {
            foreach (var entry in entries)
            {
                if (entry == "ASFE" || entry == "ASFENHANCE")
                {
                    plugins.Add("ASFEnhance");
                }

                foreach (var (pluginId, subModule) in _Adapter_.ExtensionCore.SubModules)
                {
                    if (pluginId == entry || subModule.MatchCmdPrefix(entry))
                    {
                        plugins.Add(subModule.PluginName);
                    }
                }
            }
        }
        else
        {
            plugins.Add("ASFEnhance");
            foreach (var subModule in _Adapter_.ExtensionCore.SubModules.Values)
            {
                plugins.Add(subModule.PluginName);
            }
        }

        if (plugins.Count == 0)
        {
            return FormatStaticResponse(Langs.UpdateFailedPluginNotFound, pluginNames);
        }

        var cmd = string.Format("UPDATEPLUGINS {0} {1}", channel, string.Join(',', plugins));
        return await bot.Commands.Response(access, cmd, 0).ConfigureAwait(false);
    }
}
