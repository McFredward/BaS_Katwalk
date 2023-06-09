# BaS_Katwalk
A KatWalk SDK integration mod into Blade&Sorcery **for U12**

<p align="center">
  <img src="BaS_Katwalk_logo.png" width="50%" height="50%">
</p>


## Features:

* Uses official KatWalk SDK
* Better speed integration based on the real-life speed on the KatWalk
* Modifikations with the in-game mod menu for personal preferences

## How To Install & Use

* Download the zip at [mod.io](https://mod.io/g/blade-and-sorcery/m/katwalk-sdk-integration) and unzip the folder into `Blade & Sorcery\BladeAndSorcery_Data\StreamingAssets\Mods` or directly install it using the in-game mod manager.
* Start the KatWalk Gateway
* **Turn the Walk- and Run-Source to None in the Gateway for B&S**, since the Gateway directly communicates with the game. There is no need to simulate the Joystick anymore and it could lead to unintended behavior.
* Start the game

## Modification values

* **Speed Multiplicator**: Modifies how the speed of the KatWalk is translated to the in-game speed.
* **Speed max. Range**: Modifies what maximal ingame speed value corresponds to the maximal KatWalk speed.
* **Speed curve exponent**: Modifies the speed curve which determines how fast the maximal speed is reached. 

## Known issues

* Backwards walking is not working like with the Joystick emulation (seems like the SDK dont support it right now)

## Credits & Libs

* [Official KatWalk SDK](https://drive.google.com/drive/folders/1K_0q1YWth80dl7g8LF57xoSN1YndUvM-)
* [HarmonyLib](https://github.com/pardeike/Harmony) using the MIT License
* [MelonLoader](https://github.com/LavaGang/MelonLoader) using the Apache-2.0 license

If you have any suggestions, feel free to open an Issue. Or contact me (McFredward) in the [KatWalk Discord](https://discord.gg/kat-vr-community-785305088465567824).

