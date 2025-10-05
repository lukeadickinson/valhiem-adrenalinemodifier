using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;


namespace AdrenalineModifier
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        const string pluginGUID = "mightywa33ior.valheim.adrenalinemodifier";
        const string pluginName = "AdrenalineModifier";
        const string pluginVersion = "1.0.0";

        private readonly Harmony HarmonyInstance = new Harmony(pluginGUID);

        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        //private static string ConfigFileName = pluginGUID + ".cfg";
        //private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private static ConfigEntry<float> AdrenalineMultiplier = null;
        private static ConfigEntry<float> AdrenalineDecayMultiplier = null;
        private static ConfigEntry<float> AdrenalineDecayDelayMultiplier = null;
        public static float GetAdrenalineMultiplier() => AdrenalineMultiplier.Value;
        public static float GetAdrenalineDecayMultiplier() => AdrenalineDecayMultiplier.Value;
        public static float GetAdrenalineDecayDelayMultiplier() => AdrenalineDecayDelayMultiplier.Value;

        public void Awake()
        {
            //logger.LogInfo("Thank you for using my mod!");

            Config.SaveOnConfigSet = false;   // Disable saving when binding each following config
            //// Binds the configuration, the passed variable will always reflect the current value set
            AdrenalineMultiplier = Config.Bind("General", "AdrenalineMultiplier", 1.0f, "Multiplier to gained adrenaline");
            AdrenalineDecayMultiplier = Config.Bind("General", "AdrenalineDecayMultiplier", 1.0f, "Multiplier to lost adrenaline");
            AdrenalineDecayDelayMultiplier = Config.Bind("General", "AdrenalineDecayDelayMultiplier", 1.0f, "Multiplier to delay before adrenaline decay starts");

            Config.Save();   // Save only once
            Config.SaveOnConfigSet = true;   // Re-enable saving on config changes

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }
        private void OnDestroy()
        {
            Config.Save();
        }

    [HarmonyLib.HarmonyPatch(typeof(Player), nameof(Player.AddAdrenaline))]
        public static class Patch_Player_AddAdrenaline
        {

            private static void Prefix(Player __instance, ref float v)
            {
                //logger.LogInfo("Adrenaline Prefix:" + v);
                if (v > 0f)
                {
                    v *= GetAdrenalineMultiplier();
                    //logger.LogInfo("Adrenaline add modified:" + v);
                }
                else
                {
                    //logger.LogInfo("Adrenaline decay modifier:" + GetAdrenalineDecayMultiplier());
                    v *= GetAdrenalineDecayMultiplier();
                    //logger.LogInfo("Adrenaline decay modified:" + v);

                }
            }

            public static void Postfix(Player __instance, ref float ___m_adrenalineDegenTimer, ref float v)
            {
                //logger.LogInfo("Adrenaline Postfix:" + ___m_adrenalineDegenTimer);

                float maxAdrenaline = __instance.GetMaxAdrenaline();
                if (v > 0f && maxAdrenaline > 0f)
                {
                    //logger.LogInfo("Adrenaline decay delay modifier:" + GetAdrenalineDecayDelayMultiplier());

                    ___m_adrenalineDegenTimer *= GetAdrenalineDecayDelayMultiplier();
                    //logger.LogInfo("Adrenaline decay timer modified" + ___m_adrenalineDegenTimer);

                }
            }
        }
    }
}