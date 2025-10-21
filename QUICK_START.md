# DeadDays - QUICK START CHECKLIST

**Get your game running in under 1 hour!**

---

## ✅ IMMEDIATE SETUP (30-60 minutes)

### Step 1: Unity Installation (10 min)
- [ ] Download and install Unity Hub from unity.com
- [ ] Install Unity 2021.3 LTS or newer (2D template)
- [ ] Install Visual Studio or VS Code with C# extension

### Step 2: Create Unity Project (5 min)
- [ ] Open Unity Hub
- [ ] Click "New Project"
- [ ] Select "2D (Core)" or "2D (URP)" template
- [ ] Name: `DeadDays`
- [ ] Location: `D:\Github\DeadDays`
- [ ] Click "Create Project"
- [ ] Wait for Unity to open (may take 2-3 minutes)

### Step 3: Install Required Packages (10 min)
- [ ] Open Window > Package Manager
- [ ] Install: "2D Tilemap Editor" (search and click Install)
- [ ] Install: "TextMeshPro" (click Install if prompted)
- [ ] Install: "Universal RP" (optional but recommended)
- [ ] Wait for packages to import
- [ ] Close Package Manager

### Step 4: Configure Project Settings (5 min)
- [ ] Go to Edit > Project Settings > Physics 2D
- [ ] Set Gravity Y to 0
- [ ] Go to Tags and Layers
- [ ] Add Tags: `Player`, `Zombie`, `Wall`, `Door`, `Item`
- [ ] Add Layers: `Player` (6), `Enemy` (7), `Obstacle` (8), `Interactable` (9)
- [ ] Close Project Settings

### Step 5: Create Game Scene (10 min)
- [ ] In Project window, navigate to Assets/Scenes/
- [ ] Create 2 new scenes: `MainMenu.unity`, `Game.unity`
- [ ] Open `Game.unity` (double-click)
- [ ] In Hierarchy, create Empty GameObject, name it `GameManager`
- [ ] Add Component > GameManager (script should auto-attach)
- [ ] Create 4 more Empty GameObjects: `TimeManager`, `SaveManager`, `AudioManager`, `SkillSystem`
- [ ] Attach corresponding scripts to each (Add Component > script name)
- [ ] Verify no red errors in Inspector

### Step 6: Set Up Lighting (5 min)
- [ ] In Hierarchy, right-click > Light > Global Light 2D
- [ ] Set Intensity: 1.0, Color: White
- [ ] Select `TimeManager` in Hierarchy
- [ ] Drag "Global Light 2D" to the `globalLight` field in Inspector
- [ ] Verify field shows "Global Light 2D" (not "None")

### Step 7: Create Player Prefab (10 min)
- [ ] In Hierarchy, right-click > 2D Object > Sprites > Square
- [ ] Rename to `Player`
- [ ] Add Component > Rigidbody 2D
  - [ ] Set Body Type: Dynamic
  - [ ] Set Gravity Scale: 0
  - [ ] Check "Freeze Rotation Z" under Constraints
- [ ] Add Component > Capsule Collider 2D
- [ ] Add Component > PlayerController
- [ ] Add Component > PlayerStats
- [ ] Add Component > PlayerInventory
- [ ] Add Component > PlayerCombat
- [ ] Set Layer to "Player" (top of Inspector)
- [ ] Set Tag to "Player"
- [ ] Drag Player from Hierarchy to Assets/Prefabs/ folder (creates prefab)

### Step 8: Create Zombie Prefab (10 min)
- [ ] In Hierarchy, right-click > 2D Object > Sprites > Square
- [ ] Rename to `Zombie_Walker`
- [ ] Add Component > Rigidbody 2D (same settings as Player)
- [ ] Add Component > Capsule Collider 2D
- [ ] Add Component > ZombieAI
- [ ] In ZombieAI component:
  - [ ] Set Zombie Type: Walker
  - [ ] Set Player Layer: "Player"
  - [ ] Set Obstacle Layer: "Obstacle"
- [ ] Change Sprite Renderer color to Red (for visibility)
- [ ] Set Layer to "Enemy"
- [ ] Set Tag to "Zombie"
- [ ] Drag Zombie from Hierarchy to Assets/Prefabs/

### Step 9: Test Basic Gameplay (5 min)
- [ ] Ensure Player and 1-2 Zombies are in scene
- [ ] Position Player at (0, 0, 0)
- [ ] Position Zombies at (5, 5, 0) and (-5, -5, 0)
- [ ] Press PLAY button (top center)
- [ ] Test movement with WASD keys
- [ ] Test sprint with Shift key
- [ ] Observe zombies wandering/chasing
- [ ] Press PLAY again to stop

---

## 🎮 VERIFY IT WORKS

### Movement Test
- [ ] WASD moves the player
- [ ] Shift makes player run faster
- [ ] Player sprite faces movement direction
- [ ] No errors in Console window

### Zombie AI Test
- [ ] Zombies wander randomly when player is far
- [ ] Zombies chase player when close (< 10 units)
- [ ] Zombies change to red when chasing (or maintain color)
- [ ] Console shows "Zombie entered state: Chase" when close

### Combat Test
- [ ] Left click near zombie deals damage
- [ ] Zombie flashes red when hit
- [ ] Zombie dies after multiple hits
- [ ] Console shows "Zombie died!" message

### Time System Test
- [ ] Select TimeManager in Hierarchy
- [ ] Set Day Length In Seconds: 120 (2 minutes)
- [ ] Press Play
- [ ] Observe lighting changing over 2 minutes
- [ ] Console shows "Day X - Dawn has arrived" messages

---

## 🚨 TROUBLESHOOTING

### "Scripts have errors"
**Solution:** Wait for Unity to finish compiling (bottom-right corner). If errors persist, reimport scripts: Right-click Assets/Scripts > Reimport.

### "Player doesn't move"
**Solution:** 
1. Check PlayerController has Rigidbody2D assigned in Inspector
2. Ensure Gravity Scale is 0
3. Check Console for error messages

### "Zombies don't chase player"
**Solution:**
1. Verify ZombieAI has "Player" layer selected in `playerLayer` field
2. Ensure Player GameObject has Layer set to "Player"
3. Check zombie's vision range is 10+ in ZombieAI component

### "Time cycle doesn't work"
**Solution:**
1. Ensure Universal RP package is installed
2. Verify TimeManager has Global Light 2D assigned in `globalLight` field
3. Check dayLengthInSeconds is not 0

### "Everything is white squares"
**Expected!** You haven't imported sprites yet. This is normal. Systems work, just no art yet.

---

## 📚 NEXT STEPS (After Quick Start)

### Today/Tomorrow
- [ ] Read SETUP_GUIDE.md (full detailed setup)
- [ ] Import free 8-bit sprites from Kenney.nl
- [ ] Assign sprites to Player and Zombie prefabs
- [ ] Import free audio clips
- [ ] Assign audio to AudioManager

### This Week
- [ ] Read PROJECT_STATUS.md (see what's done vs. pending)
- [ ] Follow README.md Development Roadmap Phase 1
- [ ] Implement World Generation (WorldGenerator.cs)
- [ ] Create item database

### This Month
- [ ] Implement UI systems (HUDManager, InventoryUI, etc.)
- [ ] Implement Crafting System
- [ ] Create 50+ items and 20+ recipes
- [ ] Add animations
- [ ] Playtest and balance

---

## 📖 DOCUMENTATION GUIDE

- **QUICK_START.md** ← You are here! (1-hour setup)
- **SETUP_GUIDE.md** - Detailed setup with troubleshooting (767 lines)
- **PROJECT_STATUS.md** - Track implementation progress (694 lines)
- **COMPLETION_SUMMARY.md** - Summary of completed work (519 lines)
- **README.md** - Complete game design document (3000+ lines)

**Start with QUICK_START.md → Then read SETUP_GUIDE.md → Then follow README.md roadmap**

---

## ✅ COMPLETION CHECKLIST

You're done with Quick Start when:
- [ ] Unity project opens without errors
- [ ] Player moves with WASD
- [ ] At least 1 zombie exists and chases player
- [ ] Combat works (zombie takes damage and dies)
- [ ] Time cycle works (lighting changes)
- [ ] Console shows no red errors during play

**If all checked, CONGRATULATIONS! You're ready to develop!** 🎉

**Next:** Read SETUP_GUIDE.md Section 6-8 for prefab details and asset import.

---

**Estimated Time:** 30-60 minutes  
**Difficulty:** Beginner-friendly  
**Prerequisites:** Unity Hub installed  
**Result:** Playable prototype with core systems working

**Let's build DeadDays! 🧟‍♂️🎮**