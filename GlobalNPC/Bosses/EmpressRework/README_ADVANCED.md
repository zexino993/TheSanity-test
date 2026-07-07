# Empress of Light - Advanced Rework Mod (2000+ Lines)

## Overview
Advanced rework dari Empress of Light dengan 50+ attack patterns, AI system, difficulty modes, dan complete reward system.

## 🎮 Fitur Utama

### **Difficulty Modes**
- ✅ Easy (60% difficulty normal)
- ✅ Normal (baseline)
- ✅ Hard (140% difficulty)
- ✅ Extreme (180% difficulty + bonus loot)

### **3 Phase System dengan Dynamic Scaling**
- **Phase 1** (100-66% HP): 15 attack patterns
- **Phase 2** (65-34% HP): 18 attack patterns
- **Phase 3/DSP** (<33% HP): 20 attack patterns
- **Total**: 53 unique attack variations

### **Advanced AI**
- 🧠 Intelligent movement & positioning
- 🧠 Dodge system untuk menghindari player projectiles
- 🧠 Enrage timer setelah 60 detik
- 🧠 Health threshold detection
- 🧠 Dynamic difficulty scaling

### **Attack Pattern Categories**

#### Phase 1 (15 Attacks)
1. Homing Burst Attack
2. Laser Beam Pattern
3. Charge Attack
4. Projectile Circle
5. Wave Pattern
6. Spin Attack
7. Targeted Spray
8. Diagonal Wave
9. Random Burst
10. Sweep Attack
11. Double Laser
12. Triple Homing
13. Expanding Circle
14. Cross Burst
15. Curved Wave

#### Phase 2 (18 Attacks)
1. Combo Attack
2. Screen Filling Projectiles
3. Dash Attack
4. Cross Laser
5. Spiral Pattern
6. Double Wave
7. Chaotic Burst
8. Triple Dash
9. Vortex Attack
10. Pulse Wave
11. Tracking Shots
12. Wall of Projectiles
13. ZigZag Pattern
14. Star Burst
15. Ripple Attack
16. Quad Combo
17. Sweep Vortex
18. Dense Spiral

#### Phase 3 (20 Attacks)
1. Extreme Combo
2. Projectile Hell
3. Multi Dash
4. Laser Grid
5. Chaos Pattern
6. Ultimate Burst
7. Inferno Phase
8. Omnidirectional Blast
9. Splitting Projectiles
10. Matrix Attack
11. Cylinder Barrage
12. Explosive Pattern
13. Supercombo Attack
14. Wave Slash
15. Density Bomb
16. Crescent Moon
17. Helix Barrage
18. Ring of Fire
19. Meteoral Strike
20. Final Phase Blast

### **Visual Effects System**
- 💫 Telegraph glow effect sebelum attack
- 💫 Dust particles saat projectile spawn
- 💫 Screen shake saat attack heavy
- 💫 Color-coded visual warnings
- 💫 Enrage visual indicators

### **Statistics Tracking**
- 📊 Total attacks landed
- 📊 Total damage dealt
- 📊 Battle duration
- 📊 Phase progression

### **Reward System**

**Normal Drops:**
- Crystal Storm
- Soul of Light (increased)
- Hallowed Greaves

**Extreme Mode Bonus:**
- Lunar Ore (5-15 pieces)
- Additional item drops

### **Enrage System**
- Boss becomes enraged after 60 seconds of battle
- Speed increases by 20%
- Attack cooldown decreases
- Visual enrage effects triggered
- Red torch particles emit from boss

## 📊 Stats Scaling

### Base Stats (Normal Mode)
- **Health**: 4200 HP
- **Damage**: 55 ATK
- **Defense**: 15 DEF

### Easy Mode
- **Health**: 2940 HP (70%)
- **Damage**: 33 ATK (60%)
- **Defense**: 15 DEF

### Hard Mode
- **Health**: 6300 HP (150%)
- **Damage**: 77 ATK (140%)
- **Defense**: 20 DEF

### Extreme Mode
- **Health**: 8400 HP (200%)
- **Damage**: 99 ATK (180%)
- **Defense**: 20 DEF
- **Bonus**: Lunar Ore drops

## ⏱️ Attack Timing

### Phase 1
- **Cooldown**: 90 frames (1.5 sec)
- **Telegraph**: 30 frames (0.5 sec)

### Phase 2
- **Cooldown**: 75 frames (1.25 sec)
- **Telegraph**: 25 frames (0.42 sec)

### Phase 3
- **Cooldown**: 60 frames (1.0 sec)
- **Telegraph**: 20 frames (0.33 sec)

## 🤖 AI Behavior

### Movement System
- Updates position every 30 frames
- Maintains optimal distance from player (200+ units)
- Circles around player strategically
- Smooth acceleration/deceleration

### Dodge System
- Checks for player projectiles every 60 frames
- Automatically dodges hostile projectiles within 300 units
- Maintains aggressive positioning despite dodging

### Enrage Mechanic
- Triggers after 60 seconds of battle
- Increases all attack speeds by 20%
- Visual red torch particles
- Screen shake on enrage activation

## 🔧 Configuration

**Easy Setup:**
```csharp
EmpressConfig.CurrentDifficulty = EmpressConfig.DifficultyMode.Easy;
```

**Hard Setup:**
```csharp
EmpressConfig.CurrentDifficulty = EmpressConfig.DifficultyMode.Hard;
```

**Extreme Setup:**
```csharp
EmpressConfig.CurrentDifficulty = EmpressConfig.DifficultyMode.Extreme;
```

## 📈 Code Statistics

- **Total Lines**: 2000+
- **Classes**: 4
- **Attack Patterns**: 53
- **Methods**: 100+
- **Difficulty Modes**: 4

## 🎯 Balance Notes

- Each phase has unique attack patterns
- Telegraph system ensures fair gameplay
- AI positioning creates interesting challenges
- Enrage timer rewards quick kills
- Difficulty scaling affects all aspects:
  - Health
  - Damage
  - Attack Speed
  - Loot Quality

## 🚀 Future Enhancements

- [ ] Custom sprite variations per phase
- [ ] Boss theme music per phase
- [ ] Achievement system
- [ ] PvP multiplayer scaling
- [ ] Custom projectile effects
- [ ] Boss arena mechanics

## 📝 Installation

1. Copy `EmpressReworkAdvanced.cs` ke folder `GlobalNPC/Bosses/EmpressRework/`
2. Rebuild project di tModLoader
3. Reload mods
4. Summon Empress dengan Crystal Heart

## 🎮 Tips Bermain

- **Phase 1**: Learn attack patterns, build confidence
- **Phase 2**: Aggressive attack, requires more dodging
- **Phase 3**: Maximum focus required, visual cues are key
- **Enrage**: Don't let boss reach 60 seconds if possible
- **Extreme**: Treat as true challenge mode - deserves bonus loot

---

**Made by**: TheSanity Team
**Version**: 2.0 Advanced Edition
**Lines of Code**: 2000+
