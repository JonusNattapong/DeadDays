# 📋 COMPLETE GAME PLAN DeadDays: 8-bit Isometric Zombie Survival with AI

---

## 🎯 **PROJECT OVERVIEW**

**Game Title:** Zombie Survival 8-bit (ชื่อชั่วคราว)

**Genre:** Isometric Survival Horror, Roguelike, AI-Driven

**Style:** Project Zomboid + 8-bit Pixel Art

**Core Hook:** Zombie AI ที่เรียนรู้พฤติกรรมผู้เล่นและปรับกลยุทธ์

**Target Platform:** PC (Windows/Mac/Linux)

**Development Time:** 3-6 เดือน (Full Version)

---

## 📊 **GAME DESIGN DOCUMENT**

### **1. CORE GAMEPLAY LOOP**

```
┌─────────────────────────────────────┐
│  Wake Up → Check Stats → Plan Day  │
│           ↓                         │
│  Scavenge for Resources             │
│           ↓                         │
│  Avoid/Fight Zombies (AI Learning)  │
│           ↓                         │
│  Return to Base → Secure → Rest     │
│           ↓                         │
│  Survive Another Day → Repeat       │
└─────────────────────────────────────┘
```

### **2. CHARACTER SYSTEMS**

#### **Stats & Needs:**

```
┌─────────────────────────────────────┐
│ ❤️  HEALTH       [████████░░] 80%  │
│ 🍖 HUNGER       [██████░░░░] 60%  │
│ 💧 THIRST       [████████░░] 80%  │
│ 😴 FATIGUE      [████░░░░░░] 40%  │
│ 🤒 INFECTION    [░░░░░░░░░░]  0%  │
│ 🧠 PANIC        [███░░░░░░░] 30%  │
│ 🌡️  TEMPERATURE [███████░░░] 70%  │
└─────────────────────────────────────┘
```

#### **Effects System:**

```
IF Hunger > 80%  → Health -1 per hour
IF Thirst > 90%  → Health -2 per hour
IF Fatigue > 80% → Move slower, aim worse
IF Infection > 50% → Fever, hallucinations
IF Panic > 70%   → Reduced accuracy, faster fatigue
IF Temperature < 20% → Health -1 per hour (hypothermia)
```

#### **Skills (Level 1-10):**

```
🔨 CARPENTRY
- Lv1: ตอกไม้ปิดหน้าต่าง
- Lv5: สร้างกำแพง กับดัก
- Lv10: สร้างเฟอร์นิเจอร์ ฐานขั้นสูง

🍳 COOKING
- Lv1: ต้มน้ำ ย่างเนื้อ
- Lv5: ทำอาหารซับซ้อน (+nutrition)
- Lv10: ถนอมอาหารนานขึ้น

💪 STRENGTH
- Lv1: ตีปกติ
- Lv5: ตีแรงขึ้น 50%, ยกของหนักได้
- Lv10: ตีแรงขึ้น 100%, ผลักฝูงได้

🏃 FITNESS
- Lv1: วิ่งปกติ
- Lv5: วิ่งไกลขึ้น, เหนื่อยช้าลง
- Lv10: วิ่งได้นาน, กระโดดสูงขึ้น

🎯 AIMING
- Lv1: ยิงได้ accuracy 30%
- Lv5: 60% accuracy
- Lv10: 90% accuracy + headshot chance

🔧 MECHANICS
- Lv1: ซ่อมเครื่องมือ
- Lv5: สร้างเครื่องมือใหม่
- Lv10: ซ่อมรถ, สร้างเครื่องจักร

🌿 FORAGING
- Lv1: หาผัก เห็ด
- Lv5: หาของหายาก
- Lv10: รู้ว่าอะไรพิษ/ไม่พิษ

🏥 FIRST AID
- Lv1: พันแผลพื้นฐาน
- Lv5: ผ่าตัดดึงกระสุน
- Lv10: รักษา infection ได้

🥷 STEALTH
- Lv1: เดินเงียบ
- Lv5: Zombie เห็นยากขึ้น
- Lv10: เดินผ่าน Zombie ได้
```

### **3. INVENTORY SYSTEM**

#### **Inventory Slots:**

```
┌─────────────────────────────────────┐
│ EQUIPPED:                           │
│ 🔫 Main Hand: [Baseball Bat]       │
│ 🗡️  Secondary: [Kitchen Knife]     │
│                                     │
│ BACKPACK: (20/30 kg)                │
│ [🥫][💊][🔦][🔋][📦][🧃]           │
│ [🍞][🔧][🪓][  ][  ][  ]           │
│                                     │
│ EQUIPPED ITEMS:                     │
│ 👕 Jacket (Protection +5)           │
│ 👖 Jeans                            │
│ 👟 Boots (Noise -20%)               │
└─────────────────────────────────────┘
```

#### **Item Categories:**

**WEAPONS - Melee:**

```
🔨 Hammer        | Damage: 20 | Durability: 100/100 | Speed: Fast
🪓 Axe           | Damage: 35 | Durability: 80/80   | Speed: Slow
🔪 Kitchen Knife | Damage: 15 | Durability: 50/50   | Speed: Very Fast
🏏 Baseball Bat  | Damage: 25 | Durability: 70/70   | Speed: Medium
🍳 Frying Pan    | Damage: 18 | Durability: 90/90   | Speed: Fast
🔧 Pipe Wrench   | Damage: 22 | Durability: 85/85   | Speed: Medium
⛏️  Pickaxe      | Damage: 30 | Durability: 100/100 | Speed: Slow
🗡️  Machete      | Damage: 28 | Durability: 75/75   | Speed: Medium
```

**WEAPONS - Ranged:**

```
🔫 Pistol        | Damage: 40 | Ammo: 12/12  | Noise: LOUD | Range: Medium
🔫 Shotgun       | Damage: 80 | Ammo: 6/6    | Noise: VERY LOUD | Range: Short
🏹 Crossbow      | Damage: 60 | Ammo: 1/1    | Noise: Quiet | Range: Long
🎯 Hunting Rifle | Damage: 90 | Ammo: 5/5    | Noise: LOUD | Range: Very Long
💣 Molotov       | Damage: 100| Uses: 1      | Noise: Medium | AoE
🧨 Pipe Bomb     | Damage: 150| Uses: 1      | Noise: VERY LOUD | AoE
```

**FOOD & DRINK:**

```
🥫 Canned Food   | Hunger -40 | Freshness: Never spoils
🍞 Bread         | Hunger -20 | Freshness: 5 days
🍎 Apple         | Hunger -10, Thirst -5 | Freshness: 7 days
🥩 Raw Meat      | Hunger -30 (cooked -50) | Freshness: 2 days
🧃 Water Bottle  | Thirst -50 | Reusable
🥤 Soda          | Thirst -30, Hunger -5 | Never spoils
🍕 Pizza (Cooked)| Hunger -60 | Freshness: 3 days
☕ Coffee        | Fatigue -20, Thirst +10 | Freshness: Never
```

**MEDICAL:**

```
💊 Painkillers   | Pain -50 for 2 hours
🩹 Bandage       | Bleeding -30, Health +5
💉 Antibiotics   | Infection -20 over time
🧴 Disinfectant  | Infection -10, prevents infection
🩺 First Aid Kit | Health +30, heals wounds
💊 Vitamins      | Health +5, immunity +10
🌡️  Thermometer  | Check infection level
```

**TOOLS:**

```
🔦 Flashlight    | Battery: 100% | Reveals 10 tiles
🪓 Axe           | Chop trees, break doors
🔨 Hammer        | Build, repair, fight
🪛 Screwdriver   | Dismantle electronics
🧰 Toolbox       | Craft better items
🔥 Lighter       | Start fires, light cigarettes
🗝️  Keys         | Open specific doors
📻 Radio         | Hear emergency broadcasts (future)
🧭 Compass       | Always know direction
🗺️  Map          | Reveal explored areas
```

**MATERIALS:**

```
🪵 Wood Plank    | Weight: 2kg | Used for building
🔩 Scrap Metal   | Weight: 3kg | Craft weapons/tools
🧵 Cloth/Rags    | Weight: 0.2kg | Bandages, rope
🪢 Rope          | Weight: 0.5kg | Crafting, traps
🔋 Battery       | Weight: 0.3kg | Power devices
📦 Nails         | Weight: 0.5kg | Building
🧱 Bricks        | Weight: 5kg | Strong walls
🪟 Glass Shard   | Weight: 0.3kg | Craft weapons
```

### **4. CRAFTING SYSTEM**

#### **Recipes:**

```
WEAPONS:
🗡️  Spiked Bat = Baseball Bat + Nails (Carpentry 2)
🔥 Molotov = Bottle + Cloth + Gasoline (General 1)
🧨 Pipe Bomb = Pipe + Gunpowder + Wire (Mechanics 3)
🏹 Wooden Spear = Wood Plank + Kitchen Knife (Carpentry 1)

TOOLS:
🪓 Stone Axe = Rock + Rope + Stick (Carpentry 1)
🔦 Torch = Stick + Cloth + Lighter (General 0)
🎣 Fishing Rod = Stick + Rope + Hook (Mechanics 2)

MEDICAL:
🩹 Improvised Bandage = Cloth + Disinfectant (First Aid 1)
💊 Herbal Medicine = Herbs + Water (Foraging 3)
🧴 Sterilized Bandage = Bandage + Fire (First Aid 2)

FOOD:
🍲 Stew = Meat + Vegetables + Water + Pot (Cooking 2)
🥖 Bread = Flour + Water + Oven (Cooking 3)
☕ Herbal Tea = Herbs + Boiled Water (Cooking 1)

BUILDING:
🚪 Barricade = Wood Planks x4 + Nails (Carpentry 2)
🪟 Window Boards = Wood Planks x2 + Nails (Carpentry 1)
🛏️  Bed = Wood Planks x6 + Cloth x3 (Carpentry 4)
📦 Storage Box = Wood Planks x5 + Nails (Carpentry 3)
🚪 Door = Wood Planks x8 + Hinges (Carpentry 5)
```

### **5. ZOMBIE SYSTEM**

#### **Zombie Types:**

```
🧟 SHAMBLER (Walker)
├─ HP: 100
├─ Speed: 0.5 m/s
├─ Damage: 10
├─ Vision: 8 tiles
├─ Hearing: 15 tiles
├─ Behavior: Slow, steady, persistent
└─ Spawn Rate: 60%

🧟💨 RUNNER (Fast Shambler)
├─ HP: 80
├─ Speed: 1.5 m/s
├─ Damage: 15
├─ Vision: 10 tiles
├─ Hearing: 12 tiles
├─ Behavior: Chase aggressively
└─ Spawn Rate: 25%

🧟💪 TANK (Brute)
├─ HP: 300
├─ Speed: 0.3 m/s
├─ Damage: 30
├─ Vision: 6 tiles
├─ Hearing: 10 tiles
├─ Behavior: Break through walls, slow but deadly
└─ Spawn Rate: 5%

🧟👂 SCREAMER (Alert)
├─ HP: 60
├─ Speed: 0.7 m/s
├─ Damage: 8
├─ Vision: 12 tiles
├─ Hearing: 25 tiles
├─ Behavior: Screams when sees player → attracts horde
└─ Spawn Rate: 8%

🧟🧠 SMART ZOMBIE (AI Learner) ⭐
├─ HP: 120
├─ Speed: 1.0 m/s (adaptive)
├─ Damage: 20
├─ Vision: 15 tiles
├─ Hearing: 20 tiles
├─ Behavior: LEARNS player patterns, sets ambushes
└─ Spawn Rate: 2% (increases over time)
```

#### **Zombie Senses:**

```python
# Vision System
vision_range = base_range
if is_daytime:
    vision_range *= 1.5
if player_has_flashlight:
    vision_range *= 2.0
if player_crouching:
    vision_range *= 0.5

# Hearing System
sound_levels = {
    "walking": 5,
    "running": 15,
    "gunshot": 50,
    "car_engine": 40,
    "door_slam": 20,
    "window_break": 25,
    "shouting": 30,
}

# Smell System (Blood)
if player_is_bleeding:
    attract_radius = 30
    attract_speed_multiplier = 1.5
```

#### **Zombie States:**

```
IDLE → INVESTIGATING → CHASING → ATTACKING → EATING
  ↑                                            ↓
  └──────────────────────────────────────────────┘

States:
- IDLE: Standing/wandering randomly
- INVESTIGATING: Heard sound, moving to location
- CHASING: Sees player, actively pursuing
- ATTACKING: In melee range, attacking player
- EATING: Found corpse, distracted for 30s
```

### **6. AI LEARNING SYSTEM** 🧠⭐

#### **Data Collection:**

```python
class PlayerBehaviorData:
    # Movement Patterns
    favorite_locations: List[Vector2]  # ไปไหนบ่อย
    common_paths: List[Path]           # เส้นทางที่ใช้บ่อย
    active_hours: List[int]            # ออกหาของช่วงไหน (0-23)
    hide_spots: List[Vector2]          # ชอบหลบไหน

    # Combat Patterns
    weapon_preference: str             # "melee" or "ranged"
    engagement_distance: float         # ยิงระยะเท่าไหร่
    fight_or_flight_threshold: int     # Zombie กี่ตัวถึงหนี
    retreat_direction: str             # หนีทิศไหนบ่อย

    # Resource Patterns
    looting_priority: List[str]        # เก็บของอะไรก่อน
    favorite_safehouses: List[Vector2] # บ้านที่ชอบ
    base_location: Vector2             # ฐานที่มั่น

    # Survival Patterns
    risk_tolerance: float              # กล้าเสี่ยงแค่ไหน (0-1)
    health_retreat_threshold: int      # เลือดเท่าไหร่ถึงหนี
    supplies_before_return: int        # เก็บของกี่ชิ้นถึงกลับบ้าน

    # Weaknesses Detected
    most_vulnerable_time: int          # ช่วงเวลาที่อ่อนแอ
    blind_spots: List[str]             # มุมที่มองไม่เห็น
    predictable_behaviors: List[str]   # พฤติกรรมที่คาดเดาได้
```

#### **AI Adaptive Behaviors:**

```python
class SmartZombieAI:

    def adapt_to_player(self, player_data):
        """ปรับกลยุทธ์ตาม player data"""

        # 1. Ambush Strategy
        if player_data.predictable_paths:
            self.set_ambush_points(player_data.common_paths)

        # 2. Flanking
        if player_data.weapon_preference == "ranged":
            self.use_cover = True
            self.zigzag_movement = True

        # 3. Horde Tactics
        if player_data.weapon_preference == "melee":
            self.call_reinforcements()
            self.surround_player = True

        # 4. Base Siege
        if player_data.base_location:
            self.plan_night_raid(player_data.base_location)

        # 5. Time-based Attacks
        if player_data.most_vulnerable_time:
            self.attack_time = player_data.most_vulnerable_time

        # 6. Fake Out
        if player_data.fight_or_flight_threshold:
            # ส่ง zombie น้อยกว่า threshold → player มั่นใจ
            # แล้วซุ่มโจมตีจากด้านหลัง
            self.feint_attack = True
```

#### **Reinforcement Learning Setup:**

```python
# State Space (สิ่งที่ AI รับรู้)
observation = {
    "player_position": (x, y),
    "player_health": 0-100,
    "player_weapon": one_hot_encoded,
    "player_is_running": bool,
    "distance_to_player": float,
    "nearby_zombies_count": int,
    "obstacles_nearby": array,
    "time_of_day": 0-24,
    "player_panic_level": 0-100,
    "is_player_in_building": bool,
    "player_facing_direction": 0-360,
}

# Action Space (สิ่งที่ AI ทำได้)
actions = [
    "move_towards_player",
    "move_to_ambush_point",
    "call_horde",
    "break_door",
    "break_window",
    "wait_and_observe",
    "circle_player",
    "attack",
    "retreat",
    "fake_death",
]

# Reward Function
def calculate_reward(state, action, next_state):
    reward = 0

    # Positive rewards
    if distance_decreased:
        reward += 10
    if damaged_player:
        reward += 50
    if killed_player:
        reward += 100
    if successful_ambush:
        reward += 30
    if coordinated_with_horde:
        reward += 20

    # Negative rewards
    if took_damage:
        reward -= 20
    if player_escaped:
        reward -= 30
    if stuck_in_obstacle:
        reward -= 10
    if lost_track_of_player:
        reward -= 15

    return reward
```

#### **Training Process:**

```
Phase 1: Basic Behavior (1-2 weeks training)
├─ Learn to navigate
├─ Learn to chase player
└─ Learn to avoid obstacles

Phase 2: Combat Tactics (2-3 weeks)
├─ Learn optimal attack distance
├─ Learn when to retreat
└─ Learn to use environment

Phase 3: Advanced Strategy (3-4 weeks)
├─ Learn player patterns
├─ Learn to set ambushes
├─ Learn coordination with other zombies
└─ Learn to exploit player weaknesses

Phase 4: Adaptive Learning (Continuous)
├─ Update model during gameplay
├─ Personalize to each player
└─ Remember past encounters
```

### **7. WORLD & MAP SYSTEM**

#### **Map Structure:**

```
WORLD SIZE: 100x100 tiles (isometric)
TILE SIZE: 32x32 pixels
TOTAL: 3200x3200 pixels

┌─────────────────────────────────────────┐
│  🌲🌲🌲 FOREST 🌲🌲🌲                   │
│  🌲🏠🏠🏠 RESIDENTIAL AREA 🏠🏠🏠       │
│  🏠🏠🏪🏥 DOWNTOWN 🏬🏪🏠              │
│  🚗🚗🚗 STREET 🚗🚗🚗                  │
│  🏭 INDUSTRIAL ZONE 🏭                  │
│  🌲🌲 RIVER 💧💧💧 🌲🌲                │
│  🏕️ CAMPSITE 🏕️                         │
└─────────────────────────────────────────┘
```

#### **Location Types:**

```
🏠 HOUSES (40%)
├─ Kitchen: Food, water, knives
├─ Bedroom: Clothes, medical supplies
├─ Bathroom: Medicine, disinfectant
├─ Garage: Tools, weapons, car parts
└─ Basement: Canned food, storage

🏪 STORES (15%)
├─ Grocery: Food, drinks
├─ Hardware: Tools, building materials
├─ Pharmacy: Medical supplies, drugs
├─ Gun Shop: Weapons, ammo (rare)
└─ Clothing: Protective gear

🏥 HOSPITAL (2%)
├─ High medical supplies
├─ Many zombies
├─ Pharmacy (locked)
└─ Emergency generator

🏫 SCHOOL (3%)
├─ Cafeteria: Food
├─ Nurse office: Basic medical
├─ Gym: Sports equipment (weapons)
└─ Library: Books (skill learning)

🏭 WAREHOUSE (5%)
├─ Tools
├─ Building materials
├─ Industrial equipment
└─ Forklifts (vehicle)

🏕️ CAMPSITE (3%)
├─ Tents
├─ Camping supplies
├─ Fishing spots nearby
└─ Less zombies

🌲 FOREST (20%)
├─ Hunting
├─ Foraging
├─ Natural water sources
└─ Safest at night

🚗 STREETS (12%)
├─ Abandoned cars (loot, parts)
├─ Most dangerous
└─ Main zombie spawns
```

#### **Procedural Generation:**

```python
def generate_world(seed):
    """สร้างแมพแบบสุ่ม"""

    # 1. Generate base terrain
    terrain = generate_perlin_noise(seed)

    # 2. Place key locations
    place_location("hospital", count=1, min_distance=50)
    place_location("police_station", count=1)
    place_location("grocery_store", count=3)
    place_location("gun_shop", count=1)

    # 3. Generate residential areas
    for i in range(5):
        create_neighborhood(houses=8-12, size="medium")

    # 4. Create roads
    connect_locations_with_roads()

    # 5. Place random loot
    distribute_loot(rarity_curve="exponential")

    # 6. Spawn initial zombies
    spawn_zombies(density=0.02)  # 2% of map

    # 7. Add natural features
    place_forests(coverage=0.20)
    place_river(count=1)

    return World(terrain, locations, loot, zombies)
```

### **8. TIME & WEATHER SYSTEM**

#### **Day/Night Cycle:**

```
TIME SCALE: 1 real minute = 10 game minutes
DAY LENGTH: 24 hours = 144 real minutes (2.4 hours)

┌─────────────────────────────────────────┐
│ 06:00 - 08:00  🌅 DAWN                  │
│ ├─ Safest time                          │
│ ├─ Zombies slow                         │
│ └─ Good visibility                      │
│                                         │
│ 08:00 - 18:00  ☀️ DAY                   │
│ ├─ Normal zombie activity               │
│ ├─ Best looting time                    │
│ └─ Perfect visibility                   │
│                                         │
│ 18:00 - 20:00  🌆 DUSK                  │
│ ├─ Zombies becoming active              │
│ ├─ Visibility decreasing                │
│ └─ Should return to base                │
│                                         │
│ 20:00 - 06:00  🌙 NIGHT                 │
│ ├─ VERY DANGEROUS                       │
│ ├─ Zombies 2x faster                    │
│ ├─ Zombies 2x more                      │
│ ├─ Limited visibility                   │
│ └─ Need flashlight (attracts zombies)   │
└─────────────────────────────────────────┘
```

#### **Weather System:**

```
☀️  CLEAR (50%)
├─ Normal conditions
└─ No effects

⛅ CLOUDY (20%)
├─ Slightly darker
└─ Zombies see less (-10% vision)

🌧️ RAIN (15%)
├─ Reduced visibility
├─ Muffles sounds (-30% hearing)
├─ Slippery (slower movement)
└─ Can collect water

⛈️ STORM (5%)
├─ Very dark
├─ Thunder attracts zombies
├─ Heavy rain (visibility -50%)
└─ Stay indoors!

🌫️ FOG (10%)
├─ Very low visibility
├─ Zombies harder to spot
└─ Can sneak easier

❄️ WINTER (Future DLC)
├─ Freezing temperatures
├─ Need warm clothes
├─ Food spoils slower
└─ Zombies frozen (slower)
```

### **9. BASE BUILDING SYSTEM**

#### **Buildable Structures:**

```
DEFENSIVE:
🚪 Wooden Barricade
├─ HP: 200
├─ Build Time: 10 seconds
├─ Materials: Wood Plank x4, Nails x20
└─ Carpentry: Level 2

🪟 Window Boards
├─ HP: 100
├─ Build Time: 5 seconds
├─ Materials: Wood Plank x2, Nails x10
└─ Carpentry: Level 1

🧱 Reinforced Wall
├─ HP: 500
├─ Build Time: 30 seconds
├─ Materials: Bricks x10, Cement x5
└─ Carpentry: Level 5

⚠️ Spike Trap
├─ Damage: 50
├─ Build Time: 15 seconds
├─ Materials: Wood Planks x3, Nails x30
└─ Carpentry: Level 3

UTILITY:
📦 Storage Container
├─ Capacity: 100kg
├─ Build Time: 20 seconds
├─ Materials: Wood Planks x5, Nails x15
└─ Carpentry: Level 3

🛏️ Bed
├─ Sleep quality: +50%
├─ Build Time: 30 seconds
├─ Materials: Wood Planks x6, Cloth x3
└─ Carpentry: Level 4

💧 Rain Collector
├─ Collects: 5L/hour (rain)
├─ Build Time: 25 seconds
├─ Materials: Metal Sheet x2, Barrel x1
└─ Carpentry: Level 4

🌱 Farm Plot
├─ Grows food
├─ Build Time: 15 seconds
├─ Materials: Dirt, Seeds, Water
└─ Farming: Level 2

🔥 Campfire
├─ Cook food, warmth, light
├─ Build Time: 5 seconds
├─ Materials: Wood x3, Lighter
└─ None required

⚡ Generator
├─ Power for lights, fridge
├─ Fuel consumption: 1L/hour
├─ Build Time: 40 seconds
└─ Mechanics: Level 6
```

#### **Base Locations:**

```
IDEAL BASE CHECKLIST:
✅ Multiple exits
✅ Second floor (safer)
✅ Near water source
✅ Near loot spawns
✅ Defendable
✅ Has garage (vehicle storage)
✅ Kitchen/bathroom
✅ Storage space

RECOMMENDED:
🏠 Two-story House with Garage (Best)
🏫 School (Large, many rooms)
🏥 Hospital (Medical supplies, but dangerous)
🏪 Warehouse (Huge storage)
🏕️ Isolated Cabin (Safest, but far from loot)
```

### **10. SKILL PROGRESSION**

#### **XP System:**

```python
def gain_xp(skill, action):
    xp_gain = {
        "carpentry": {
            "build_barricade": 50,
            "build_wall": 100,
            "repair_structure": 25,
        },
        "cooking": {
            "cook_meal": 30,
            "cook_complex_meal": 60,
            "preserve_food": 40,
        },
        "combat": {
            "kill_zombie": 10,
            "headshot": 20,
            "melee_kill": 15,
        },
        "fitness": {
            "sprint_100m": 5,
            "carry_heavy_load": 10,
        },
        "stealth": {
            "sneak_past_zombie": 20,
            "remain_undetected_5min": 50,
        },
    }

    xp_needed = level * 100  # Level 2 = 200 XP
    ```python
    return xp_gain[skill][action]

def level_up(skill, current_level):
    """Level up และได้ perks"""
    new_level = current_level + 1

    perks = {
        "carpentry": {
            2: "Build barricades",
            3: "Build traps",
            4: "Build furniture",
            5: "Build walls",
            6: "Build second floor",
            7: "Repair faster (50%)",
            8: "Build watchtower",
            9: "Build metal structures",
            10: "Master builder - structures 2x HP"
        },
        "cooking": {
            2: "Cook meat without burning",
            3: "Make complex meals (+nutrition)",
            4: "Preserve food (lasts 2x longer)",
            5: "Cook with less fuel",
            6: "Identify poisonous food",
            7: "Make medicine from herbs",
            8: "Cook masterpieces (+morale)",
            9: "Smoke/dry meat (lasts forever)",
            10: "Master chef - food heals +50%"
        },
        "combat": {
            2: "+10% damage",
            3: "Critical hits (10% chance)",
            4: "+20% damage",
            5: "Push zombies",
            6: "+30% damage",
            7: "Critical hits (20% chance)",
            8: "Instant kill weak zombies",
            9: "+50% damage",
            10: "One-hit headshot"
        },
        "fitness": {
            2: "Sprint longer (+20%)",
            3: "Carry +10kg",
            4: "Sprint longer (+40%)",
            5: "Fatigue -20% slower",
            6: "Carry +20kg",
            7: "Jump higher/further",
            8: "Sprint longer (+60%)",
            9: "Climb faster",
            10: "Unlimited stamina sprint"
        },
        "stealth": {
            2: "Crouch walk faster",
            3: "Zombie vision -20%",
            4: "Footstep sounds -30%",
            5: "Zombie vision -40%",
            6: "Perform stealth kills",
            7: "Footstep sounds -60%",
            8: "Become invisible when crouching still",
            9: "Walk past zombies (slow)",
            10: "Master ninja - completely silent"
        }
    }

    return perks[skill][new_level]
```

---

### **11. SOUND SYSTEM** 🔊

#### **Sound Propagation:**

```python
# Sound levels (tiles)
SOUND_LEVELS = {
    # Movement
    "crouch_walk": 2,
    "walk": 5,
    "run": 15,
    "sprint": 25,

    # Actions
    "open_door_slow": 3,
    "open_door_fast": 10,
    "break_window": 25,
    "break_door": 30,
    "climb_fence": 8,

    # Combat
    "melee_swing": 5,
    "melee_hit": 8,
    "pistol": 50,
    "shotgun": 70,
    "rifle": 80,
    "explosion": 100,

    # Items
    "drop_item": 4,
    "craft": 6,
    "eat": 2,

    # Voice
    "whisper": 3,
    "talk": 10,
    "shout": 40,

    # Vehicles
    "car_engine": 60,
    "car_horn": 80,
    "crash": 90,
}

def propagate_sound(position, sound_level):
    """เสียงแพร่กระจาย ดึงดูด zombies"""

    affected_zombies = []

    for zombie in nearby_zombies:
        distance = calculate_distance(position, zombie.position)

        # ปรับตามสภาพแวดล้อม
        adjusted_range = sound_level

        if weather == "rain":
            adjusted_range *= 0.7  # ฝนลดเสียง

        if is_inside_building(position):
            adjusted_range *= 0.5  # ในอาคารเสียงดังน้อยกว่า

        if is_foggy:
            adjusted_range *= 0.8

        # Zombie ได้ยินไหม?
        if distance <= adjusted_range * zombie.hearing_multiplier:
            zombie.investigate(position)
            affected_zombies.append(zombie)

    return affected_zombies
```

#### **Music & Audio:**

```
🎵 SOUNDTRACK:
├─ Menu Theme - Melancholic 8-bit
├─ Day Exploration - Calm, tense undertones
├─ Night Theme - Dark, scary
├─ Combat - Fast-paced, intense
├─ Safe House - Relaxing, hopeful
└─ Death/Game Over - Sad, dramatic

🔊 SOUND EFFECTS:
├─ Zombie moans (varied)
├─ Footsteps (different surfaces)
├─ Door creaks
├─ Window breaks
├─ Weapon swings/shots
├─ Eating/drinking
├─ Crafting sounds
├─ Rain/thunder
└─ Heartbeat (when panic high)
```

---

### **12. UI/UX DESIGN** (8-bit Style)

#### **HUD Layout:**

```
┌─────────────────────────────────────────────┐
│ DAY 15  ☀️ 14:30  🌡️ 22°C              [?]│
├─────────────────────────────────────────────┤
│                                             │
│  ❤️  ████████░░ 80    🧟 x12 nearby       │
│  🍖 ██████░░░░ 60                          │
│  💧 ████████░░ 80    [📍 Safehouse: 50m]  │
│  😴 ████░░░░░░ 40                          │
│                                             │
│                                             │
│         [GAME WORLD - ISOMETRIC VIEW]       │
│                                             │
│                  👤                         │
│              🧟     🧟                      │
│          🌲   🏠    🌲                      │
│              🧟 🧟                          │
│                                             │
│                                             │
├─────────────────────────────────────────────┤
│ 🔫 [Baseball Bat] 70/70  🎯 Level 3        │
│ 🎒 [18/30 kg]           💊 x3  🥫 x5      │
└─────────────────────────────────────────────┘

CONTROLS:
WASD - Move
Space - Attack
E - Interact
I - Inventory
C - Crafting
B - Building
M - Map
ESC - Menu
Shift - Sprint
Ctrl - Crouch
```

#### **Menu Screens:**

```
MAIN MENU:
┌─────────────────────────────────┐
│    ___  ___  __  __  ___  ___  │
│   |_ / |  | |  \/  || _ \|_ _| │
│    / /  | |  | |\/| ||  _/ | |  │
│   /___| |__| |_|  |_||_|  |___|  │
│                                 │
│   ZOMBIE SURVIVAL 8-BIT         │
│                                 │
│   > NEW GAME                    │
│     CONTINUE                    │
│     OPTIONS                     │
│     TUTORIAL                    │
│     CREDITS                     │
│     QUIT                        │
│                                 │
│   [Music: ON]  [SFX: ON]        │
└─────────────────────────────────┘

INVENTORY:
┌─────────────────────────────────────┐
│ INVENTORY          Weight: 18/30 kg │
├─────────────────────────────────────┤
│ EQUIPPED:                           │
│ 🔫 Main: [Baseball Bat] 70/70      │
│ 🗡️  Alt:  [Kitchen Knife] 50/50    │
│                                     │
│ BACKPACK:                           │
│ [🥫] Canned Food x5                 │
│ [💊] First Aid Kit x3               │
│ [🔦] Flashlight (90% battery)       │
│ [🔋] Battery x4                     │
│ [🪵] Wood Plank x8                  │
│ [📦] Nails x50                      │
│ [🧃] Water Bottle (Full) x2         │
│ [🍞] Bread (Fresh) x1               │
│                                     │
│ [E] Use  [D] Drop  [C] Combine      │
└─────────────────────────────────────┘

CRAFTING:
┌─────────────────────────────────────┐
│ CRAFTING                            │
├─────────────────────────────────────┤
│ > Spiked Bat ⭐                     │
│   Baseball Bat + Nails x20          │
│   Carpentry 2 Required              │
│   Damage: 25 → 35                   │
│   [CRAFT]                           │
│                                     │
│   Molotov Cocktail                  │
│   Bottle + Cloth + Gas              │
│   Missing: Gas                      │
│   [---]                             │
│                                     │
│   Wooden Spear                      │
│   Wood Plank + Knife                │
│   Carpentry 1 Required              │
│   [CRAFT]                           │
└─────────────────────────────────────┘

CHARACTER STATS:
┌─────────────────────────────────────┐
│ SURVIVOR: [PLAYER NAME]             │
│ Days Survived: 15                   │
│ Zombies Killed: 127                 │
├─────────────────────────────────────┤
│ SKILLS:                             │
│ 🔨 Carpentry    ████░░░░░░ Lv.4    │
│ 🍳 Cooking      ██░░░░░░░░ Lv.2    │
│ 💪 Strength     ██████░░░░ Lv.6    │
│ 🏃 Fitness      ████░░░░░░ Lv.4    │
│ 🎯 Aiming       █████░░░░░ Lv.5    │
│ 🔧 Mechanics    ███░░░░░░░ Lv.3    │
│ 🌿 Foraging     ██░░░░░░░░ Lv.2    │
│ 🏥 First Aid    █████░░░░░ Lv.5    │
│ 🥷 Stealth      ███░░░░░░░ Lv.3    │
└─────────────────────────────────────┘
```

---

### **13. GAME PROGRESSION & DIFFICULTY**

#### **Dynamic Difficulty:**

```python
class DifficultyScaler:
    def __init__(self):
        self.day = 0
        self.player_skill = 0  # 0-100
        self.deaths = 0

    def calculate_difficulty(self):
        """ปรับความยากตามเวลาและทักษะผู้เล่น"""

        # Base difficulty increases over time
        base_difficulty = 1.0 + (self.day * 0.05)  # +5% per day

        # Adjust based on player performance
        if self.player_skill > 70:  # Player เก่ง
            difficulty_multiplier = 1.5
        elif self.player_skill < 30:  # Player อ่อน
            difficulty_multiplier = 0.7
        else:
            difficulty_multiplier = 1.0

        # Death penalty
        if self.deaths > 5:
            difficulty_multiplier *= 0.8  # ลดความยากถ้าตายบ่อย

        final_difficulty = base_difficulty * difficulty_multiplier

        return final_difficulty

    def apply_difficulty(self, difficulty):
        """นำความยากไปใช้"""

        # Zombie spawning
        zombie_spawn_rate = 0.02 * difficulty
        zombie_speed_multiplier = 1.0 + (difficulty * 0.1)
        zombie_damage_multiplier = 1.0 + (difficulty * 0.15)

        # Loot scarcity
        loot_spawn_rate = 1.0 / difficulty

        # Hunger/Thirst rate
        needs_decay_rate = 1.0 * difficulty

        # AI Intelligence
        smart_zombie_spawn_rate = 0.02 * difficulty
        ai_learning_speed = 1.0 * difficulty

        return {
            "zombie_spawn": zombie_spawn_rate,
            "zombie_speed": zombie_speed_multiplier,
            "zombie_damage": zombie_damage_multiplier,
            "loot_rate": loot_spawn_rate,
            "needs_decay": needs_decay_rate,
            "ai_spawn": smart_zombie_spawn_rate,
            "ai_learning": ai_learning_speed,
        }
```

#### **Milestones:**

```
DAY 1-3: TUTORIAL PHASE
├─ Few zombies
├─ Plenty of loot
├─ Learn mechanics
└─ Build first base

DAY 4-7: EARLY GAME
├─ Zombie density increases
├─ Must manage resources
├─ Establish routine
└─ First Smart Zombie appears

DAY 8-14: MID GAME
├─ Resources scarce
├─ Night raids common
├─ Multiple Smart Zombies
└─ Must adapt strategies

DAY 15-30: LATE GAME
├─ Survival mode
├─ Hordes frequent
├─ Smart Zombies dominant
└─ Ultimate challenge

DAY 30+: ENDGAME
├─ Only for masters
├─ Zombie AI fully adapted
├─ Extreme difficulty
└─ Legendary status
```

---

### **14. ACHIEVEMENTS & PROGRESSION**

```
🏆 SURVIVAL ACHIEVEMENTS:

First Night
└─ Survive your first night

Week Warrior
└─ Survive 7 days

Fortnight Fighter
└─ Survive 14 days

Monthly Survivor
└─ Survive 30 days

Living Legend
└─ Survive 100 days

---

🧟 COMBAT ACHIEVEMENTS:

First Blood
└─ Kill your first zombie

Zombie Slayer
└─ Kill 100 zombies

Zombie Destroyer
└─ Kill 500 zombies

Zombie Genocide
└─ Kill 1,000 zombies

Headshot Master
└─ 100 headshot kills

Melee Master
└─ Kill 50 zombies with melee

Pacifist
└─ Survive 7 days without killing

---

🏠 BASE BUILDING:

Homemaker
└─ Build your first base

Fortified
└─ Build 10 barricades

Fortress
└─ Build a completely secure base

Master Builder
└─ Build every structure type

---

🎯 SKILL ACHIEVEMENTS:

Jack of All Trades
└─ Level 5 in all skills

Master Carpenter
└─ Carpentry Level 10

Master Chef
└─ Cooking Level 10

Silent Assassin
└─ Stealth Level 10

Olympic Athlete
└─ Fitness Level 10

---

🧠 AI ACHIEVEMENTS:

Outsmarted
└─ Get killed by Smart Zombie

Adaptive Survivor
└─ Survive 3 days against Smart Zombies

AI Hunter
└─ Kill 10 Smart Zombies

Turing Test Failed
└─ Smart Zombie predicts your move

---

💀 DEATH ACHIEVEMENTS:

First Death
└─ Die for the first time

Death by Stupidity
└─ Die from starvation/thirst

Suicide Mission
└─ Die at night outside

Overwhelmed
└─ Die to horde (10+ zombies)

---

🎮 SPECIAL ACHIEVEMENTS:

No Guns Allowed
└─ Survive 30 days without firearms

Vegetarian
└─ Survive 14 days on vegetables only

Nomad
└─ Never build a base, survive 14 days

Speedrunner
└─ Reach Day 7 in under 2 real hours

Ironman
└─ Survive 30 days without loading saves
```

---

### **15. SAVE SYSTEM**

```python
class SaveSystem:
    def save_game(self):
        """บันทึกเกม"""
        save_data = {
            # World State
            "seed": world.seed,
            "day": game_time.day,
            "time": game_time.hour,
            "weather": weather.current,

            # Player
            "player": {
                "position": player.position,
                "health": player.health,
                "hunger": player.hunger,
                "thirst": player.thirst,
                "fatigue": player.fatigue,
                "infection": player.infection,
                "skills": player.skills,
                "inventory": player.inventory,
            },

            # World
            "zombies": [z.serialize() for z in zombies],
            "loot": [l.serialize() for l in remaining_loot],
            "structures": [s.serialize() for s in player_built_structures],

            # AI Learning Data
            "ai_model": smart_zombie_ai.export_model(),
            "player_behavior": player_behavior_data.export(),

            # Stats
            "stats": {
                "days_survived": stats.days,
                "zombies_killed": stats.kills,
                "distance_traveled": stats.distance,
                "items_crafted": stats.crafted,
                "deaths": stats.deaths,
            },

            # Meta
            "version": "1.0.0",
            "timestamp": datetime.now(),
        }

        # Save to file
        with open(f"save_{slot}.json", "w") as f:
            json.dump(save_data, f)

        # Auto-save every 10 minutes
        # Manual save anytime (not in combat)

    def load_game(self, slot):
        """โหลดเกม"""
        with open(f"save_{slot}.json", "r") as f:
            save_data = json.load(f)

        # Restore everything
        restore_world(save_data)
        restore_player(save_data["player"])
        restore_zombies(save_data["zombies"])
        restore_ai_model(save_data["ai_model"])

        return True
```

---

## 🛠️ **DEVELOPMENT ROADMAP**

### **PHASE 1: FOUNDATION (Weeks 1-4)**

#### Week 1: Project Setup & Core Movement

```
✅ Setup Unity Project
├─ Install Unity 2022.3 LTS
├─ Create project structure
├─ Setup version control (Git)
└─ Install required packages

✅ Isometric System
├─ Create isometric tilemap
├─ Setup isometric camera
├─ Implement 8-directional movement
└─ Character controller

✅ Basic Graphics
├─ Create character sprite (32x32)
├─ Create tile sprites
├─ Basic animations (walk, idle)
└─ Camera follow system

DELIVERABLE: Character can move on isometric map
```

#### Week 2: World & Interaction

```
✅ World Building
├─ Procedural map generation
├─ Place buildings (houses, stores)
├─ Add props (trees, cars)
└─ Collision system

✅ Interaction System
├─ Open/close doors
├─ Search containers
├─ Pick up items
└─ UI prompts

✅ Day/Night Cycle
├─ Time system (24 hours)
├─ Lighting changes
├─ UI clock display
└─ Visual transitions

DELIVERABLE: Explorable world with interactions
```

#### Week 3: Inventory & Items

```
✅ Inventory System
├─ Inventory data structure
├─ Inventory UI
├─ Item pickup/drop
├─ Weight system
└─ Equipment slots

✅ Items
├─ Create item database
├─ Food items
├─ Weapons (melee)
├─ Tools
└─ Medical items

✅ Using Items
├─ Eat food
├─ Use medical items
├─ Equip weapons
└─ Item tooltips

DELIVERABLE: Full inventory system working
```

#### Week 4: Survival Stats & Combat

```
✅ Survival Mechanics
├─ Health system
├─ Hunger/Thirst system
├─ Fatigue system
├─ Stats UI display
└─ Death system

✅ Basic Combat
├─ Melee attack
├─ Weapon damage
├─ Hit detection
├─ Combat animations
└─ Damage numbers

✅ Polish
├─ Sound effects
├─ Particle effects
├─ Screen shake
└─ Feedback improvements

DELIVERABLE: Playable survival game (no AI yet)
```

---

### **PHASE 2: ZOMBIE AI (Weeks 5-8)**

#### Week 5: Basic Zombie AI

```
✅ Zombie Creation
├─ Zombie sprites & animations
├─ Zombie stats (HP, speed, damage)
├─ Zombie types (walker, runner)
└─ Spawn system

✅ Pathfinding
├─ A* pathfinding implementation
├─ NavMesh setup
├─ Obstacle avoidance
└─ Path optimization

✅ Basic Behavior
├─ Idle wandering
├─ Chase player when seen
├─ Attack when in range
└─ Sound detection

DELIVERABLE: Basic zombie AI working
```

#### Week 6: Advanced Zombie Behavior

```
✅ Senses System
├─ Vision system (line of sight)
├─ Hearing system (sound propagation)
├─ Smell system (blood tracking)
└─ Memory (last seen position)

✅ State Machine
├─ Idle state
├─ Investigating state
├─ Chasing state
├─ Attacking state
└─ Eating state

✅ Horde Behavior
├─ Zombie grouping
├─ Follow the leader
├─ Breaking doors/windows
└─ Horde spawning

DELIVERABLE: Smart traditional zombie AI
```

#### Week 7-8: ML-Agents Setup & Training Prep

```
✅ ML-Agents Installation
├─ Install Python 3.10
├─ Install ML-Agents package
├─ Setup Unity ML-Agents package
└─ Configure training environment

✅ Training Environment
├─ Create training scene
├─ Smart Zombie agent script
├─ Observation space setup
├─ Action space setup
└─ Reward function implementation

✅ Data Collection System
├─ Player behavior tracker
├─ Data logging system
├─ CSV export functionality
└─ Visualization tools

DELIVERABLE: Ready to train AI zombies
```

---

### **PHASE 3: AI TRAINING & INTEGRATION (Weeks 9-12)**

#### Week 9-10: Initial Training

```
✅ Training Configuration
├─ hyperparameters tuning
├─ Training curriculum
├─ Reward shaping
└─ Training monitoring

🎓 Training Phase 1: Navigation
├─ Learn to move efficiently
├─ Learn to avoid obstacles
├─ Learn to chase moving target
└─ Train for 50K episodes

🎓 Training Phase 2: Combat
├─ Learn attack timing
├─ Learn to dodge
├─ Learn when to retreat
└─ Train for 100K episodes

DELIVERABLE: Trained AI model (basic)
```

#### Week 11: Advanced AI Training

```
🎓 Training Phase 3: Strategy
├─ Learn player patterns
├─ Learn to set ambushes
├─ Learn coordination
└─ Train for 150K episodes

✅ Model Optimization
├─ Model compression
├─ Inference optimization
├─ Performance testing
└─ Bug fixing

✅ Integration
├─ Import trained model to Unity
├─ Smart Zombie integration
├─ Spawn balancing
└─ Difficulty tuning

DELIVERABLE: Smart AI zombies in game
```

#### Week 12: Adaptive Learning

```
✅ Online Learning System
├─ Real-time data collection
├─ Model update mechanism
├─ Player profiling
└─ Personalized difficulty

✅ AI Behaviors
├─ Ambush tactics
├─ Flanking maneuvers
├─ Base sieging
└─ Trap setting

✅ Balance & Testing
├─ Playtest sessions
├─ AI behavior tuning
├─ Difficulty balancing
└─ Bug fixes

DELIVERABLE: Fully adaptive AI system
```

---

### **PHASE 4: CONTENT & POLISH (Weeks 13-16)**

#### Week 13: Content Creation

```
✅ More Items
├─ 20+ weapon types
├─ 30+ food items
├─ 15+ medical items
├─ 25+ crafting materials
└─ Item sprites & data

✅ More Locations
├─ 5 unique building types
├─ Interior designs
├─ Loot tables
└─ Special locations

✅ Crafting Recipes
├─ 30+ craftable items
├─ Crafting UI
├─ Recipe discovery
└─ Crafting animations

DELIVERABLE: Rich content variety
```

#### Week 14: Base Building & Systems

```
✅ Building System
├─ 15+ buildable structures
├─ Building placement
├─ Building UI/UX
└─ Structure HP/durability

✅ Advanced Systems
├─ Farming system
├─ Water collection
├─ Power generation
└─ Food preservation

✅ Skills & Progression
├─ Skill leveling
├─ Perks system
├─ Skill UI
└─ Balance skills

DELIVERABLE: Full survival sandbox
```

#### Week 15: Audio & Visual Polish

```
✅ Audio
├─ Background music (5 tracks)
├─ Sound effects (100+)
├─ Ambient sounds
└─ Audio mixing

✅ Visual Effects
├─ Particle effects
├─ Screen effects
├─ Weather effects
└─ Lighting improvements

✅ Animations
├─ Character animations (20+)
├─ Zombie animations (15+)
├─ Item animations
└─ Environmental animations

DELIVERABLE: Polished audiovisual experience
```

#### Week 16: UI/UX & Final Polish

```
✅ UI Overhaul
├─ Main menu
├─ HUD redesign
├─ Inventory UI polish
├─ Settings menu
└─ Tutorial system

✅ Quality of Life
├─ Tooltips everywhere
├─ Keyboard shortcuts
├─ Quick actions
├─ Auto-save
└─ Multiple save slots

✅ Optimization
├─ Performance profiling
├─ Memory optimization
├─ Load time reduction
└─ FPS improvements

✅ Final Testing
├─ Bug hunting
├─ Balance testing
├─ Playtest feedback
└─ Final fixes

DELIVERABLE: Release-ready game!
```

---

### **PHASE 5: LAUNCH & POST-LAUNCH (Week 17+)**

```
✅ Launch Preparation
├─ Build for Windows/Mac/Linux
├─ Create game page (itch.io/Steam)
├─ Marketing materials
├─ Trailer video
└─ Press kit

🚀 LAUNCH!
├─ Release on itch.io (free/paid)
├─ Community building
├─ Gather feedback
└─ Monitor analytics

📊 Post-Launch Support
├─ Bug fixes (ongoing)
├─ Balance patches
├─ Community feedback implementation
└─ Performance improvements

🎮 Future Updates (DLC ideas)
├─ Multiplayer co-op
├─ New maps
├─ New zombie types
├─ Seasonal events
├─ Winter survival
└─ Story mode
```

---

## 💻 **TECHNICAL ARCHITECTURE**

### **Project Structure:**

```
ZombieSurvival8bit/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── TimeManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   └── AudioManager.cs
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerStats.cs
│   │   │   ├── PlayerInventory.cs
│   │   │   └── PlayerCombat.cs
│   │   ├── AI/
│   │   │   ├── ZombieAI.cs
│   │   │   ├── SmartZombieAgent.cs (ML-Agents)
│   │   │   ├── ZombieSpawner.cs
│   │   │   ├── ZombieSenses.cs
│   │   │   └── BehaviorTracker.cs
│   │   ├── World/
│   │   │   ├── WorldGenerator.cs
│   │   │   ├── BuildingGenerator.cs
│   │   │   ├── LootSpawner.cs
│   │   │   └── WeatherSystem.cs
│   │   ├── Systems/
│   │   │   ├── InventorySystem.cs
│   │   │   ├── CraftingSystem.cs
│   │   │   ├── BuildingSystem.cs
│   │   │   ├── SkillSystem.cs
│   │   │   └── SoundSystem.cs
│   │   └── UI/
│   │       ├── HUDManager.cs
│   │       ├── InventoryUI.cs
│   │       ├── CraftingUI.cs
│   │       └── MenuUI.cs
│   ├── Sprites/
│   │   ├── Characters/
│   │   ├── Zombies/
│   │   ├── Items/
│   │   ├── Tiles/
│   │   └── UI/
│   ├── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── MainMenu
│   │   ├── Game
│   │   └── Training (ML-Agents)
│   └── ML-Agents/
│       ├── Config/
│       │   └── zombie_training.yaml
│       └── Models/
│           └── smart_zombie.onnx
├── Python/ (ML Training)
│   ├── train.py
│   ├── config.yaml
│   └── analysis/
│       ├── visualize_data.py
│       └── behavior_analysis.py
└── Builds/
    ├── Windows/
    ├── Mac/
    └── Linux/
```

---

### **Key Scripts Overview:**

```csharp
// SmartZombieAgent.cs - Main ML-Agents script
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SmartZombieAgent : Agent
{
    public override void CollectObservations(VectorSensor sensor)
    {
        // Player info
        sensor.AddObservation(playerPosition);
        sensor.AddObservation(playerHealth / 100f);
        sensor.AddObservation(playerWeapon);
        sensor.AddObservation(playerIsRunning);

        // Self info
        sensor.AddObservation(transform.position);
        sensor.AddObservation(health / maxHealth);
        sensor.AddObservation(distanceToPlayer);

        // Environment
        sensor.AddObservation(timeOfDay / 24f);
        sensor.AddObservation(nearbyZombiesCount);
        sensor.AddObservation(isPlayerInBuilding);

        // Raycast perception (8 directions)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f);
            sensor.AddObservation(hit.collider != null ? hit.distance / 10f : 1f);
        }
    }

    // Actions
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions
        int moveAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1];
        int specialAction = actions.DiscreteActions[2];

        // Movement (9 options: 8 directions + stay)
        Vector2 move = Vector2.zero;
        switch(moveAction)
        {
            case 0: move = Vector2.up; break;
            case 1: move = new Vector2(1, 1).normalized; break;
            case 2: move = Vector2.right; break;
            case 3: move = new Vector2(1, -1).normalized; break;
            case 4: move = Vector2.down; break;
            case 5: move = new Vector2(-1, -1).normalized; break;
            case 6: move = Vector2.left; break;
            case 7: move = new Vector2(-1, 1).normalized; break;
            case 8: move = Vector2.zero; break; // Stay still
        }

        rb.velocity = move * moveSpeed;

        // Attack
        if (attackAction == 1 && canAttack)
        {
            Attack();
        }

        // Special actions
        switch(specialAction)
        {
            case 0: break; // Nothing
            case 1: CallReinforcements(); break;
            case 2: BreakDoor(); break;
            case 3: HideAndAmbush(); break;
        }

        // Rewards
        CalculateRewards();
    }

    void CalculateRewards()
    {
        // Distance-based reward
        float newDistance = Vector2.Distance(transform.position, playerPosition);
        if (newDistance < lastDistance)
            AddReward(0.01f); // Getting closer
        else
            AddReward(-0.005f); // Getting farther
        lastDistance = newDistance;

        // Time penalty (encourage efficiency)
        AddReward(-0.001f);

        // Collision penalty
        if (isStuck)
            AddReward(-0.1f);
    }

    // Events
    void OnPlayerDamaged(float damage)
    {
        AddReward(damage / 10f); // +5.0 for 50 damage
    }

    void OnPlayerKilled()
    {
        AddReward(10f); // Big reward!
        EndEpisode();
    }

    void OnTookDamage(float damage)
    {
        AddReward(-damage / 20f); // -1.0 for 20 damage
    }

    void OnDeath()
    {
        AddReward(-5f); // Death penalty
        EndEpisode();
    }
}
```csharp
    public override void CollectObservations(VectorSensor sensor)
    {
        // Player info
        sensor.AddObservation(playerPosition);
        sensor.AddObservation(playerHealth / 100f);
        sensor.AddObservation(playerWeapon);
        sensor.AddObservation(playerIsRunning);

        // Self info
        sensor.AddObservation(transform.position);
        sensor.AddObservation(health / maxHealth);
        sensor.AddObservation(distanceToPlayer);

        // Environment
        sensor.AddObservation(timeOfDay / 24f);
        sensor.AddObservation(nearbyZombiesCount);
        sensor.AddObservation(isPlayerInBuilding);

        // Raycast perception (8 directions)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f);
            sensor.AddObservation(hit.collider != null ? hit.distance / 10f : 1f);
        }
    }

    // Actions
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions
        int moveAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1];
        int specialAction = actions.DiscreteActions[2];

        // Movement (9 options: 8 directions + stay)
        Vector2 move = Vector2.zero;
        switch(moveAction)
        {
            case 0: move = Vector2.up; break;
            case 1: move = new Vector2(1, 1).normalized; break;
            case 2: move = Vector2.right; break;
            case 3: move = new Vector2(1, -1).normalized; break;
            case 4: move = Vector2.down; break;
            case 5: move = new Vector2(-1, -1).normalized; break;
            case 6: move = Vector2.left; break;
            case 7: move = new Vector2(-1, 1).normalized; break;
            case 8: move = Vector2.zero; break; // Stay still
        }

        rb.velocity = move * moveSpeed;

        // Attack
        if (attackAction == 1 && canAttack)
        {
            Attack();
        }

        // Special actions
        switch(specialAction)
        {
            case 0: break; // Nothing
            case 1: CallReinforcements(); break;
            case 2: BreakDoor(); break;
            case 3: HideAndAmbush(); break;
        }

        // Rewards
        CalculateRewards();
    }

    void CalculateRewards()
    {
        // Distance-based reward
        float newDistance = Vector2.Distance(transform.position, playerPosition);
        if (newDistance < lastDistance)
            AddReward(0.01f); // Getting closer
        else
            AddReward(-0.005f); // Getting farther
        lastDistance = newDistance;

        // Time penalty (encourage efficiency)
        AddReward(-0.001f);

        // Collision penalty
        if (isStuck)
            AddReward(-0.1f);
    }

    // Events
    void OnPlayerDamaged(float damage)
    {
        AddReward(damage / 10f); // +5.0 for 50 damage
    }

    void OnPlayerKilled()
    {
        AddReward(10f); // Big reward!
        EndEpisode();
    }

    void OnTookDamage(float damage)
    {
        AddReward(-damage / 20f); // -1.0 for 20 damage
    }

    void OnDeath()
    {
        AddReward(-5f); // Death penalty
        EndEpisode();
    }
}
```

```csharp
// BehaviorTracker.cs - Track player behavior for AI learning
public class BehaviorTracker : MonoBehaviour
{
    private PlayerBehaviorData data;
    private List<Vector2> positionHistory;
    private List<string> actionHistory;

    void Start()
    {
        data = new PlayerBehaviorData();
        StartCoroutine(TrackBehavior());
    }

    IEnumerator TrackBehavior()
    {
        while (true)
        {
            // Track every second
            yield return new WaitForSeconds(1f);

            // Record position
            positionHistory.Add(player.position);

            // Detect patterns every 5 minutes
            if (Time.time % 300 < 1f)
            {
                AnalyzePatterns();
            }
        }
    }

    void AnalyzePatterns()
    {
        // Find favorite locations (clustering)
        data.favoriteLocations = FindClusters(positionHistory);

        // Find common paths
        data.commonPaths = AnalyzePaths(positionHistory);

        // Active hours
        data.activeHours = GetActiveHours();

        // Combat patterns
        data.weaponPreference = GetMostUsedWeapon();
        data.fightOrFlightThreshold = CalculateThreshold();

        // Export data for AI
        ExportToJSON();
    }

    void ExportToJSON()
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText("player_behavior.json", json);

        // Also send to ML-Agents for online learning
        SmartZombieAgent.UpdateBehaviorModel(data);
    }
}
```

```csharp
// ZombieSpawner.cs - Intelligent spawning system
public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] zombiePrefabs;
    [SerializeField] private float baseSpawnRate = 0.02f;

    void Update()
    {
        float difficulty = DifficultyManager.Instance.GetCurrentDifficulty();
        float spawnRate = baseSpawnRate * difficulty;

        if (Random.value < spawnRate * Time.deltaTime)
        {
            SpawnZombie();
        }
    }

    void SpawnZombie()
    {
        // Choose zombie type based on difficulty
        GameObject zombiePrefab = ChooseZombieType();

        // Spawn away from player
        Vector2 spawnPos = GetSpawnPosition();

        Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
    }

    GameObject ChooseZombieType()
    {
        float difficulty = DifficultyManager.Instance.GetCurrentDifficulty();
        float smartChance = Mathf.Min(0.02f * difficulty, 0.15f); // Max 15%

        if (Random.value < smartChance)
        {
            return zombiePrefabs[4]; // Smart Zombie
        }
        else
        {
            // Regular zombie types
            float r = Random.value;
            if (r < 0.60f) return zombiePrefabs[0]; // Walker
            else if (r < 0.85f) return zombiePrefabs[1]; // Runner
            else if (r < 0.93f) return zombiePrefabs[2]; // Screamer
            else return zombiePrefabs[3]; // Tank
        }
    }

    Vector2 GetSpawnPosition()
    {
        Vector2 playerPos = PlayerController.Instance.transform.position;

        // Spawn 20-40 tiles away
        float distance = Random.Range(20f, 40f);
        float angle = Random.Range(0f, 360f);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * distance;

        return playerPos + offset;
    }
}
```

```csharp
// CraftingSystem.cs - Crafting implementation
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;

    private Dictionary<string, Recipe> recipes;

    void Awake()
    {
        Instance = this;
        LoadRecipes();
    }

    void LoadRecipes()
    {
        recipes = new Dictionary<string, Recipe>();

        // Load from JSON or define here
        recipes.Add("spiked_bat", new Recipe
        {
            name = "Spiked Bat",
            ingredients = new List<Ingredient>
            {
                new Ingredient("baseball_bat", 1),
                new Ingredient("nails", 20)
            },
            result = "spiked_bat",
            skillRequired = "carpentry",
            skillLevel = 2,
            craftTime = 10f
        });

        // Add more recipes...
    }

    public bool CanCraft(string recipeId)
    {
        Recipe recipe = recipes[recipeId];

        // Check ingredients
        foreach (Ingredient ing in recipe.ingredients)
        {
            if (!PlayerInventory.Instance.HasItem(ing.itemId, ing.amount))
                return false;
        }

        // Check skill
        if (PlayerStats.Instance.GetSkillLevel(recipe.skillRequired) < recipe.skillLevel)
            return false;

        return true;
    }

    public IEnumerator Craft(string recipeId)
    {
        Recipe recipe = recipes[recipeId];

        // Remove ingredients
        foreach (Ingredient ing in recipe.ingredients)
        {
            PlayerInventory.Instance.RemoveItem(ing.itemId, ing.amount);
        }

        // Crafting time
        float timer = 0f;
        while (timer < recipe.craftTime)
        {
            timer += Time.deltaTime;
            CraftingUI.Instance.UpdateProgress(timer / recipe.craftTime);
            yield return null;
        }

        // Add result
        PlayerInventory.Instance.AddItem(recipe.result, 1);

        // Gain XP
        PlayerStats.Instance.GainXP(recipe.skillRequired, 50);

        // Sound & effect
        AudioManager.Instance.Play("craft_success");
    }
}
```

---

## 📊 **ML-AGENTS TRAINING CONFIGURATION**

### **training_config.yaml**

```yaml
behaviors:
  SmartZombie:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 0.0003
      beta: 0.005
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
      curiosity:
        gamma: 0.99
        strength: 0.01
        encoding_size: 256
        learning_rate: 0.0003
    keep_checkpoints: 5
    max_steps: 5000000
    time_horizon: 128
    summary_freq: 10000
    threaded: true
```

### **Training Command:**

```bash
# Start training
mlagents-learn config/zombie_training.yaml --run-id=zombie_v1

# Resume training
mlagents-learn config/zombie_training.yaml --run-id=zombie_v1 --resume

# Multiple environments (faster training)
mlagents-learn config/zombie_training.yaml --run-id=zombie_v1 --num-envs=8

# With Tensorboard monitoring
tensorboard --logdir results/
```

### **Training Phases:**

```python
# Phase 1: Basic Navigation (Week 9)
# Curriculum: Easy environment, few obstacles
max_steps: 500000
reward_weights:
    distance_to_player: 1.0
    collision: -0.5
    damage_dealt: 2.0

# Phase 2: Combat (Week 10)
# Curriculum: Add player attacks
max_steps: 1000000
reward_weights:
    distance_to_player: 1.0
    collision: -0.5
    damage_dealt: 3.0
    damage_taken: -2.0
    player_killed: 10.0

# Phase 3: Advanced Strategy (Week 11)
# Curriculum: Complex environment, multiple players
max_steps: 2000000
reward_weights:
    distance_to_player: 0.5
    collision: -0.5
    damage_dealt: 3.0
    damage_taken: -2.0
    player_killed: 10.0
    ambush_success: 5.0
    coordination_bonus: 2.0
```

---

## 🎨 **ART PRODUCTION GUIDE**

### **Pixel Art Specifications:**

```
CHARACTER SPRITES:
- Size: 32x32 pixels
- Frames: 4-8 per animation
- Colors: 8-16 colors per sprite
- Export: PNG with transparency

ANIMATIONS NEEDED:
Player:
├─ Idle (4 frames) x 4 directions
├─ Walk (8 frames) x 4 directions
├─ Run (8 frames) x 4 directions
├─ Attack (6 frames) x 4 directions
├─ Die (6 frames)
└─ Hurt (2 frames)

Zombies:
├─ Idle (4 frames) x 4 directions
├─ Walk (8 frames) x 4 directions
├─ Run (8 frames) x 4 directions
├─ Attack (6 frames) x 4 directions
├─ Die (8 frames)
└─ Eating (8 frames)

TILE SPRITES:
- Size: 32x32 pixels (isometric)
- Grass, dirt, road, floor tiles
- Walls (4 directions + corners)
- Doors, windows
- Props (furniture, trees, cars)

ITEMS:
- Size: 16x16 pixels
- All weapons, food, items
- Consistent style
- Clear silhouettes

UI ELEMENTS:
- 8-bit font (monospace recommended)
- Buttons, panels
- Icons (16x16)
- Health bars, progress bars
```

### **Color Palette (Recommended):**

```
MAIN PALETTE (16 colors):
#1a1c2c (Dark Blue - night sky)
#5d275d (Dark Purple - shadows)
#b13e53 (Red - blood, danger)
#ef7d57 (Orange - fire, sunset)
#ffcd75 (Yellow - light, day)
#a7f070 (Green - grass, health)
#38b764 (Dark Green - trees)
#257179 (Teal - water)
#29366f (Blue - buildings)
#3b5dc9 (Bright Blue - sky)
#41a6f6 (Light Blue - UI)
#73eff7 (Cyan - highlights)
#f4f4f4 (White - text)
#94b0c2 (Light Gray - concrete)
#566c86 (Gray - metal)
#333c57 (Dark Gray - asphalt)

ZOMBIE PALETTE:
#5d7e5c (Zombie Green)
#3a5639 (Dark Zombie)
#8b9a8b (Gray Zombie)
#8b4c4c (Blood/Damage)
```

---

## 🎵 **AUDIO PRODUCTION GUIDE**

### **Music Tracks Needed:**

```
1. MAIN MENU THEME (2:00 loop)
   - Melancholic, mysterious
   - Slow tempo (80 BPM)
   - Instruments: 8-bit synth, bass

2. DAY EXPLORATION (3:00 loop)
   - Calm but tense
   - Medium tempo (100 BPM)
   - Subtle danger undertones

3. NIGHT THEME (3:00 loop)
   - Dark, scary
   - Slow tempo (70 BPM)
   - Heavy bass, dissonant

4. COMBAT MUSIC (2:00 loop)
   - Fast, intense
   - Fast tempo (140 BPM)
   - Driving beat

5. SAFE HOUSE (2:30 loop)
   - Peaceful, hopeful
   - Slow tempo (85 BPM)
   - Warm tones

6. GAME OVER (0:30 one-shot)
   - Sad, dramatic
   - Descending melody
```

### **Sound Effects Needed (100+):**

```
PLAYER SOUNDS:
- Footsteps (grass, concrete, wood, metal)
- Breathing (normal, tired, panicked)
- Eating, drinking
- Item pickup, drop
- Door open/close
- Window break
- Hurt sounds (male/female)
- Death sound

ZOMBIE SOUNDS:
- Moan (various pitches)
- Growl
- Attack grunt
- Eating sounds
- Footsteps (dragging)
- Death sound

COMBAT:
- Melee swing (whoosh)
- Melee hit (flesh, bone)
- Pistol shot
- Shotgun blast
- Rifle shot
- Explosion
- Reload sounds

CRAFTING/BUILDING:
- Hammer sounds
- Saw sounds
- Nails being hammered
- Item assembly

ENVIRONMENT:
- Wind
- Rain (light, heavy)
- Thunder
- Fire crackling
- Car engine
- Bird chirps (day)
- Cricket sounds (night)

UI:
- Button click
- Menu open/close
- Item equip
- Level up
- Achievement unlock
- Error sound
```

### **Audio Tools (Free/Paid):**

```
Music:
✅ BeepBox (free, browser-based)
✅ FamiTracker (free, NES-style)
✅ FL Studio (paid, professional)

Sound Effects:
✅ Bfxr (free, retro SFX generator)
✅ ChipTone (free, 8-bit sounds)
✅ Audacity (free, audio editor)

Mixing:
✅ Reaper (cheap, full DAW)
✅ FMOD/Wwise (free for indie)
```

---

## 📈 **ANALYTICS & METRICS**

### **Data to Track:**

```csharp
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    // Survival Stats
    private int dayssurvived;
    private int totalDeaths;
    private float longestRun;
    private float totalPlayTime;

    // Combat Stats
    private int zombiesKilled;
    private int headshots;
    private int meleeKills;
    private int rangedKills;
    private int damageTaken;
    private int damageDealt;

    // Resource Stats
    private int itemsCrafted;
    private int itemsLooted;
    private int foodEaten;
    private int waterDrank;
    private int structuresBuilt;

    // AI Performance
    private int smartZombieEncounters;
    private int smartZombieKills;
    private int deathsBySmartZombie;
    private float avgSmartZombieLifetime;

    // Player Behavior
    private Dictionary<string, int> locationVisits;
    private Dictionary<int, int> activeHoursSplit;
    private Dictionary<string, int> weaponUsage;
    private List<string> causeOfDeath;

    public void TrackEvent(string eventName, Dictionary<string, object> parameters)
    {
        // Log to file
        string json = JsonUtility.ToJson(new {
            timestamp = DateTime.Now,
            eventName = eventName,
            parameters = parameters
        });

        File.AppendAllText("analytics.json", json + "\n");

        // Send to server (optional)
        // SendToServer(json);
    }

    public void GenerateReport()
    {
        StringBuilder report = new StringBuilder();

        report.AppendLine("=== GAME ANALYTICS REPORT ===");
        report.AppendLine($"Total Play Time: {FormatTime(totalPlayTime)}");
        report.AppendLine($"Days Survived (Best): {longestRun}");
        report.AppendLine($"Total Deaths: {totalDeaths}");
        report.AppendLine($"Zombies Killed: {zombiesKilled}");
        report.AppendLine($"Kill/Death Ratio: {(float)zombiesKilled / totalDeaths:F2}");
        report.AppendLine();

        report.AppendLine("=== MOST VISITED LOCATIONS ===");
        foreach (var loc in locationVisits.OrderByDescending(x => x.Value).Take(5))
        {
            report.AppendLine($"{loc.Key}: {loc.Value} visits");
        }
        report.AppendLine();

        report.AppendLine("=== MOST USED WEAPONS ===");
        foreach (var weapon in weaponUsage.OrderByDescending(x => x.Value).Take(5))
        {
            report.AppendLine($"{weapon.Key}: {weapon.Value} kills");
        }
        report.AppendLine();

        report.AppendLine("=== CAUSE OF DEATH ===");
        var deathCounts = causeOfDeath.GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .Take(5);
        foreach (var cause in deathCounts)
        {
            report.AppendLine($"{cause.Key}: {cause.Count()} times");
        }

        File.WriteAllText("game_report.txt", report.ToString());
    }
}
```

---

## 🎯 **SUCCESS METRICS**

### **MVP (Minimum Viable Product) Goals:**

```
✅ Core Gameplay:
- Player can move, interact, collect items
- Survival stats work (hunger, thirst, health)
- Combat works (kill zombies)
- Day/night cycle
- Death and respawn

✅ Basic AI:
- Zombies spawn and chase player
- Pathfinding works
- Zombies can attack and kill player

✅ Content:
- At least 1 map
- 10 item types
- 3 weapon types
- 5 craftable items

✅ Polish:
- Basic UI works
- Sound effects present
- No game-breaking bugs
```

### **Full Release Goals:**

```
✅ Advanced Gameplay:
- Full survival systems
- Base building complete
- Skill progression
- Multiple game modes

✅ Smart AI:
- ML-Agents trained
- Adaptive difficulty
- Player behavior learning
- Strategic zombie behaviors

✅ Rich Content:
- 100+ items
- 30+ craftable recipes
- Multiple maps/scenarios
- 5+ zombie types

✅ Professional Polish:
- Full soundtrack
- 100+ sound effects
- Polished UI/UX
- Tutorial system
- Achievements

✅ Technical:
- Optimized performance (60 FPS)
- Save/load system
- Settings menu
- Controller support
```

---

## 💰 **BUDGET & RESOURCES**

### **Free Resources:**

```
✅ Software (FREE):
- Unity Personal (free)
- Visual Studio Code (free)
- Aseprite alternative: Piskel (free)
- GIMP (free image editor)
- Audacity (free audio editor)
- Bfxr (free SFX generator)

✅ Assets (FREE):
- itch.io (free pixel art)
- OpenGameArt.org
- Kenney.nl assets
- Freesound.org (sounds)

✅ Learning (FREE):
- Unity Learn
- YouTube tutorials
- ML-Agents documentation
- Community forums
```

### **Paid Options (Optional):**

```
💰 Recommended Purchases:
- Aseprite ($19.99) - Best pixel art tool
- Unity Plus ($40/month) - More features, optional
- Asset packs ($5-30) - Speed up development

💰 Optional:
- Sound effects pack ($10-50)
- Music composer ($100-500 for full soundtrack)
- Play tester (free friends or paid $20-50)
```

### **Time Investment:**

```
SOLO DEVELOPER:
- 10-15 hours/week = 6 months
- 20-30 hours/week = 3-4 months
- Full time = 2-3 months

SMALL TEAM (2-3 people):
- Part time = 2-3 months
- Full time = 1-2 months
```

---

## 🚀 **LAUNCH STRATEGY**

### **Pre-Launch (2 weeks before):**

```
✅ Marketing Materials:
- Game trailer (1-2 minutes)
- Screenshots (10+)
- GIFs for social media
- Press kit

✅ Social Media:
- Twitter/X posts
- Reddit (r/gamedev, r/indiegames)
- TikTok clips
- YouTube devlog

✅ Community:
- Discord server
- Mailing list
- Dev blog

✅ Press:
- Contact gaming journalists
- Submit to indie game sites
- Press release
```

### **Launch Platforms:**

```
🎮 itch.io (Recommended first):
- Easy upload
- Flexible pricing (free/paid/donation)
- Great for feedback
- 10% revenue share

🎮 Steam (Later):
- $100 submission fee
- Larger audience
- More professional
- 30% revenue share

🎮 Epic Games Store:
- No submission fee
- 12% revenue share
- Harder to get accepted

🎮 Game Jolt:
- Free hosting
- Similar to itch.io
```

### **Pricing Strategy:**

```
Option 1: FREE + Donations
- Build community first
- Get feedback
- Generate buzz
- Ask for donations

Option 2: Early Access ($5-10)
- Fund development
- Engaged players
- Iterative updates

Option 3: Full Release ($10-15)
- After full polish
- Complete game
- Better reviews
```

---

## 📝 **FINAL CHECKLIST**

```
BEFORE YOU START:
☐ Unity installed
☐ IDE installed (VS Code/Visual Studio)
☐ Python installed (for ML later)
☐ Git/version control setup
☐ Backup system ready

WEEK 1 DELIVERABLE:
☐ Character moves on isometric grid
☐ Camera follows character
☐ Basic tilemap created
☐ Collision detection works

PHASE 1 COMPLETE (Month 1):
☐ Movement, inventory, survival stats work
☐ Can interact with world
☐ Game loop functional
☐ Death and respawn working

PHASE 2 COMPLETE (Month 2):
☐ Basic zombie AI working
☐ Zombies chase and attack player
☐ Player can fight back
☐ Horde spawning works

PHASE 3 COMPLETE (Month 3):
☐ ML-Agents integrated
☐ Smart zombies trained
☐ Adaptive difficulty working
☐ AI learns player behavior

PHASE 4 COMPLETE (Month 4):
☐ All content added
☐ Audio and visuals polished
☐ UI/UX complete
☐ Major bugs fixed

READY FOR RELEASE:
☐ Full play test completed
☐ No critical bugs
☐ Tutorial works
☐ Trailer made
☐ Store page ready
☐ Marketing materials ready

POST-LAUNCH:
☐ Monitor feedback
☐ Fix bugs quickly
☐ Plan updates
☐ Engage community
```

---

## 🎓 **LEARNING RESOURCES**

### **Unity Basics:**

```
📺 YouTube Channels:
- Brackeys (RIP but archived)
- Code Monkey
- Jason Weimann
- Hardkorn Studio (Thai)

📚 Courses:
- Unity Learn (free official)
- Udemy Unity courses
- Coursera Game Development

📖 Documentation:
- Unity Manual
- Unity Scripting API
```

### **ML-Agents:**

```
📚 Official:
- ML-Agents Documentation
- ML-Agents GitHub Examples
- Unity ML-Agents Course

📺 Tutorials:
- Immersive Limit (YouTube)
- Code Monkey ML-Agents videos
- Two Minute Papers (theory)

📖 Books:
- "Hands-On Machine Learning with ML-Agents"
- "Unity Machine Learning Agents"
```

### **Game Design:**

```
📚 Recommended Reading:
- "The Art of Game Design" - Jesse Schell
- "Game Feel" - Steve Swink
- "Rules of Play" - Katie Salen

🎮 Study These Games:
- Project Zomboid (main inspiration)
- Vampire Survivors (addictive loop)
- Don't Starve (survival mechanics)
- Dead Cells (roguelike elements)
```

---

## 🤝 **GETTING HELP**

```
💬 Communities:
- Unity Forum
- Reddit r/Unity3D
- Reddit r/gamedev
- Unity Discord
- ML-Agents Discord

🐛 Bug Help:
- Stack Overflow
- Unity Answers
- GitHub Issues (ML-Agents)

🎨 Art Help:
- Pixelation Forum
- Lospec (palettes)
- OpenGameArt Forum

🎵 Audio Help:
- r/gameaudio
- Sound Design Stack Exchange
```

---

## 🎉 **FINAL THOUGHTS**

```
Remember:
✨ Start SMALL - Don't try to build everything at once
🎯 Focus on FUN first - Graphics/polish come later
🔄 Iterate quickly - Playtest early and often
📊 Learn from data - Use analytics to improve
🤖 AI is HARD - Be patient with ML training
👥 Get feedback - Show your game to others
🎮 Play test A LOT - You'll find issues you never expected
💪 Don't give up - Game dev is hard but rewarding!

Good luck! 🚀🧟‍♂️
```
