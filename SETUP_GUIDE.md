# DeadDays: 8-bit Isometric Zombie Survival - Setup Guide

**Complete Unity Project Setup Instructions**

---

## 📋 TABLE OF CONTENTS

1. [Prerequisites](#prerequisites)
2. [Unity Project Setup](#unity-project-setup)
3. [Package Installation](#package-installation)
4. [Project Configuration](#project-configuration)
5. [Scene Setup](#scene-setup)
6. [Prefab Creation](#prefab-creation)
7. [Script Integration](#script-integration)
8. [Asset Import](#asset-import)
9. [Testing](#testing)
10. [ML-Agents Setup (Optional)](#ml-agents-setup)
11. [Python Environment (Optional)](#python-environment)
12. [Troubleshooting](#troubleshooting)

---

## 1. PREREQUISITES

### Required Software
- **Unity Hub** (latest version)
- **Unity 2021.3 LTS or newer** (2D template)
- **Visual Studio 2022** or **Visual Studio Code** (with C# extension)
- **Git** (for version control)
- **Python 3.8-3.10** (for ML-Agents, optional)

### Recommended Specs
- OS: Windows 10/11, macOS 10.15+, or Linux
- RAM: 8GB minimum, 16GB recommended
- Storage: 5GB free space
- GPU: Any modern GPU with 2GB+ VRAM

---

## 2. UNITY PROJECT SETUP

### Step 1: Create New Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **2D (URP)** template or **2D Core** template
4. Set Project Name: `DeadDays`
5. Set Location: `D:\Github\DeadDays` (or your preferred path)
6. Click **"Create Project"**

### Step 2: Import Existing Scripts

Your scripts are already in `Assets/Scripts/`. Unity will automatically compile them on first import.

**Wait for compilation to complete** (check bottom-right corner of Unity Editor).

### Step 3: Fix Compilation Errors (If Any)

Some scripts reference Unity packages that may not be installed yet. Continue to Step 3 (Package Installation) to resolve.

---

## 3. PACKAGE INSTALLATION

### Required Packages

Open **Window > Package Manager** and install:

#### A. 2D Sprite (Usually Pre-installed)
- **Package:** com.unity.2d.sprite
- **Version:** Latest
- **Purpose:** Sprite rendering and management

#### B. 2D Tilemap Editor
- **Package:** com.unity.2d.tilemap
- **Version:** Latest
- **Purpose:** Isometric tilemap creation

#### C. Universal Render Pipeline (URP) - Optional but Recommended
- **Package:** com.unity.render-pipelines.universal
- **Version:** 12.x or newer
- **Purpose:** 2D lighting system for day/night cycle

#### D. TextMeshPro
- **Package:** com.unity.textmeshpro
- **Version:** Latest
- **Purpose:** High-quality UI text

**Installation Steps:**
1. Open **Window > Package Manager**
2. Click **"+"** button (top-left)
3. Select **"Add package by name..."**
4. Enter package name (e.g., `com.unity.2d.tilemap`)
5. Click **"Add"**
6. Repeat for all packages

### Optional Packages (For ML-Agents)

#### E. ML-Agents (Week 7+ of Roadmap)
- **Package:** com.unity.ml-agents
- **Version:** 2.3.0 or newer
- **Purpose:** AI training integration

**Installation:**
1. Open **Window > Package Manager**
2. Click **"+"** > **"Add package from git URL..."**
3. Enter: `https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents`
4. Click **"Add"**

**Note:** ML-Agents requires additional Python setup (see Section 10).

---

## 4. PROJECT CONFIGURATION

### Step 1: Configure Project Settings

#### Physics 2D Settings
1. Go to **Edit > Project Settings > Physics 2D**
2. Set **Gravity Y:** 0 (top-down game, no gravity)
3. Configure **Layer Collision Matrix:**
   - Uncheck: Player vs Player
   - Uncheck: Enemy vs Enemy
   - Check: Player vs Enemy
   - Check: Player vs Obstacle
   - Check: Enemy vs Obstacle

#### Tags and Layers
1. Go to **Edit > Project Settings > Tags and Layers**
2. Add **Tags:**
   - `Player`
   - `Zombie`
   - `Wall`
   - `Door`
   - `Container`
   - `Item`
   - `Obstacle`
   - `Interactable`

3. Add **Layers:**
   - Layer 6: `Player`
   - Layer 7: `Enemy`
   - Layer 8: `Obstacle`
   - Layer 9: `Interactable`
   - Layer 10: `Projectile`

#### Input Manager (Optional - For Custom Controls)
1. Go to **Edit > Project Settings > Input Manager**
2. Verify axes exist: `Horizontal`, `Vertical`, `Mouse ScrollWheel`
3. Add custom inputs if needed (already uses default Unity inputs)

### Step 2: Configure Quality Settings

1. Go to **Edit > Project Settings > Quality**
2. Set **VSync Count:** Every V Blank (for smooth 60 FPS)
3. Set **Anti Aliasing:** 2x or 4x Multi Sampling

### Step 3: Configure Audio Settings

1. Go to **Edit > Project Settings > Audio**
2. Set **System Sample Rate:** 48000 Hz
3. Set **DSP Buffer Size:** Best latency

---

## 5. SCENE SETUP

### Step 1: Create Scenes

1. In **Project** window, navigate to `Assets/Scenes/`
2. Right-click > **Create > Scene**
3. Create 3 scenes:
   - `MainMenu.unity`
   - `Game.unity`
   - `Training.unity` (for ML-Agents)

### Step 2: Set Up Main Menu Scene

1. Open `MainMenu.unity`
2. Create UI:
   - Right-click in Hierarchy > **UI > Canvas**
   - Add **UI > Button - TextMeshPro** (named "StartButton")
   - Add **UI > Button - TextMeshPro** (named "LoadButton")
   - Add **UI > Button - TextMeshPro** (named "QuitButton")
   - Add **UI > Text - TextMeshPro** (title: "DeadDays")
3. Position buttons in center of screen
4. We'll connect these to GameManager later

### Step 3: Set Up Game Scene (Critical)

1. Open `Game.unity`
2. **Create Core Managers:**
   - Right-click in Hierarchy > **Create Empty** (name: `GameManager`)
   - Create Empty (name: `TimeManager`)
   - Create Empty (name: `SaveManager`)
   - Create Empty (name: `AudioManager`)
   - Create Empty (name: `SkillSystem`)

3. **Attach Scripts:**
   - Select `GameManager` > Add Component > `GameManager.cs`
   - Select `TimeManager` > Add Component > `TimeManager.cs`
   - Select `SaveManager` > Add Component > `SaveManager.cs`
   - Select `AudioManager` > Add Component > `AudioManager.cs`
   - Select `SkillSystem` > Add Component > `SkillSystem.cs`

4. **Create Lighting (For Day/Night Cycle):**
   - Right-click in Hierarchy > **Light > Global Light 2D** (URP)
   - Set **Intensity:** 1.0
   - Set **Color:** White (RGB: 255, 255, 255)
   - Drag this light to **TimeManager's `globalLight` field** in Inspector

5. **Create Main Camera:**
   - Should already exist; if not, create one
   - Add Component > **Pixel Perfect Camera** (optional, for crisp pixels)
   - Set **Size:** 10 (adjust for zoom level)
   - Set **Background:** Black

6. **Create Tilemap (Temporary - will be procedural later):**
   - Right-click in Hierarchy > **2D Object > Tilemap > Rectangular**
   - This creates a Grid > Tilemap structure
   - You can paint tiles here for testing (optional)

7. **Connect Manager References:**
   - Select `GameManager`
   - In Inspector, drag `TimeManager` to `timeManager` field
   - Drag `SaveManager` to `saveManager` field
   - Drag `AudioManager` to `audioManager` field

### Step 4: Build Settings

1. Go to **File > Build Settings**
2. Click **"Add Open Scenes"** to add `Game.unity`
3. Drag `MainMenu.unity` to top (index 0)
4. Ensure `Game.unity` is index 1
5. Set **Target Platform:** PC, Mac & Linux Standalone
6. Close (don't build yet)

---

## 6. PREFAB CREATION

### Step 1: Create Player Prefab

1. In Hierarchy (in `Game.unity` scene), right-click > **2D Object > Sprites > Square**
2. Rename to `Player`
3. **Add Components:**
   - Add Component > **Rigidbody 2D**
     - Body Type: Dynamic
     - Gravity Scale: 0
     - Constraints: Freeze Rotation Z
   - Add Component > **Capsule Collider 2D** (or Circle Collider 2D)
     - Set size to match sprite
   - Add Component > **Animator** (create later)
   - Add Component > `PlayerController.cs`
   - Add Component > `PlayerStats.cs`
   - Add Component > `PlayerInventory.cs`
   - Add Component > `PlayerCombat.cs`

4. **Configure PlayerController:**
   - Assign `rb` field (drag Rigidbody2D component)
   - Set `moveSpeed`: 5
   - Set `sprintMultiplier`: 1.5
   - Set `isIsometric`: True (if using isometric view)
   - Set `interactionRange`: 2
   - Set **Layer Masks:**
     - `interactableLayer`: Select "Interactable" layer
     - (Leave others default for now)

5. **Configure PlayerCombat:**
   - Set `enemyLayer`: Select "Enemy" layer
   - Set `obstacleLayer`: Select "Obstacle" layer
   - Set `baseMeleeDamage`: 10
   - Set `meleeRange`: 1.5
   - Leave other fields at defaults

6. **Set Layer and Tag:**
   - Set Layer: `Player`
   - Set Tag: `Player`

7. **Save as Prefab:**
   - Drag `Player` from Hierarchy to `Assets/Prefabs/` folder
   - This creates a prefab you can reuse

### Step 2: Create Zombie Prefabs

For each zombie type, create a prefab:

1. In Hierarchy, right-click > **2D Object > Sprites > Square**
2. Rename to `Zombie_Walker` (or Runner, Brute, etc.)
3. **Add Components:**
   - Add Component > **Rigidbody 2D**
     - Body Type: Dynamic
     - Gravity Scale: 0
     - Constraints: Freeze Rotation Z
   - Add Component > **Capsule Collider 2D**
   - Add Component > **Animator**
   - Add Component > `ZombieAI.cs`

4. **Configure ZombieAI:**
   - Set `zombieType`: Walker (or respective type)
   - Assign `rb` field (drag Rigidbody2D)
   - Set `maxHealth`: 50 (adjust per type)
   - Set `moveSpeed`: 2 (adjust per type)
   - Set `attackDamage`: 10
   - Set `visionRange`: 10
   - Set `hearingRange`: 15
   - Set **Layer Masks:**
     - `playerLayer`: Select "Player" layer
     - `obstacleLayer`: Select "Obstacle" layer

5. **Set Layer and Tag:**
   - Set Layer: `Enemy`
   - Set Tag: `Zombie`

6. **Change Sprite Color (temporary visual distinction):**
   - Select Sprite Renderer
   - Set Color: Red (for Walkers), Yellow (Runners), etc.

7. **Save as Prefab:**
   - Drag to `Assets/Prefabs/` folder
   - Repeat for all 7 zombie types (optional: do Walker first, test, then create variants)

### Step 3: Create Item Pickup Prefab (Simple)

1. Create **2D Object > Sprites > Circle**
2. Rename to `ItemPickup`
3. **Add Components:**
   - Add Component > **Circle Collider 2D**
     - Is Trigger: Checked
   - (ItemPickup script is embedded in PlayerController.cs)

4. Set Layer: `Interactable`
5. Set Tag: `Item`
6. Save as Prefab

---

## 7. SCRIPT INTEGRATION

### Step 1: Test Player in Scene

1. Open `Game.unity`
2. Place `Player` prefab in scene (near origin: 0, 0, 0)
3. **Press Play**
4. Test:
   - Movement (WASD or Arrow keys)
   - Sprint (Hold Shift)
   - Inventory (E key for interaction - won't work without items yet)
   - Check Console for any errors

### Step 2: Test Zombie AI

1. Place 1-2 `Zombie_Walker` prefabs in scene (around position 10, 10)
2. **Press Play**
3. Observe:
   - Zombies should wander around
   - If you get close, they should chase you
   - If they reach you, they should attack (check Console logs)
4. Debug visualizations:
   - Select a zombie in Hierarchy while playing
   - In Scene view, you'll see vision/hearing/smell ranges (colored spheres)

### Step 3: Test Combat

1. With Player selected, note `PlayerCombat` component
2. **Press Play**
3. Test melee:
   - Approach a zombie
   - Click **Left Mouse Button** (or hold Ctrl)
   - Zombie should take damage (red flash) and die after hits
4. Test ranged (if projectile prefab created):
   - Switch to ranged mode (Tab key)
   - Aim with mouse
   - Click to shoot
   - (Will not work without projectile prefab - create in Step 6.4 below)

### Step 4: Test Time System

1. Select `TimeManager` in Hierarchy
2. In Inspector, set `dayLengthInSeconds`: 120 (2 minutes for testing)
3. Set `startingHour`: 6 (dawn)
4. **Press Play**
5. Observe:
   - Lighting should gradually change from dark to light (dawn)
   - After ~60 seconds, it should get dark again (dusk/night)
   - Check Console for day/night transition logs

### Step 5: Test Save/Load

1. **Press Play**
2. Move player around, change stats (e.g., take damage from zombie)
3. Open Console and type (or add a test button in Inspector):
   - Call `SaveManager.Instance.SaveGame()` via script or Inspector button
4. Check Console: Should say "Game saved successfully"
5. Stop Play mode
6. **Press Play** again
7. Call `SaveManager.Instance.LoadGame()`
8. Player should teleport to saved position

---

## 8. ASSET IMPORT

### Step 1: Download 8-bit Pixel Art Assets (Free Resources)

**Recommended Sources:**
- **Kenney.nl** (kenney.nl/assets) - Free game assets
  - "Pixel Platformer" pack
  - "RPG Urban Pack"
- **itch.io** - Search "8-bit isometric" or "pixel zombie"
  - Many free asset packs available
- **OpenGameArt.org** - Community-created assets

### Step 2: Import Sprites

1. Download sprite packs (PNG files)
2. In Unity, drag image files to `Assets/Sprites/` subfolders:
   - Characters → `Assets/Sprites/Characters/`
   - Zombies → `Assets/Sprites/Zombies/`
   - Items → `Assets/Sprites/Items/`
   - Tiles → `Assets/Sprites/Tiles/`
   - UI → `Assets/Sprites/UI/`

3. **Configure Sprite Import Settings:**
   - Select all sprites in Project window
   - In Inspector:
     - Texture Type: Sprite (2D and UI)
     - Sprite Mode: Multiple (for spritesheets) or Single
     - Pixels Per Unit: 16 or 32 (for 8-bit style)
     - Filter Mode: Point (no filter) - for crisp pixels
     - Compression: None (for pixel art)
     - Click **"Apply"**

4. **Slice Spritesheets (if applicable):**
   - Select spritesheet
   - Click **"Sprite Editor"** button in Inspector
   - Click **"Slice"** > Choose "Grid By Cell Count" or "Automatic"
   - Set cell size (e.g., 32x32 pixels)
   - Click **"Slice"**, then **"Apply"**

### Step 3: Apply Sprites to Prefabs

1. Open `Player` prefab (double-click in Project window)
2. Select the Player GameObject
3. In **Sprite Renderer** component:
   - Drag player sprite to `Sprite` field
4. Repeat for zombie prefabs (use different sprites for each type)

### Step 4: Import Audio

1. Download 8-bit sound effects and music:
   - **Freesound.org** - Free SFX
   - **OpenGameArt.org** - Free music
   - **Incompetech.com** - Royalty-free music
   - **BFXR** (bfxr.net) - Generate 8-bit SFX

2. Drag audio files to `Assets/Audio/` subfolders:
   - Music → `Assets/Audio/Music/`
   - SFX → `Assets/Audio/SFX/`

3. **Configure Audio Import Settings:**
   - Select audio files
   - In Inspector:
     - Load Type: "Compressed In Memory" (for music) or "Decompress On Load" (for short SFX)
     - Compression Format: Vorbis (for music) or PCM (for SFX)
     - Click **"Apply"**

4. **Assign to AudioManager:**
   - Select `AudioManager` in Hierarchy
   - In Inspector, expand `AudioManager` script
   - Drag audio clips to corresponding fields:
     - `dayMusic` → Your day music clip
     - `nightMusic` → Your night music clip
     - `footstepClip` → Footstep SFX
     - `zombieGroanClip` → Zombie SFX
     - Etc.

---

## 9. TESTING

### Full Playtest Checklist

#### Core Systems
- [ ] Player moves smoothly with WASD
- [ ] Sprint (Shift) works and drains stamina
- [ ] Stats decay over time (check in Inspector while playing)
- [ ] Time cycles through day/night
- [ ] Lighting changes with time of day
- [ ] Save game creates a file (check `Application.persistentDataPath`)
- [ ] Load game restores player position and stats

#### Combat
- [ ] Melee attack damages zombies
- [ ] Zombies die after enough damage
- [ ] Ranged attack works (if implemented)
- [ ] Player takes damage from zombies
- [ ] Player dies at 0 health (triggers game over)

#### AI
- [ ] Zombies wander when idle
- [ ] Zombies chase player when detected
- [ ] Zombies attack player when in range
- [ ] Zombies hear sounds (e.g., gunshots)
- [ ] Different zombie types have different speeds/behaviors

#### Inventory
- [ ] Items can be picked up (if ItemPickup prefabs placed)
- [ ] Inventory UI shows items (if UI implemented)
- [ ] Items can be used (food, medicine, etc.)
- [ ] Weight limit prevents over-encumbrance

#### Audio
- [ ] Music plays and changes with day/night
- [ ] Footsteps play when walking
- [ ] Combat sounds play
- [ ] Sounds propagate to zombies (they investigate)

### Performance Testing

1. Place 50+ zombies in scene
2. Press Play
3. Check **Window > Analysis > Profiler**
4. Ensure FPS stays above 30 (ideally 60)
5. If performance is poor:
   - Reduce zombie count
   - Optimize scripts (e.g., reduce Update() frequency)
   - Use object pooling for zombies

---

## 10. ML-AGENTS SETUP (Optional - Week 7+)

### Prerequisites

- Unity ML-Agents package installed (see Step 3.E)
- Python 3.8-3.10 installed
- Administrator/sudo access for package installation

### Step 1: Install Python Packages

Open terminal/command prompt:

```bash
pip install mlagents
pip install torch torchvision
```

Verify installation:
```bash
mlagents-learn --help
```

Should display ML-Agents help text.

### Step 2: Create Training Configuration

1. Navigate to `DeadDays/Assets/ML-Agents/Config/`
2. Create file `zombie_training.yaml` (if not exists)
3. Copy configuration from README.md (section: ML-AGENTS TRAINING CONFIGURATION)

### Step 3: Create SmartZombieAgent Script

1. In `Assets/Scripts/AI/`, create `SmartZombieAgent.cs`
2. Use the template from README.md or create from scratch:

```csharp
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SmartZombieAgent : Agent
{
    // Implement CollectObservations, OnActionReceived, Heuristic
}
```

3. Implement observation space, action space, and reward function as per README

### Step 4: Create Training Scene

1. Open `Training.unity` scene
2. Create training environment:
   - Add multiple zombie agents
   - Add player (or dummy target)
   - Add obstacles
3. Configure ML-Agents Academy (automatically detected)

### Step 5: Run Training

Open terminal in project root:

```bash
mlagents-learn Assets/ML-Agents/Config/zombie_training.yaml --run-id=zombie_run_1
```

Press **Play** in Unity when prompted.

Training will run for several hours (10M+ steps). Monitor progress in terminal.

---

## 11. PYTHON ENVIRONMENT (Optional - For ML Training)

### Step 1: Install Python (If Not Already)

1. Download Python 3.8, 3.9, or 3.10 from python.org
2. **Important:** Check "Add Python to PATH" during installation
3. Verify: Open terminal and type `python --version`

### Step 2: Create Virtual Environment (Recommended)

Navigate to `DeadDays/Python/` folder:

```bash
cd Python
python -m venv ml_env
```

Activate:
- **Windows:** `ml_env\Scripts\activate`
- **macOS/Linux:** `source ml_env/bin/activate`

### Step 3: Install Dependencies

```bash
pip install mlagents==0.30.0
pip install torch torchvision torchaudio
pip install numpy pandas matplotlib
```

### Step 4: Create Training Script

Create `train.py` in `Python/` folder (basic template):

```python
from mlagents_envs.environment import UnityEnvironment

env = UnityEnvironment(file_name=None)  # Uses open Unity Editor
env.reset()

# Training loop here
```

---

## 12. TROUBLESHOOTING

### Common Issues

#### Issue 1: Scripts Have Errors After Import
**Solution:**
- Ensure all packages are installed (TextMeshPro, 2D Sprite, etc.)
- Check Unity version (2021.3+ required)
- Reimport scripts: Right-click `Assets/Scripts/` > Reimport

#### Issue 2: Player Doesn't Move
**Solution:**
- Check `PlayerController` has `Rigidbody2D` assigned
- Ensure Input Manager has "Horizontal" and "Vertical" axes
- Check Console for errors
- Ensure no colliders are blocking movement

#### Issue 3: Zombies Don't Chase Player
**Solution:**
- Verify `playerLayer` is set to "Player" layer in `ZombieAI`
- Ensure Player GameObject has Layer set to "Player"
- Check `visionRange` is large enough (default: 10)
- Debug: Select zombie in Hierarchy while playing, look for vision cone in Scene view

#### Issue 4: Day/Night Cycle Not Working
**Solution:**
- Ensure URP package is installed
- Check `TimeManager` has `globalLight` field assigned (drag Global Light 2D)
- Verify Global Light 2D exists in scene
- Check `dayLengthInSeconds` is not 0

#### Issue 5: Audio Not Playing
**Solution:**
- Ensure audio clips are assigned in `AudioManager` Inspector
- Check audio import settings (see Step 8.4)
- Verify `AudioListener` exists in scene (usually on Main Camera)
- Check volume sliders in `AudioManager` are not 0

#### Issue 6: Save/Load Not Working
**Solution:**
- Check Console for error messages
- Verify `SaveManager` is in scene and not destroyed
- Check file permissions for `Application.persistentDataPath`
- Test: Call `SaveManager.Instance.SaveGame()` manually via script

#### Issue 7: High Memory Usage / Low FPS
**Solution:**
- Reduce zombie count (start with 10-20)
- Disable audio object pooling temporarily
- Use Profiler (**Window > Analysis > Profiler**) to identify bottleneck
- Ensure "VSync" is enabled in Quality Settings

#### Issue 8: ML-Agents Training Not Starting
**Solution:**
- Ensure ML-Agents Python package is installed: `pip install mlagents`
- Check Python version (3.8-3.10 only)
- Verify `zombie_training.yaml` path is correct
- Ensure Unity scene has ML-Agents Academy (auto-detected)
- Check firewall isn't blocking Unity <-> Python communication

---

## 📞 ADDITIONAL RESOURCES

### Official Unity Documentation
- Unity Manual: https://docs.unity3d.com/Manual/
- Unity Scripting API: https://docs.unity3d.com/ScriptReference/
- ML-Agents Documentation: https://github.com/Unity-Technologies/ml-agents

### Tutorials
- Brackeys (YouTube): Unity 2D basics
- Sebastian Lague (YouTube): Advanced programming
- Unity Learn: https://learn.unity.com/

### Asset Resources
- Kenney.nl: Free game assets
- OpenGameArt.org: Community assets
- itch.io: Indie game assets (free and paid)

### Community Support
- Unity Forums: https://forum.unity.com/
- Reddit: r/Unity2D, r/Unity3D
- Discord: Unity Developer Community

---

## ✅ FINAL CHECKLIST

Before considering setup complete:

- [ ] Unity project opens without errors
- [ ] All scripts compile successfully
- [ ] Player prefab moves in scene
- [ ] At least 1 zombie prefab works
- [ ] Time cycle progresses (day to night)
- [ ] Audio plays (music and SFX)
- [ ] Save/load creates and reads files
- [ ] No errors in Console during Play mode
- [ ] Game runs at acceptable FPS (30+)

---

## 🎉 YOU'RE READY TO DEVELOP!

With setup complete, you can now:
1. Follow the **Development Roadmap** in README.md
2. Implement remaining systems (World Generation, Crafting, UI)
3. Create custom content (items, recipes, zombie types)
4. Train ML-Agents for adaptive zombie AI
5. Playtest and iterate

Refer to **PROJECT_STATUS.md** to track progress and see what's completed vs. pending.

**Happy developing!** 🧟‍♂️🎮

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**For DeadDays v0.1.0 - Foundation Phase**