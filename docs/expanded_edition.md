# 100 Burger Family — Expanded Edition

![Architecture](images/ai_battle_banner.png)

## Table of Contents

- [System Overview](#system-overview)
- [Core Game — Today's Burger](#core-game--todays-burger)
- [Expansion — AI Versus Mode](#expansion--ai-versus-mode)
- [Tech Stack & Architecture](#tech-stack--architecture)
- [Development Roadmap](#development-roadmap)
- [B2B Strategy](#b2b-strategy)

---



## System Overview

```
Core Game (1)                        Expansion (2)
─────────────────────                ─────────────────────────────
1 guest per day                  →   Collect 100-day play data
Craft 1 perfect burger               ↓
3 ~ 10 minutes per session           AI learns player patterns
Freshness + Grill + Time = Score     ↓
100-day league & ranking             AI Chef (Baek Gi-jun) versus system
```

---

## Core Game — Today's Burger

### Concept

A fine dining game where you craft one perfect burger for one guest, every day.
Not fast food — **precision and care** are what matter.

- 1 game per day, 1 guest, 1 burger
- Play time: 3 min (skilled) ~ 10 min (careful)
- Daily guest generated from a shared seed → same conditions for all players

### Three Scoring Dimensions

| Dimension | Description | Measurement |
|---|---|---|
| Freshness | How timely were ingredients harvested and used? | Time elapsed between harvest and use |
| Grill Accuracy | How closely did you match the guest's requested doneness? | 15-stage gauge judgment |
| Time | Total time to completion (faster = bonus, but accuracy comes first) | Elapsed seconds |

### Score Calculation

```
Daily Score = (Freshness × 0.35) + (Grill Accuracy × 0.45) + (Time Bonus × 0.20)

Grill Judgment:
  Perfect (center slot)  → ×1.5
  Good    (side slots)   → ×1.0
  Failed  (out of range) → ×0.0

Time Bonus:
  Under 3 min  → +20%
  Under 5 min  → +10%
  Over 10 min  → none
```

### 100-Day League System

```
Season Structure:
Day 1 ~ Day 100  →  Daily score accumulation
                 →  Weekly ranking updates (every 7 days)
                 →  Final season ranking confirmed

League Tiers (by cumulative score):
  🥇 Master Chef      Top 5%
  🥈 Head Chef        Top 20%
  🥉 Kitchen Lead     Top 50%
  🍳 Apprentice       All others
```

---

## Expansion — AI Versus Mode

After the 100-day season, player data is used to train AI Chef Baek Gi-jun, creating a competitive versus system.

### Data Collection (During 100-Day Season)

Every action across 100 days of play becomes training data.

```
Collected data:
- Time spent at each station
- Ingredient harvest timing
- Grill gauge stop position
- Assembly order and duration
- Final score and judgment results
```

### AI Training Design (Phase 2)

**WebGL-compatible approach**: Train on collected data in Python → Export as ONNX → Run via Unity Inference Engine

**State (Observations):**
- Current grill gauge position (0.0 ~ 1.0 normalized)
- Ingredient freshness remaining (0.0 ~ 1.0)
- Ingredient quantities (Bun / Patty / Bacon / Sauce, each 0~5)
- Elapsed time (0.0 ~ 1.0 normalized)

**Action (Discrete):**
- 0: Stop grill (timing decision)
- 1: Move to next station
- 2: Harvest / collect ingredient
- 3: Move to supply station (restock)

**Reward:**

| Condition | Reward |
|---|---|
| Perfect grill timing | +1.0 |
| Good grill timing | +0.5 |
| High-freshness serving | +0.3 |
| Ingredient burned | -0.5 |
| Zero-freshness serving | -0.3 |

### AI Difficulty Scaling

After training on 100-day data, AI difficulty auto-adjusts based on the player base's average skill level.

| Difficulty | AI Behavior |
|---|---|
| Beginner | Rule-based, mimics top 75% average |
| Normal | Data-trained, top 50% play patterns |
| Expert | Top 20% play patterns |
| Master | Top 5% (Master Chef) play patterns reproduced |

### AI Versus Result Screen

```
┌─────────────────────────────────────────┐
│           Today's Result                │
├──────────────┬──────────────────────────┤
│  🧑 Player   │  🤖 Baek Gi-jun         │
│  Freshness:88│  Freshness: 94          │
│  Grill: Perfect│ Grill: Perfect        │
│  Time:  4:32 │  Time:  3:58            │
│  Score:  910 │  Score: 1,040           │
├──────────────┴──────────────────────────┤
│  🏆 Baek Gi-jun wins — Keep practicing! │
└─────────────────────────────────────────┘
```

---

## Tech Stack & Architecture

### Script Structure

```
Assets/Scripts/
├── Core/
│   ├── GameMode.cs          ← enum: Daily | VsAI
│   ├── OrderManager.cs      ← DailySeed-based guest/order generation
│   ├── ScoreManager.cs      ← Composite score: freshness · grill · time
│   └── SeasonManager.cs     ← 100-day league progression & ranking
├── Game/
│   ├── GrillGauge.cs        ← 15-stage grill gauge
│   ├── FreshnessTracker.cs  ← Per-ingredient freshness time tracking
│   └── AIChef.cs            ← Baek Gi-jun behavior (rule-based → ONNX)
├── Data/
│   └── PlayDataRecorder.cs  ← 100-day play data collection (AI training)
└── UI/
    ├── DailyResultHUD.cs    ← Daily score result screen
    ├── LeaderboardHUD.cs    ← League & ranking screen
    └── VsHUD.cs             ← AI versus comparison HUD
```

### Platform

- **Unity 6.3 LTS**, WebGL (mobile-optimized)
- **Data Collection**: Firebase Realtime DB (100-day play logs)
- **ML-Agents**: Python training environment → ONNX Export → Unity Inference Engine

---

## Development Roadmap

### v0.6.0 — Core Game Complete
- 15-stage grill gauge (replaces GrillStation timer)
- Freshness tracker (FreshnessTracker)
- Daily score calculation (ScoreManager)

### v0.7.0 — 100-Day League
- DailySeed guest generation (OrderManager)
- Daily result screen + cumulative ranking
- Firebase play data collection begins

### v0.8.0 — Beta Test (100-Day Season)
- Tester recruitment and 100-day live operation
- Play data collection and validation
- League tier system finalized

### v0.9.0 — AI Training & Versus System
- ML-Agents training on 100-day dataset
- ONNX export / Unity integration
- AI Chef (Baek Gi-jun) with 4 difficulty tiers

### v1.0.0 — Public Release
- WebGL build optimization
- Coupon / reward system integration
- B2B demo video

---

## B2B Strategy

This project is developing a B2B monetization model beyond the core game experience.

- Telecom family plan add-on service integration
- Senior care platform partnership
- F&B industry training & education platform expansion

For partnership inquiries, please reach out directly.
