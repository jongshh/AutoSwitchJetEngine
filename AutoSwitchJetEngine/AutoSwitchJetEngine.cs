using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // For Unity base features (GUI, Input, etc.)
using KSP.IO;      // For File I/O

namespace AutoSwitchJetEngine
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AutoSwitchMaster : MonoBehaviour
    {
        public static AutoSwitchMaster Instance;

        // GUI Window Variables
        private Rect _windowRect = new Rect(100, 100, 300, 250);
        private bool _isGuiVisible = false;
        private const int WINDOW_ID = 982101;

        // Data Class for Vessel Profiles
        public class VesselProfile
        {
            public bool isEnabled = true;
            public float threshold = 0.70f; // Default 70%
        }

        private Dictionary<string, VesselProfile> _profiles = new Dictionary<string, VesselProfile>();
        private string _configPath;

        private void Awake()
        {
            Instance = this;
            _configPath = KSPUtil.ApplicationRootPath + "GameData/AutoSwitchJetEngine/PluginData/settings.cfg";
            LoadSettings();
        }

        private void Update()
        {
            // Explicitly use UnityEngine.Input to avoid conflict with KSP.IO.Input
            if (GameSettings.MODIFIER_KEY.GetKey() && UnityEngine.Input.GetKeyDown(KeyCode.J))
            {
                _isGuiVisible = !_isGuiVisible;
            }
        }

        private void OnGUI()
        {
            if (_isGuiVisible && HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null)
            {
                // Explicitly use UnityEngine.GUI
                _windowRect = UnityEngine.GUI.Window(WINDOW_ID, _windowRect, DrawWindow, "AutoSwitch Jet Engine");
            }
        }

        private void DrawWindow(int windowID)
        {
            Vessel activeVessel = FlightGlobals.ActiveVessel;
            if (activeVessel == null) return;

            string vName = activeVessel.vesselName;
            VesselProfile profile = GetProfile(vName);

            // Explicitly use UnityEngine.GUILayout
            UnityEngine.GUILayout.BeginVertical();

            UnityEngine.GUILayout.Label($"Vessel: {vName}");

            // Enable/Disable Toggle
            bool newEnabled = UnityEngine.GUILayout.Toggle(profile.isEnabled, "Auto Switch Enabled");
            if (newEnabled != profile.isEnabled)
            {
                profile.isEnabled = newEnabled;
            }

            UnityEngine.GUILayout.Space(10);
            UnityEngine.GUILayout.Label($"Activation Threshold: {profile.threshold:P0}");

            // Threshold Slider
            float newThreshold = UnityEngine.GUILayout.HorizontalSlider(profile.threshold, 0f, 1f);

            if (Mathf.Abs(newThreshold - profile.threshold) > 0.001f)
            {
                profile.threshold = newThreshold;
            }

            UnityEngine.GUILayout.Space(20);

            // Save Button
            if (UnityEngine.GUILayout.Button("Save Profile"))
            {
                SaveSettings();
            }

            UnityEngine.GUILayout.EndVertical();
            UnityEngine.GUI.DragWindow();
        }

        // --- Helper Methods ---

        public VesselProfile GetProfile(string vesselName)
        {
            if (!_profiles.ContainsKey(vesselName))
            {
                _profiles[vesselName] = new VesselProfile();
            }
            return _profiles[vesselName];
        }

        private void LoadSettings()
        {
            ConfigNode node = ConfigNode.Load(_configPath);
            if (node == null) return;

            foreach (ConfigNode vesselNode in node.GetNodes("VESSEL_PROFILE"))
            {
                string name = vesselNode.GetValue("name");
                if (string.IsNullOrEmpty(name)) continue;

                VesselProfile p = new VesselProfile();
                bool.TryParse(vesselNode.GetValue("isEnabled"), out p.isEnabled);
                float.TryParse(vesselNode.GetValue("threshold"), out p.threshold);

                _profiles[name] = p;
            }
        }

        private void SaveSettings()
        {
            ConfigNode root = new ConfigNode();
            foreach (var kvp in _profiles)
            {
                ConfigNode vesselNode = new ConfigNode("VESSEL_PROFILE");
                vesselNode.AddValue("name", kvp.Key);
                vesselNode.AddValue("isEnabled", kvp.Value.isEnabled);
                vesselNode.AddValue("threshold", kvp.Value.threshold);
                root.AddNode(vesselNode);
            }

            string dir = System.IO.Path.GetDirectoryName(_configPath);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            root.Save(_configPath);
        }
    }

    public class ModuleAutoSwitchJetEngine : PartModule
    {
        private MultiModeEngine _multiModeEngine;
        private List<ModuleEngines> _engines;
        private const float BUFFER = 0.02f; // 2% Buffer zone

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            _multiModeEngine = part.FindModuleImplementing<MultiModeEngine>();
            _engines = part.FindModulesImplementing<ModuleEngines>();
        }

        public override void OnFixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || this.vessel == null) return;

            // Only run logic if this is the currently active vessel
            if (this.vessel != FlightGlobals.ActiveVessel) return;

            if (AutoSwitchMaster.Instance == null) return;
            var profile = AutoSwitchMaster.Instance.GetProfile(this.vessel.vesselName);

            if (!profile.isEnabled) return;

            CheckAndSwitch(profile.threshold);
        }
        private void CheckAndSwitch(float threshold)
        {
            if (_multiModeEngine == null || _engines == null) return;

            float currentThrottle = this.vessel.ctrlState.mainThrottle;
            string currentModeName = _multiModeEngine.mode;

            ModuleEngines activeEngine = _engines.FirstOrDefault(e => e.engineID == currentModeName);
            if (activeEngine == null) return;

            bool isEngineOn = activeEngine.EngineIgnited || activeEngine.finalThrust > 0.0f;

            if (!isEngineOn) return;

            // Ensure we are using IntakeAir (prevent switching in ClosedCycle/Oxidizer mode)
            bool isAirBreathing = activeEngine.propellants.Any(p => p.name == "IntakeAir");
            if (!isAirBreathing) return;

            bool isSecondary = (_multiModeEngine.mode == _multiModeEngine.secondaryEngineID);

            // Logic: Switch based on throttle threshold + buffer
            if (currentThrottle > (threshold + BUFFER) && !isSecondary)
            {
                SwitchMode();
            }
            else if (currentThrottle < (threshold - BUFFER) && isSecondary)
            {
                SwitchMode();
            }
        }

        private void SwitchMode()
        {
            // Iterate through events to find the mode switch event
            foreach (var eventData in _multiModeEngine.Events)
            {
                // Event name is case-sensitive and typically 'ModeEvent'
                if (eventData.name.StartsWith("ModeEvent"))
                {
                    // FIX: Must use BaseEventDetails with Sender.USER for proper event invocation
                    eventData.Invoke(new BaseEventDetails(BaseEventDetails.Sender.USER));
                    return;
                }
            }

            // Fallback: Manually toggle state if event invocation fails
            _multiModeEngine.runningPrimary = !_multiModeEngine.runningPrimary;
        }
    }
}