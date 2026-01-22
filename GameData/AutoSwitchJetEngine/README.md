# AutoSwitchJetEngine

I have a HOTAS but I have to press button everytime
AJE is too heavy (or not support Tweakscale)

Try this simple mod!

<img width="2560" height="1440" alt="screenshot7461" src="https://github.com/user-attachments/assets/a2d76d4c-6fba-478c-b131-7e9b68226d99" />

**AutoSwitchJetEngine** is a lightweight plugin for Kerbal Space Program that automatically toggles multi-mode jet engines (like the J-404 Panther) between Dry and Wet (Afterburner) modes based on your throttle input.

It simulates the **"Afterburner Detent"** found in real-world fighter jets, allowing you to control engine modes purely with your throttle lever, without needing action groups or right-click menus.

## ✨ Features

* **Automatic Mode Switching:**
    * Throttle **above** threshold (default 66%): Switches to **Wet (Afterburner)** mode.
    * Throttle **below** threshold: Switches back to **Dry (Standard)** mode.
* **Smart Hysteresis:** Includes a 2% buffer zone to prevent the engine from flickering rapidly between modes when the throttle is near the threshold.
* **Per-Vessel Configuration:**
    * **Toggle Switch:** Enable or disable the automation for specific engines/crafts.
    * **Adjustable Threshold:** Use the in-game slider to set your preferred afterburner kick-in point (0% ~ 100%).
    * Settings are saved automatically in your `.craft` file or save game.
* **Safety Checks:**
    * **Ignition Check:** Only operates when the engine is actually running.
* **Mod Compatibility:** Works with any modded engine that uses the stock `MultiModeEngine` module (e.g., B9 Aerospace, Airplane Plus) via ModuleManager.

## 📦 Installation

1.  Download the latest release.
2.  Ensure you have **[ModuleManager](https://forum.kerbalspaceprogram.com/topic/50533-module-manager/)** installed.
3.  Copy the `AutoSwitchJetEngine` folder into your KSP `GameData` directory.

## 🎮 Usage

1.  Build a plane with a multi-mode engine (e.g., J-404 Panther).
2.  Right-click the engine in the Editor (SPH/VAB) or in Flight to see the settings:
    * **Auto Afterburner:** Click to Enable/Disable.
    * **AB Threshold:** Slide to change when the afterburner kicks in (Default: 0.66).
3.  Launch and fly! Push the throttle past the threshold to feel the kick.

## ⚙️ Configuration (Advanced)

This mod uses `ModuleManager` to apply the functionality to all relevant engines. You can customize the default behavior by editing `GameData/AutoSwitchJetEngine/AutoSwitchJetEngine.cfg`.

```cfg
// Example: Change default threshold to 90%
@PART[*]:HAS[@MODULE[MultiModeEngine],@MODULE[ModuleEngines*]:HAS[@PROPELLANT[IntakeAir]],!MODULE[ModuleAutoSwitchJetEngine]]:FINAL
{
    MODULE
    {
        name = ModuleAutoSwitchJetEngine
        thresholdOn = 0.90   // Set default to 90%
        // thresholdOff is automatically calculated as (thresholdOn - 0.02)
    }
}
