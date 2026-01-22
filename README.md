# AutoSwitchJetEngine

I have a HOTAS but I have to press a button every time to toggle modes?
AJE is too heavy?

**Try this simple mod!**

<img width="2560" height="1440" alt="screenshot7461" src="https://github.com/user-attachments/assets/a2d76d4c-6fba-478c-b131-7e9b68226d99" />

**AutoSwitchJetEngine** is a lightweight plugin for Kerbal Space Program that automatically toggles multi-mode jet engines (like the J-404 Panther) between Dry and Wet (Afterburner) modes based on your throttle input.

It simulates the **"Afterburner Detent"** found in real-world fighter jets, allowing you to control engine modes purely with your throttle lever.

## ✨ Features

* **Automatic Mode Switching:**
    * Throttle **above** threshold: Switches to **Wet (Afterburner)** mode.
    * Throttle **below** threshold: Switches back to **Dry (Standard)** mode.
* **Smart Hysteresis:** Includes a 2% buffer zone to prevent the engine from flickering rapidly between modes when the throttle is near the threshold.
* **In-Flight GUI Control:**
    * Simple window to adjust settings on the fly.
    * **Toggle Switch:** Enable or disable the automation for the current vessel.
    * **Adjustable Threshold:** Use the slider to set your preferred afterburner kick-in point (0% ~ 100%).
* **Profile System:**
    * Settings are saved automatically per **Vessel Name** in an external configuration file (`PluginData/settings.cfg`).
    * Your preferences persist across launches without modifying craft files.
* **Mod Compatibility:** Works with any modded engine that uses the stock `MultiModeEngine` module (e.g., B9 Aerospace, Airplane Plus) via ModuleManager.

## 📦 Installation

1.  Download the latest release.
2.  Ensure you have **[ModuleManager](https://forum.kerbalspaceprogram.com/topic/50533-module-manager/)** installed.
3.  Copy the `AutoSwitchJetEngine` folder into your KSP `GameData` directory.

## 🎮 Usage

1.  Launch a plane with a multi-mode engine (e.g., J-404 Panther).
2.  In Flight, press **`Mod + J`** (usually **Alt + J** on Windows) to open the settings window.
3.  Adjust the **Activation Threshold** slider (Default: 70%).
4.  Push the throttle past the threshold to feel the kick!

## ⚠️ Known Issues
* **Staging Requirement:** Engines must be activated via **Staging** (Spacebar) for the automation to detect them reliably. Engines activated solely via Action Groups might not trigger the switch immediately.
