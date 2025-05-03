# 🚀 Space! (Based on Kepler's orbit equations)

## 🎮 Game Concept

A small-scale space simulation game inspired by *Kerbal Space Program*.  
The player can design rockets, launch them into orbit, and explore space using realistic (but simplified in distances) orbital mechanics.
The game features a progression system based on science and economy.  
  
## 📈 Player Progression
Players earn **science points** by conducting experiments, exploring new celestial bodies, and reaching key milestones (e.g., first orbit, landing, interplanetary transfer). These points can be used to unlock new technologies through a **science tree**, gradually expanding the player's ability to build more advanced rockets and modules.
  
Each rocket part has a **cost**, and missions must be planned within budget constraints, encouraging efficient design and thoughtful execution.  
  
This system aims to simulate the challenges of real-world space programs - balancing scientific advancement, engineering capabilities, and limited funding.

## ✅ Implemented Features

- 3D hybrid space environment
- Orbital mechanics using Keplerian calculations
- Planetary gravity and sphere of influence (SOI)
- Thrust and rotation controls (WASDQE + throttle)
- Navigation UI with navball
- Floating Origin support
- Camera controls (focus, zoom)
- Save/load game state
- Tools for creating Solar Systems 
## 🔜 Planned Features


### 🧩 Gameplay Systems

- Rocket construction system (modular assembly)
- Mission objectives and progression
- Science point system with unlockable tech tree
- Budget management and part costs

### ⚙️ Physics & Mechanics

- Atmospheric drag and reentry heat simulation
- Aerodynamics system for designing functional aircraft and spaceplanes (lift, drag, control surfaces)
- Surface landing mechanics
- Orbital maneuvering tool

### 🌍 Simulation & World

- Solar system map mode with multiple SOIs (Sphere of Influence)
- Time warp system for orbital maneuvering and long-distance travel

### 🛠 Procedural Generation

- Procedural planet generation (size, terrain, gravity)
- Procedural solar system generation (planet count, layout, orbit)

## 🧱 Technologies & Tools

- Unity (version 2022.3.3)
- C#

## 🧪 How to Play / Test

1. Clone the repository  
2. Open in Unity Hub  
3. Load any available scene  
4. Press Play and explore space!  

## 💡 Development Philosophy

- Built with principles of **SOLID**, **DRY**, and **KISS**. 

## 🤝 Contributions

Pull requests are welcome.    
If you're interested in collaborating, feel free to reach out or open an issue.

> ⚠️ **Code Style & Precision Guidelines**  
> - Be mindful when choosing between `float` and `double`. While Unity primarily uses `float`, certain orbital or long-distance calculations may require `double` for precision.  
> - Avoid unnecessary conversions between `float` and `double` to reduce performance cost and bugs.  
> - Precision errors can occur when dealing with very large coordinates (e.g., in interplanetary distances). Always test floating origin and orbital calculations carefully to prevent jittering or drift.
> - All units use The International System of Units (SI System)