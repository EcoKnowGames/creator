## EcoKnow Data Export Guide

This document describes the JSON file produced by the Sandbox data export. It covers how to export, the top-level structure, field meanings, and parsing notes. An example is provided using `Wilderness_Test_data.json`.

### How to export
- In-game: open the results screen and press the Export button. On supported platforms (non-WebGL), a file save dialog appears with a default name of `<ScenarioName>_data.json`.
- Programmatically: call `Data.DataManager.Instance.ShowSaveDialog(onSuccess, onCancel)`.
- Default location: wherever you choose in the save dialog. Historically the default path was `Application.persistentDataPath` with the same default filename.

### Top-level structure
```json
{
  "Config": { /* ScenarioConfig */ },
  "Events": [ /* EventDataObject[] */ ]
}
```

### Config (ScenarioConfig)
- **AppVersion**: string. App version that produced the export.
- **UnityVersion**: string. Unity version used.
- **Scenario**: object containing the scenario definition at the time of export.
  - **Name**: string.
  - **Rounds**: int. Total rounds.
  - **ActionsPerRound**: int.
  - **StartCurrency**: int.
  - **Seed**: int.
  - **Entities**: array of entity definitions:
    - **ID**: string (entity identifier, e.g. "Hare").
    - **Icon**: string (resource path).
    - **Colour**: string (8-char RGBA hex, e.g. "FC9F4EFF").
    - **GrowthRate**: float.
    - **MovementRate**: float (0..1 typical).
    - **VulnerableThreshold**: float (population threshold).
    - **AbundanceThreshold**: float (population threshold).
    - **AutoPlace**: bool (auto-seed initial population in valid cells).
    - **CanHarvest**: bool.
    - **CanIntroduce**: bool.
    - **HarvestQuantities**: array of { "ID": string, "Value": int } or null.
    - **IntroduceQuantities**: array of { "ID": string, "Value": int } or null.
  - **Items**: array of item definitions:
    - **ID**: string.
    - **Icon**: string or null.
    - **Value**: int (sell value in currency).
    - **CanSell**: bool.
  - **WinConditions**: array of conditions:
    - **Title**: string.
    - **Description**: string.
    - **EntityIndex**: int (index into `Entities`, aligned with `Matrix.entityIDs`).
    - **LowerLimit**: float.
    - **UpperLimit**: float (0 to disable upper bound).
    - **RequiredRounds**: int (>=1 for consecutive rounds; -1 for persistent range targets).
  - **Matrix**: interaction coefficients and IDs:
    - **entityIDs**: string[] (order defines entity index mapping across the export).
    - **entityMatrix**: float[][] (NxN array serialized as array-of-arrays; aligns with `entityIDs`).
  - **Map**: layout reference and grid:
    - **fileName**: string (CSV source name).
    - **gridDef**: object
      - **rows**: int.
      - **columns**: int.
      - **tileIDs**: int[][] (2D array serialized as array-of-arrays; `-1` indicates an invalid/unplayable cell).

### Events (EventDataObject[])
Each entry captures a snapshot at a gameplay event.

- **Type**: string enum. One of:
  - `GAME_START`, `GAME_END`, `ROUND_END`, `INTRODUCE`, `HARVEST`, `SELL`.
- **Player**: int (player index; single-player uses 1).
- **Populations**: object mapping `Entity.ID` → int (total across valid cells).
- **Inventory**: object mapping item IDs → int. Includes constants `currency` and `action` alongside any item IDs.
- **WinConditions**: array
  - **Title**: string.
  - **Completed**: bool.
  - **Results**: object mapping int (round number) → string (result/explanation). May be empty `{}`.
- **Map**: object
  - **Populations**: 2D array `[columns][rows]` of per-cell population dictionaries:
    - Each cell: object mapping `Entity.ID` → int.
    - A value of `-1` indicates the cell is invalid/unplayable for that entity (or the cell itself is invalid).
- **Meta**: object (optional). Reserved keys include:
  - `InventoryChange`: planned delta of inventory between events.
  - `CellChange`: planned per-cell changes. May be absent or empty.

### Example (truncated)
```json
{
  "Config": {
    "AppVersion": "1.0",
    "UnityVersion": "6000.0.49f1",
    "Scenario": {
      "Name": "Wilderness Test",
      "Rounds": 12,
      "ActionsPerRound": 4,
      "StartCurrency": 800000,
      "Seed": 123456,
      "Entities": [ { "ID": "Hare", "Colour": "FC9F4EFF", "GrowthRate": 1.4, ... } ],
      "Items": [ { "ID": "Meat", "Value": 5, "CanSell": true } ],
      "WinConditions": [ { "Title": "Fox Population", "EntityIndex": 1, ... } ],
      "Matrix": { "entityIDs": ["Hare", "Fox", "House"], "entityMatrix": [[-0.002, 0.001, 0.0], ...] },
      "Map": { "fileName": "Level 1", "gridDef": { "rows": 6, "columns": 12, "tileIDs": [[1,4,1,-1,-1,-1], ...] } }
    }
  },
  "Events": [
    {
      "Type": "GAME_START",
      "Player": 1,
      "Populations": { "Hare": 5900, "Fox": 5900, "House": 0 },
      "Inventory": { "currency": 800000, "action": 4 },
      "WinConditions": [ { "Title": "Fox Population", "Completed": false, "Results": {} } ],
      "Map": { "Populations": [ [ { "Hare": 100, "Fox": 100, "House": 0 }, { "Hare": -1, "Fox": -1, "House": -1 } ], ... ] },
      "Meta": {}
    }
  ]
}
```

### Parsing notes
- 2D arrays (`float[,]`, `int[,]`, `Dictionary<string,int>[,]`) are serialized as arrays of arrays. Treat them as `[columns][rows]` to align with grid coordinates used in the game code.
- Negative values (`-1`) in populations or `tileIDs` mark invalid/unplayable cells. Filter these out when computing totals.
- Inventory uses string keys. The constants `currency` and `action` are always in lowercase.
- Colors use 8-digit hex RGBA (no `#`).

### Versioning and stability
- The schema is intentionally explicit via named keys and may evolve. Consumers should key off property names rather than array order.
- `Config.AppVersion` and `Config.UnityVersion` identify the producing versions; use them for compatibility checks.

### Intended uses
- Analyze player actions and ecosystem evolution over time.
- Train or validate models on population dynamics under different policies.
- Replay or visualize per-cell population maps per event.

If you need a formal JSON Schema for validation, we can add one alongside this document.


