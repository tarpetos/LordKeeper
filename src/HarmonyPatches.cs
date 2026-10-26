using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace LordKeeper
{
    public enum CaptorScope
    {
        Other,
        PlayerParty,
        ClanParty,
        ClanFief,
        Kingdom
    }

    public static class Captors
    {
        public static CaptorScope Classify(PartyBase captor)
        {
            if (captor == null) return CaptorScope.Other;
            if (captor == PartyBase.MainParty) return CaptorScope.PlayerParty;

            if (captor.IsSettlement)
            {
                Clan owner = captor.Settlement?.OwnerClan;
                if (owner == Clan.PlayerClan) return CaptorScope.ClanFief;
                return IsKingdomMate(owner) ? CaptorScope.Kingdom : CaptorScope.Other;
            }

            if (captor.IsMobile)
            {
                Clan owner = captor.MobileParty?.ActualClan;
                if (owner == Clan.PlayerClan) return CaptorScope.ClanParty;
                return IsKingdomMate(owner) ? CaptorScope.Kingdom : CaptorScope.Other;
            }

            return CaptorScope.Other;
        }

        private static bool IsKingdomMate(Clan clan)
        {
            Kingdom kingdom = Clan.PlayerClan?.Kingdom;
            return clan != null && kingdom != null && clan.Kingdom == kingdom;
        }
    }

    // Holds the hero whose escape the mod is currently deciding on, set while a vanilla escape roll runs.
    public static class EscapeGuard
    {
        public static Hero Hero;
        public static CaptorScope Scope;
        public static float Factor;

        public static void Set(Hero hero, CaptorScope scope, float factor)
        {
            Hero = hero;
            Scope = scope;
            Factor = factor;
        }

        public static void Clear() => Hero = null;
    }

    // The vanilla daily tick rolls the escape and, on success, ends with EndCaptivityAction.ApplyByEscape.
    // While that tick runs for a hero we keep, the escape only goes through with the configured probability,
    // so the vanilla escape chance is scaled by exactly that factor.
    [HarmonyPatch(typeof(PrisonerReleaseCampaignBehavior), "DailyHeroTick")]
    public static class DailyEscapeRollPatch
    {
        public static void Prefix(Hero hero)
        {
            EscapeGuard.Clear();
            LordKeeperSettings settings = LordKeeperSettings.Instance;
            if (settings == null || hero == null || !hero.IsPrisoner) return;

            CaptorScope scope = Captors.Classify(hero.PartyBelongedToAsPrisoner);
            float factor = settings.LordEscapeFactor(scope);
            if (factor < 1f) EscapeGuard.Set(hero, scope, factor);
        }

        public static void Postfix() => EscapeGuard.Clear();
    }

    // Vanilla HourlyPartyTick: while a party holds more prisoners than its limit, it picks a random prisoner
    // for every prisoner over the limit and rolls that prisoner's escape. For the parties we keep, the same loop
    // runs here, but each roll only happens with the configured probability (for lords, the escape itself is
    // decided by EscapeGuard so a caught attempt can be reported).
    [HarmonyPatch(typeof(PrisonerReleaseCampaignBehavior), "HourlyPartyTick")]
    public static class OverLimitEscapePatch
    {
        private static readonly MethodInfo ApplyEscapeChance =
            AccessTools.Method(typeof(PrisonerReleaseCampaignBehavior), "ApplyEscapeChanceToExceededPrisoners");

        public static bool Prefix(PrisonerReleaseCampaignBehavior __instance, MobileParty mobileParty)
        {
            LordKeeperSettings settings = LordKeeperSettings.Instance;
            if (settings == null || mobileParty == null || ApplyEscapeChance == null) return true;

            CaptorScope scope = Captors.Classify(mobileParty.Party);
            float factor = settings.OverLimitEscapeFactor(scope);
            if (factor >= 1f) return true;

            TroopRoster prisoners = mobileParty.PrisonRoster;
            int limit = mobileParty.Party.PrisonerSizeLimit;
            if (mobileParty.MapEvent != null || mobileParty.SiegeEvent != null || prisoners.TotalManCount <= limit) return false;

            int overLimit = prisoners.TotalManCount - limit;
            for (int i = 0; i < overLimit; i++)
            {
                CharacterObject prisoner = PickPrisoner(prisoners);
                if (prisoner == null) continue;

                if (prisoner.IsHero) EscapeGuard.Set(prisoner.HeroObject, scope, factor);
                else if (MBRandom.RandomFloat >= factor) continue;

                try { ApplyEscapeChance.Invoke(__instance, new object[] { prisoner, mobileParty }); }
                finally { EscapeGuard.Clear(); }
            }
            return false;
        }

        // Same pick as vanilla: a random regular if there are any, otherwise a random prisoner.
        private static CharacterObject PickPrisoner(TroopRoster prisoners)
        {
            bool hasRegulars = prisoners.TotalRegulars > 0;
            int index = (int)((hasRegulars ? prisoners.TotalRegulars : prisoners.TotalManCount) * MBRandom.RandomFloat);
            foreach (TroopRosterElement element in prisoners.GetTroopRoster())
            {
                if (element.Character.IsHero && hasRegulars) continue;
                index -= element.Number;
                if (index <= 0) return element.Character;
            }
            return null;
        }
    }

    // Patches ApplyInternal rather than ApplyByEscape: ApplyByEscape is a tiny wrapper that the JIT inlines into
    // its callers, so a patch on it never runs from there.
    [HarmonyPatch(typeof(EndCaptivityAction), "ApplyInternal")]
    public static class EscapePatch
    {
        public static bool Prefix(Hero prisoner, EndCaptivityDetail detail)
        {
            if (detail != EndCaptivityDetail.ReleasedAfterEscape || prisoner == null || prisoner != EscapeGuard.Hero) return true;
            if (MBRandom.RandomFloat < EscapeGuard.Factor) return true;

            if (LordKeeperSettings.Instance?.ShowCaughtEscapes == true && EscapeGuard.Scope != CaptorScope.Kingdom)
            {
                TextObject message = new TextObject("{=lord_keeper_escape_caught}{HERO} tried to escape, but was caught.");
                message.SetTextVariable("HERO", prisoner.Name);
                InformationManager.DisplayMessage(new InformationMessage(message.ToString(), Colors.Green));
            }
            return false;
        }
    }
}
