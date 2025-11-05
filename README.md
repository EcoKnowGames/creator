## EcoKnow

### Overview
EcoKnow is a Unity-based, turn‑based ecosystem sandbox/puzzle. You load or author a scenario (species, interaction matrix, map) and play on a grid where species populations change each round based on growth rates, a Lotka‑Volterra‑style interaction matrix, and probabilistic movement between neighboring cells. You spend limited actions to harvest or introduce species (using an inventory/currency), aiming to keep populations within target ranges for consecutive rounds to satisfy win conditions. Gameplay and state can be exported to JSON for analysis.

### Core concepts
- **Grid ecosystem**: Species tokens in each cell with state rings (extinct, vulnerable, stable, abundant) and population indicators.
- **Population dynamics**: Intrinsic growth, inter‑species interaction matrix, and per‑round movement to neighboring cells.
- **Action economy**: Limited actions per round to harvest/introduce species, consuming/earning resources.
- **Win conditions**: Keep target species totals within thresholds for required consecutive rounds (with a grace allowance).

### Project layout (key paths)
- **Scene**: `Assets/Scenes/scene_Sandbox.unity`
- **Scripts (runtime)**: `Assets/Scripts/Sandbox/`
- **UI**: `Assets/_EcoKnow/UI/`
- **Icons/Resources**: `Assets/_EcoKnow/Resources/`
- **Maps (CSV)**: `Assets/_EcoKnow/Maps/`
- **Matrices (CSV)**: `Assets/_EcoKnow/Matrices/`
- **Scenario Graph assets**: `Assets/_EcoKnow/Scenario Editor/` (e.g., `TestScenarioGraph.asset`)

### FIRST TIME SETUP
- **Unity Editor version**: Use Unity `6000.0.60f1` (Unity 6). Opening with a different version may cause compatibility issues.
- **Steps**:
  1. Clone the repository.
  2. Ensure GIT LFS is enabled or assets will be blank
  3. Open Unity Hub, add the project folder, and select Editor `6000.0.49f1`.
  4. Unity will import and resolve packages automatically (internet required for Git-based packages).
  5. Open the sandbox scene: `Assets/Scenes/scene_Sandbox.unity`.
  6. Press Play.

### Packages used (auto-installed via Package Manager)
- **com.github.siccity.xnode**: BSD-3-Clause — Graph editor framework used by the Scenario Editor.
- **com.yasirkula.simplefilebrowser**: MIT — Runtime file dialogs for loading/saving JSON.
- **com.unity.uiextensions**: BSD-3-Clause — Additional UI components used by the in-game UI.
- **com.unity.nuget.newtonsoft-json**: MIT — JSON serialization (Newtonsoft.Json via Unity NuGet bridge).
- **Unity official packages** (e.g., `com.unity.ugui`, `com.unity.feature.2d`, `com.unity.timeline`, `com.unity.test-framework`, `com.unity.visualscripting`, editor integrations, and `com.unity.modules.*`): Licensed by Unity (typically under the Unity Companion License and/or Unity Terms). See each package’s License entry in the Unity Package Manager for details.

### Art and icon assets
- **Microsoft Fluent 3 Emoji (Fluent Emoji/Fluent 3D Emoji)**: MIT — Open-source emoji/icon set by Microsoft. Source: [Fluent Emoji GitHub repository](https://github.com/microsoft/fluentui-emoji).
- Icons included in this project live under `Assets/_EcoKnow/Resources/Icons/` (high-contrast PNG variants are used in UI).
- When creating scenarios, please only reference icons from this provided set to ensure licensing compliance and that assets are available in builds.

### Scenario authoring (Graph Editor)
- Author scenarios using the node graph system (xNode-based): assets live under `Assets/_EcoKnow/Scenario Editor/`.
- A scenario includes: name, rounds, actions per round, start currency, seed, Entities, Items, Win Conditions, Matrix, and Map Layout.

### CREATING YOUR FIRST SCENARIO
- Scenarios are saved in the `Assets/_EcoKnow/Scenario Editor/` folder.
- Right click -> Create -> Scenario Node Graph.
- Double‑click the new asset to open it in xNode.
- Right‑click in the canvas to add nodes.
- Node options:
  - (see image attached)
- When you’re ready, you can either:
  - Export your Scenario via right click -> Export JSON (ensure you used only icons from the provided set), or
  - Replace the default Scenario loaded in the `scene_Sandbox` scene (GameObject: `ScenarioLoader TEMP`) with your new Graph.

### PLAYING A SCENARIO
- Inside Unity: Press Play. A file dialog appears to load a Scenario JSON. Cancelling uses the `ScenarioLoader TEMP` graph.
- You can also select a JSON exported from the graph to load that scenario at runtime.
- Navigation: WASD moves the board; +/- zooms the camera (planned update: drag + middle mouse/pinch).
- If you can’t introduce an item, you likely lack the required resources. Check the Scenario Graph for costs/rules.
- When a session ends, you can export a JSON data file. See the Data export section below.

### Data export
- The game records events each round, including: total populations by species, inventory, per‑cell populations, and win condition results.
- Press `P` during play to export the current session data as JSON to Unity's `persistentDataPath`. A save‑as dialog variant is also available from UI.

### Controls and tips
- Use the UI entity list to select a species for actions.
- Advance the round using the on‑screen control; action points are replenished each round.
- Token borders/colors indicate per‑cell species state; totals are shown in the side panel.

### Troubleshooting
- If Git-based packages fail to resolve, ensure you have network access and that Unity Hub/Editor can access Git URLs.
- If the wrong Unity version is used, upgrade/downgrade in Unity Hub to `6000.0.49f1` and reopen the project.

### Collaborators
- This project is a collaboration between **GLITCHERS** and the **University of Stirling**.

#### Project collaborators (holding)
- Name — Role (Affiliation)
- Name — Role (Affiliation)

### License
- **Project license**: GPL-3.0-only. See the `LICENSE` file for full terms.
- **Third-party licenses**: See the Packages section for per‑package license notes (e.g., xNode — BSD-3-Clause; Unity UI Extensions — BSD-3-Clause; SimpleFileBrowser — MIT; Newtonsoft.Json — MIT). Unity official packages are licensed by Unity; refer to their entries in the Package Manager.
