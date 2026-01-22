using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoSwitchJetEngine
{
    public class ModuleAutoSwitchJetEngine : PartModule
    {
        // 1. [On/Off Switch] Allows toggling this feature per vessel.
        // isPersistant = true: Saves this setting in the .craft file (Acts as a profile setting).
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Auto Afterburner")]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isAutoSwitchEnabled = true;

        // 2. [Slider] Allows the user to manually set the afterburner activation threshold.
        // Default is 0.66 (66%). This value is also saved per craft.
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "AB Threshold")]
        [UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, stepIncrement = 0.01f, scene = UI_Scene.All)]
        public float thresholdOn = 0.66f;

        // The deactivation threshold is automatically calculated as 2% lower than the activation threshold.
        // This creates a hysteresis loop to prevent rapid toggling at the boundary.
        private float thresholdOff => Mathf.Clamp01(thresholdOn - 0.02f);

        private MultiModeEngine multiModeEngine;

        public override void OnStart(StartState state)
        {
            // Retrieve the MultiModeEngine module from the part
            multiModeEngine = this.part.FindModuleImplementing<MultiModeEngine>();
        }

        public override void OnFixedUpdate()
        {
            // Safety Checks: Ensure the module and vessel exist
            if (multiModeEngine == null || this.vessel == null) return;

            // [Check 1] If the user disabled the feature, do nothing.
            if (!isAutoSwitchEnabled) return;

            // Get the current main throttle input (0.0 to 1.0)
            float currentThrottle = this.vessel.ctrlState.mainThrottle;

            // [Logic 1] Activate Afterburner
            // Condition: Throttle > User Threshold AND currently in Dry (Primary) mode
            if (currentThrottle > thresholdOn && multiModeEngine.runningPrimary)
            {
                // Ensure the engine is actually running to avoid switching when off
                if (IsEngineIgnited())
                {
                    multiModeEngine.ToggleMode(); // Switch to Wet mode
                }
            }
            // [Logic 2] Deactivate Afterburner
            // Condition: Throttle < Calculated Off-Threshold AND currently in Wet (Secondary) mode
            else if (currentThrottle < thresholdOff && !multiModeEngine.runningPrimary)
            {
                if (IsEngineIgnited())
                {
                    multiModeEngine.ToggleMode(); // Switch to Dry mode
                }
            }
        }

        // Helper: Checks if the currently active engine mode is ignited
        private bool IsEngineIgnited()
        {
            var engines = this.part.FindModulesImplementing<ModuleEngines>();
            foreach (var eng in engines)
            {
                // Check if the engine module matches the current MultiMode state ID
                if (eng.EngineIgnited && eng.engineID == multiModeEngine.mode)
                {
                    // Additional Check: Ensure it uses IntakeAir (prevents issues with RAPIER in Rocket mode)
                    return CheckIfUsesIntakeAir(eng);
                }
            }
            return false;
        }

        // Helper: Verifies that the engine consumes IntakeAir (Oxygen)
        private bool CheckIfUsesIntakeAir(ModuleEngines engine)
        {
            if (engine.propellants == null) return false;
            foreach (Propellant p in engine.propellants)
            {
                if (p.name == "IntakeAir") return true;
            }
            return false;
        }
    }
}