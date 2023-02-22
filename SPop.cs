using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Oxide.Plugins
{
    [Info("SPop", "Gt403cyl2", "1.0.0")]
    [Description("Simple server population command")]
    public class SPop : RustPlugin
    {
        #region Config
        private ConfigData _configData;
        private class ConfigData
        {
            [JsonProperty(PropertyName = "Use permissions:")]
            public bool usePerms = false;
            [JsonProperty(PropertyName = "Population Command:")]
            public string popCommand = "pop";

            public string ToJson() => JsonConvert.SerializeObject(this);
            public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
        }
        private void Init()
        {
            cmd.AddChatCommand(_configData.popCommand, this, CommandPop);
            permission.RegisterPermission("spop.use", this);
        }
        protected override void LoadDefaultConfig() => _configData = new ConfigData();
        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _configData = Config.ReadObject<ConfigData>();
                if (_configData == null)
                {
                    throw new JsonException();
                }
                if (!_configData.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
                {
                    PrintWarning("Configuration appears to be outdated; Updating and saving");
                    SaveConfig();
                }
            }
            catch
            {
                PrintWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }
        protected override void SaveConfig()
        {
            PrintWarning($"Configuration changes saved to {Name}.json");
            Config.WriteObject(_configData, true);
        }
        #endregion

        #region Commands
        private void CommandPop(BasePlayer player, string commands, string[] args)
        {
            int pCount = 0;
            int psCount = 0;
            int pJoin = ServerMgr.Instance.connectionQueue.Joining;
            int pQueued = ServerMgr.Instance.connectionQueue.Queued;

            if (_configData.usePerms && !permission.UserHasPermission(player.UserIDString, "spop.use"))
            {
                player.ChatMessage($"{lang.GetMessage("NoPermission", this)}");
                return;
            }
            else
            {
                foreach (BasePlayer playerc in BasePlayer.activePlayerList)
                {
                    pCount++;
                }
                foreach (BasePlayer players in BasePlayer.allPlayerList)
                {
                    if (players.IsSleeping()) psCount++;
                }
                if (pJoin == 0 && pQueued == 0)
                {
                    player.ChatMessage($"{lang.GetMessage("ReportOnline", this)}" + pCount + "\n" + $"{lang.GetMessage("ReportSleeping", this)}" + psCount);
                    return;
                }
                if (pJoin != 0)
                {
                    player.ChatMessage($"{lang.GetMessage("ReportOnline", this)}" + pCount + "\n" + $"{lang.GetMessage("ReportSleeping", this)}" + psCount + "\n" + $"{lang.GetMessage("ReportJoining", this)}" + pJoin);
                    return;
                }
                if (pQueued != 0)
                {
                    player.ChatMessage($"{lang.GetMessage("ReportOnline", this)}" + pCount + "\n" + $"{lang.GetMessage("ReportSleeping", this)}" + psCount + "\n" + $"{lang.GetMessage("ReportQueued", this)}" + pQueued);
                    return;
                }
                if (pJoin != 0 && pQueued != 0)
                { 
                player.ChatMessage($"{lang.GetMessage("ReportOnline", this)}" + pCount + "\n" + $"{lang.GetMessage("ReportSleeping", this)}" + psCount + "\n" + $"{lang.GetMessage("ReportJoining", this)}" + pJoin + "\n" + $"{lang.GetMessage("ReportQueued", this)}" + pQueued);
                    return;
                }
            }
        }
        #endregion

        #region Lang
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = $"You don't have permission to use this command.",
                ["ReportOnline"] = $"Players Online: ",
                ["ReportSleeping"] = $"Players Sleeping: ",
                ["ReportJoining"] = $"Players Joining: ",
                ["ReportQueued"] = $"Players Queued: "
            }, this);
        }
        #endregion        
    }
}