# 100 Burger Family - Draft Design

> Status: Draft
>
> This is an early design draft. Detailed PM documents and technical decisions will be finalized later.

## 1. Game Overview

A casual management game where the player runs a burger restaurant for 3-10 minutes and competes with an AI rival on sales and reputation.

- Engine: Unreal Engine 5
- Implementation direction: Blueprint-first
- Play style: Short daily sessions with long-term restaurant growth
- Presentation: Friendly UI, icons, and customer reactions over high-end graphics

## 2. Core Gameplay Loop

```text
Review shop and prepare
→ Choose menu, prices, ingredients, and advertising
→ Run the restaurant for 3-10 minutes
→ Process customer orders and make burgers
→ Settle inventory, sales, and reputation
→ Compare results with the AI competitor
→ Receive rewards and upgrades
→ Start the next day
```

The first MVP should validate a 3-minute operating mode.

## 3. Web Shop Concept

The web shop is a long-term management layer used before and after each operating session, not the main interaction screen during service.

### Shop products

- Buns, patties, sauces, and toppings
- Refrigerator and cooking station upgrades
- Special burger recipes
- Advertising coupons
- Staff or automation equipment

### Basic flow

```text
Purchase from the web shop
→ Add items to inventory and equipment
→ Use them during the short restaurant session
→ Consume ingredients and earn sales
→ Save results and rewards
```

The first version should use an in-game mock shop UI and `SaveGame` instead of a live backend. Web API or Firestore integration can be evaluated after the gameplay loop is validated.

## 4. AI Competitor

The first version should use a score-based AI that reacts to the player's recent choices instead of starting with a real machine-learning model.

```text
AI decision score
= expected demand
+ price competitiveness
+ current inventory
+ response to the player's recent strategy
+ personality modifier
```

### Example AI personalities

- Low-price: attracts more customers with lower prices
- Premium: prioritizes higher prices, quality, and reputation
- Trend-driven: quickly follows popular ingredients and new recipes
- Aggressive: directly counters the player's main menu

Actual AI training, data collection, and ONNX integration are future expansion items after the core game loop is stable.

## 5. UE5 Structure Draft

- `BP_RestaurantManager`: day flow, sales, and reputation
- `BP_InventoryComponent`: ingredients and quantities
- `BP_OrderDesk`: web shop orders and restocking
- `BP_CompetitorAI`: AI menu, price, and advertising decisions
- `BP_Customer`: customer preferences and orders
- `BP_DayManager`: day start and day end
- `BP_RestaurantEconomy`: costs, sales, and rewards
- `DA_Ingredient`: ingredient data
- `DA_Recipe`: menu data
- `DA_ShopProduct`: shop product data
- `WBP_Shop`: web shop UI
- `WBP_RestaurantHUD`: service HUD
- `WBP_DailyResult`: daily results and AI comparison
- `SaveGame_BurgerProfile`: local progression save

## 6. MVP Scope

- 5 burger menus
- 6 ingredient types
- 4 customer types
- 1 AI competitor
- 3-minute restaurant session
- Menu, price, and advertising choices
- Ingredient purchases and inventory consumption
- Daily result screen
- 3 upgrades
- Local save data

## 7. Out of Scope for MVP

- Multiplayer
- Real-money payments
- Real-time online trading
- Large-scale backend services
- Reinforcement-learning AI from day one
- Large C++ framework
- Final art production

## 8. First Validation Target

Build the following flow as one playable vertical slice:

```text
WBP_Shop purchase
→ Update inventory
→ Run a 3-minute session
→ Consume ingredients
→ Grant sales rewards
→ Compare with the AI result
→ Save with SaveGame
```

Only after this flow is fun and stable should the team review detailed PM documents, online synchronization, seasons, rankings, or real AI training.
