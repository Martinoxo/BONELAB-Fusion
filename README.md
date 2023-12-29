# BONELAB TideFusion Rewrite
A fork of BONELAB Fusion, utilizing P2P networking in order to support Quest users without a PC.
[You can view a basic installation guide here.](INSTALLATION.md)

#### THIS IS A REWRITE OF THE ORIGIONAL TIDEFUSION PROJECT, NOT ALL THINGS ARE PRESENT IN THIS REPOSITORY

## Networking
This fork introduces a new networking layer option titled "Riptide", which is based on the P2P transport model. This networking solution allows devices to connect directly to eachother, meaning Quest users do not need any external device to connect to other players (no Fusion Helper), and that the latency between player to player is much smaller. However, there is the quirk that the host of a server needs to "Port Forward", which allows external connections to enter the network, but it also sort of a safety hazard. More info on Port Forwarding is contained [here](https://cybernews.com/what-is-vpn/port-forwarding/).
#### VOICE CHAT HAS NOT YET BEEN IMPLEMENTED IN THE RIPTIDE LAYER FOR QUEST _OR_ PC.

## Credits
- Lakatrazz: Created the base Fusion mod, without that of course TideFusion would not exist.
- All other credits are on the base Fusion repository

## Licensing
- The source code of [Riptide Networking](https://github.com/RiptideNetworking/Riptide) is used under the MIT License. The full license can be found [here](https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md).
- The source of [BONELAB Fusion](https://github.com/Lakatrazz/BONELAB-Fusion) is used under the MIT Liscense. The full license can be found [here](https://github.com/Lakatrazz/BONELAB-Fusion/blob/main/LICENSE).

## Setting up the Source Code
1. Clone the git repo into a folder
2. Setup a "managed" folder in the "Core" folder.
3. Drag the dlls from Melonloader/Managed into the managed folder.
4. Drag MelonLoader.dll and 0Harmony.dll into the managed folder.
5. Place the RiptideNetworking.dll and netstandard.dll from the plugins folder included in the release into the managed folder.
6. You're done!

## Disclaimer

#### THIS PROJECT IS NOT AFFILIATED WITH SLZ, LAKATRAZZ, OR ANY OTHER MULTIPLAYER MOD DEVELOPMENT TEAMS.
