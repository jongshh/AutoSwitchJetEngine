using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoSwitchJetEngine
{
    public class ModuleAutoSwitchJetEngine : PartModule
    {
        // Configuration fields
        [KSPField]
        public float thresholdOn = 0.66f;  // Throttle > 66% -> Switch to Wet (Afterburner)

        [KSPField]
        public float thresholdOff = 0.64f; // Throttle < 64% -> Switch to Dry (Normal)

        private MultiModeEngine multiModeEngine;

        public override void OnStart(StartState state)
        {
            // Find the MultiModeEngine module on this part
            multiModeEngine = this.part.FindModuleImplementing<MultiModeEngine>();

            if (multiModeEngine == null)
            {
                // Error handling
                Debug.LogError("[AutoSwitchJetEngine] Error: MultiModeEngine not found on part " + part.partInfo.title);
            }
            else
            {
                // Success logging
                Debug.Log("[AutoSwitchJetEngine] Module loaded successfully on " + part.partInfo.title);
            }
        }

        public override void OnFixedUpdate()
        {
            // Safety Check
            if (multiModeEngine == null || this.vessel == null) return;

            // Get current main throttle (0.0 to 1.0)
            float currentThrottle = this.vessel.ctrlState.mainThrottle;

            // [Condition 1] Activate Afterburner
            // If throttle > 66% AND currently in Dry (Primary) mode
            if (currentThrottle > thresholdOn && multiModeEngine.runningPrimary)
            {
                // Check if engine is actually ignited to avoid log spam
                if (IsEngineIgnited())
                {
                    Debug.Log($"[AutoSwitchJetEngine] Activating Afterburner! (Throttle: {currentThrottle:F2})");
                    multiModeEngine.ToggleMode(); // Switch mode
                }
            }
            // [Condition 2] Deactivate Afterburner
            // If throttle < 64% AND currently in Wet (Secondary) mode
            else if (currentThrottle < thresholdOff && !multiModeEngine.runningPrimary)
            {
                if (IsEngineIgnited())
                {
                    Debug.Log($"[AutoSwitchJetEngine] Deactivating Afterburner... (Throttle: {currentThrottle:F2})");
                    multiModeEngine.ToggleMode(); // Switch mode
                }
            }
        }

        // Helper: Check if the ACTIVE engine is ignited and uses IntakeAir
        private bool IsEngineIgnited()
        {
            var engines = this.part.FindModulesImplementing<ModuleEngines>();
            foreach (var eng in engines)
            {
                // Check if this engine module matches the current MultiMode state
                if (eng.EngineIgnited && eng.engineID == multiModeEngine.mode)
                {
                    // Ensure it consumes IntakeAir (prevents switching RAPIER in Rocket mode)
                    return CheckIfUsesIntakeAir(eng);
                }
            }
            return false;
        }

        // Helper: Verify the engine consumes oxygen (IntakeAir)
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