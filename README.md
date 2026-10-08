# Vault Voice: Blackout Heist

A two-player asymmetric co-located VR heist. The **Infiltrator** wears a Meta Quest 3 headset and moves through a dark bank vault, solving a keypad and a laser puzzle. The **Handler** sits next to them with a laptop that shows the vault blueprint, the codes and the current objective. Neither player can finish alone, so they have to talk.

## Students

- Michaela Koso
- Tal Shkuri

Reichman University

## Required platform and hardware

- Meta Quest 3 (Quest 2 / Quest Pro should also work), with both Touch controllers
- Developer Mode enabled on the headset (needed to install an APK)
- A PC or Mac with a USB-C cable to install the APK
- A laptop for the Handler with any modern browser (Chrome recommended). No install and no network connection between the two devices are needed.

## Installation and launch

**Option A: Meta Quest Developer Hub (easiest)**
1. Connect the headset to the computer with USB-C and allow USB debugging in the headset.
2. Open Meta Quest Developer Hub and drag `VaultVoice.apk` onto the device panel.

**Option B: SideQuest or adb**
```
adb install -r VaultVoice.apk
```

**Handler:** open `Handler/vault-overwatch.html` in the browser and press F11 for full screen. An internet connection loads the interface fonts; without it the page still works with fallback fonts.

**Launch the VR app:** in the headset open App Library, choose **Unknown Sources**, then **T&M-seminar-project**.

**Opening the source project:** open the folder in Unity Hub with Unity **6000.3.15f1**. The first open takes a few minutes while Unity rebuilds the `Library` folder. The main scene is `Assets/Scenes/REALSCENE.unity`. To build, switch the platform to Android in Build Profiles and build.

## Engine version

Unity 6 (6000.3.15f1), Universal Render Pipeline

## Packages and plugins

Installed automatically through `Packages/manifest.json`:

- Meta XR SDK All-in-One 201.0.0
- XR Interaction Toolkit 3.5.0
- OpenXR 1.17.0 and Android XR OpenXR 1.3.1
- Input System 1.19.0
- Universal RP 17.3.0
- AI Navigation 2.0.12, Timeline 1.8.12

Also needed: Android Build Support module in Unity Hub (with OpenJDK and Android SDK/NDK).

## How to play

**Infiltrator (headset)**
- Movement uses the Meta XR SDK locomotion: left thumbstick to move, right thumbstick to turn
- Hands / controllers: press keypad buttons and laser puzzle buttons by touching them
- Restart button in the scene: reloads the game from the start

**Handler (laptop, Vault Voice // Overwatch)**
- Click **Laser disarming** to open five `light_code` files. Each one shows a 5x5 light pattern; only one matches the laser puzzle the Infiltrator is facing, so they have to compare notes.
- Click **Vault code** and then `num_comb` to open a letter grid. Click letters to black them out while working out the code with the Infiltrator.
- Windows can be dragged by their title bar. Esc closes the top window.

**Goal:** the Infiltrator describes what they see, the Handler guides them with the files on the Overwatch screen, and together they get through the laser puzzle and open the vault keypad.

In the study, the researcher reads a countdown aloud (13 or 9 minutes, or no limit). There is no timer on screen.

## Known issues and limitations

- No on-screen timer; the countdown is read aloud by the researcher.
- No event logging; task outcomes are recorded by hand.
- The two devices are not connected. The Handler page is static and does not react to what happens in VR.
- The comms log on the Handler screen is decorative (scripted messages), and the map panel is intentionally offline ("SIGNAL LOST").
- Player camera height follows the real head position. The game is designed to be played seated, so if the Infiltrator stands or sits on a high chair, the view can end up too high. Play seated on a regular chair.
