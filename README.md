# The Last Voyage of the Picasso

A 15-minute single-player VR welding game for Meta Quest 3, made by **Spark-Smith Studio** at the Entertainment Technology Center (ETC), Carnegie Mellon University, Spring 2026.

**Project website:** https://projects.etc.cmu.edu/sparksmith-studio/

You play the captain of the Picasso, a painter whose small ship gets hit by an asteroid. With help from Pablo, the ship's AI, you weld a patch for the hull, repair a cracked fuel cell before it melts down, and then use the leftover scrap to build a sculpture while you wait for rescue.

## Why we made it

Our client builds XR welding training tools. They teach the skill well, but it's hard to get new learners interested in the first place. We wanted to see whether a short, story-driven game could make teenagers and young adults (15–21) with no welding experience curious and confident enough to try the real thing. It's an introduction to welding, not a realistic simulator.

## Download and play

The game itself is on the [Releases](../../releases) page. You don't need Unity to play it.

1. Download `TheLastVoyageOfThePicasso-Quest3.apk` from the latest release.
2. Turn on Developer Mode for your Quest 3 in the Meta Horizon phone app.
3. Install the APK with Meta Quest Developer Hub or SideQuest, or run `adb install TheLastVoyageOfThePicasso-Quest3.apk`.
4. On the headset, find it under Unknown Sources in your library.

It's also on itch.io: https://mtwentiethree.itch.io/the-last-voyage-of-the-picasso

More about the project:
- Project website and weekly dev blog: https://projects.etc.cmu.edu/sparksmith-studio/

## Controls

| Input | Action |
|-------|--------|
| Thumbstick | Move |
| Grip | Grab (scrap, torch, mask, visor, power dial) |
| Index trigger (holding the torch) | Weld |

## Team

- **Yuelin Liu**, Producer
- Zelin Chen
- Alvin Li
- Ethan Wills
- Barry Yuan

Client: Prof. Dina El-Zanfaly, CMU School of Design
Faculty advisors: Derek Ham, Scott Stevens
Community partner: Industrial Arts Workshop, Pittsburgh

## Opening the Unity project

Everything below is for working with the source. To just play, use the release above.

### Requirements

- Unity **6000.2.6f2**
- Meta XR SDK (`com.meta.xr.sdk.all` 83.0.4), URP 17.2, OpenXR, Input System, VFX Graph
- Meta Quest 3 (Android build target)
- Git LFS. Models, textures, video, audio and baked lighting are stored in LFS.

### Getting started

```
git lfs install
git clone <this repo>
```

Open the folder with Unity 6000.2.6f2. The first import takes a while.

Build scenes, in order (File > Build Profiles):

| # | Scene | What it is |
|---|-------|-----------|
| 0 | `Assets/Scenes/SoftMenu.unity` | Start menu, controls screen, captain intro video |
| 1 | `Assets/Scenes/FinalBuild.unity` | The game: tutorial, fuel cell mission, sculpture room |
| 2 | `Assets/Scenes/FinalEnd.unity` | Outro video and credits |

To test in the editor, open `SoftMenu` with Quest Link connected, or open `FinalBuild` directly to skip the intro.

## Code overview

All gameplay code is in `Assets/Scripts`.

- `Draw/Metaball/` welding. `WeldingController` places small spheres at the torch tip while the trigger is held. `MarchingCubes` turns them into the bead mesh on the CPU. `WeldingConnection` joins two scrap pieces into one object when you weld where they touch. `DistanceEvaluation` drives the "Too Close / Too Far / Nice!" feedback.
- `GameManager/` game flow. `PlaytestGameManager` runs the state machine (Tutorial → Mission1 → FreeBuild → End). Each step has its own trigger and submit scripts in the subfolders.
- `WeldMask/` helmet visor, auto-darkening and the arc-flash effect when you weld with the visor up.
- `Sound/SoundManager` handles Pablo's voice lines, SFX, the weld success/fail loops and music.
- `UI/` mission board text, subtitles, controller hints, floating zero-g scrap.

## Credits

Models, textures, music and sound effects were made by the team. Third-party code and tools:

- Marching cubes triangulation tables in `Assets/Scripts/Draw/Metaball/CubeLookup.cs` are from [dwilliamson's gist](https://gist.github.com/dwilliamson/c041e3454a713e58baf6e4f8e5fffecd).
- Meta XR SDK (`com.meta.xr.sdk.all`).
- Unity packages: Universal Render Pipeline, Shader Graph, VFX Graph, Timeline, Input System, OpenXR, TextMesh Pro, AI Navigation.

## Known issues

- The power dial and the old push-button switch both exist in the booth. The dial wins because it writes the power state every frame.
- Some names are leftovers from earlier versions and were kept on purpose (`PlaytestGameManager`, the `Tutoriall` folder, `PerparationVOTrigger`). Renaming a script file outside Unity breaks scene references, so rename them inside the editor if you want to.
