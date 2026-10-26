using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace LordKeeper
{
    public class LordKeeperSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            // Pass the assembly explicitly: the parameterless PatchAll() finds it from the call stack, which can miss our assembly.
            new Harmony("lordkeeper.bannerlord").PatchAll(typeof(LordKeeperSubModule).Assembly);
        }
    }
}
