using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace LordKeeper
{
    public class LordKeeperSettings : AttributeGlobalSettings<LordKeeperSettings>
    {
        private const string LordsGroup = "{=lord_keeper_settings_group_lords}Captured lords";
        private const string OverLimitGroup = "{=lord_keeper_settings_group_over}Prisoners over the party limit";
        private const string MessagesGroup = "{=lord_keeper_settings_group_messages}Messages";
        private const string LordsHint = "{=lord_keeper_settings_lords_hint}Share of the vanilla daily escape chance of captured lords that still applies. 0% = they never escape, 100% = vanilla.";
        private const string OverLimitHint = "{=lord_keeper_settings_over_hint}Share of the vanilla hourly escape chance of prisoners held above the party's prisoner limit that still applies. 0% = they never escape, 100% = vanilla.";

        public override string Id => "LordKeeperSettings";
        public override string DisplayName => new TaleWorlds.Localization.TextObject("{=lord_keeper_settings_name}Lord Keeper").ToString();
        public override string FolderName => "LordKeeper";
        public override string FormatType => "json2";

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_lords_party}In your party", 0f, 1f, "#0%", Order = 1, RequireRestart = false, HintText = LordsHint)]
        [SettingPropertyGroup(LordsGroup, GroupOrder = 1)]
        public float LordsInPlayerParty { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_lords_clan_parties}In your clan's parties", 0f, 1f, "#0%", Order = 2, RequireRestart = false, HintText = LordsHint)]
        [SettingPropertyGroup(LordsGroup, GroupOrder = 1)]
        public float LordsInClanParties { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_lords_fiefs}In your clan's towns and castles", 0f, 1f, "#0%", Order = 3, RequireRestart = false, HintText = LordsHint)]
        [SettingPropertyGroup(LordsGroup, GroupOrder = 1)]
        public float LordsInClanFiefs { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_lords_kingdom}With other clans of your kingdom", 0f, 1f, "#0%", Order = 4, RequireRestart = false, HintText = LordsHint)]
        [SettingPropertyGroup(LordsGroup, GroupOrder = 1)]
        public float LordsInKingdom { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_over_party}Your party", 0f, 1f, "#0%", Order = 1, RequireRestart = false, HintText = OverLimitHint)]
        [SettingPropertyGroup(OverLimitGroup, GroupOrder = 2)]
        public float OverLimitInPlayerParty { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=lord_keeper_settings_over_clan_parties}Your clan's parties", 0f, 1f, "#0%", Order = 2, RequireRestart = false, HintText = OverLimitHint)]
        [SettingPropertyGroup(OverLimitGroup, GroupOrder = 2)]
        public float OverLimitInClanParties { get; set; } = 1f;

        [SettingPropertyBool("{=lord_keeper_settings_show_caught_name}Show caught escape attempts", Order = 1, RequireRestart = false, HintText = "{=lord_keeper_settings_show_caught_hint}Show a message when a lord tries to escape from you or your clan but is caught thanks to this mod.")]
        [SettingPropertyGroup(MessagesGroup, GroupOrder = 3)]
        public bool ShowCaughtEscapes { get; set; } = true;

        public float LordEscapeFactor(CaptorScope scope)
        {
            switch (scope)
            {
                case CaptorScope.PlayerParty: return LordsInPlayerParty;
                case CaptorScope.ClanParty: return LordsInClanParties;
                case CaptorScope.ClanFief: return LordsInClanFiefs;
                case CaptorScope.Kingdom: return LordsInKingdom;
                default: return 1f;
            }
        }

        public float OverLimitEscapeFactor(CaptorScope scope)
        {
            switch (scope)
            {
                case CaptorScope.PlayerParty: return OverLimitInPlayerParty;
                case CaptorScope.ClanParty: return OverLimitInClanParties;
                default: return 1f;
            }
        }
    }
}
