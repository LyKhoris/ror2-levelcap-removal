using System.Reflection;
using BepInEx;
using RoR2;

namespace LevelCapRemover
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public sealed class LevelCapRemoverPlugin : BaseUnityPlugin
    {
        // Run converts the int cap to float before FloorToInt. int.MaxValue
        // rounds UP to 2^31 there; this is the last float below that boundary.
        private const int MaxAmbientLevel = 2_147_483_520;

        private void Awake()
        {
            // Historical reference: Moffein/RaiseMonsterLevelCap (1.0.8) set
            // this field and patched the calculation. RoR2 1.5.0 reads the
            // field directly, so changing it alone keeps vanilla scaling.
            var cap = typeof(Run).GetField("ambientLevelCap",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (cap == null || cap.FieldType != typeof(int) || cap.IsInitOnly || cap.IsLiteral)
            {
                Logger.LogError("Could not find a writable Run.ambientLevelCap (int). Check for a game update.");
                return;
            }

            var previous = (int)cap.GetValue(null);
            if (previous != 99)
            {
                Logger.LogWarning($"Expected the vanilla cap of 99, found {previous}. Leaving it unchanged.");
                return;
            }

            cap.SetValue(null, MaxAmbientLevel);
            Logger.LogInfo("Removed the level-99 ambient cap.");
        }
    }
}
