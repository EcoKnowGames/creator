## EcoKnow

### Overview
EcoKnow is a Unity-based, turn‑based ecosystem sandbox/puzzle. You load or author a scenario (species, interaction matrix, map) and play on a grid where species populations change each round based on growth rates, a Lotka‑Volterra‑style interaction matrix, and probabilistic movement between neighboring cells. You spend limited actions to harvest or introduce species (using an inventory/currency), aiming to keep populations within target ranges for consecutive rounds to satisfy win conditions. Gameplay and state can be exported to JSON for analysis.

### Core concepts
- **Grid ecosystem**: Cells display species tokens with state rings (extinct, vulnerable, stable, abundant) and per‑cell population indicators.
- **Population dynamics**: Per‑species intrinsic growth, inter‑species interaction matrix, and movement to neighboring cells each round.
- **Action economy**: Each round grants a limited number of actions to harvest or introduce species, consuming/earning resources.
- **Win conditions**: Keep target species totals within thresholds for required consecutive rounds (with a grace allowance).
- **Scenario authoring**: Create scenarios in a node‑based editor and export to JSON; or load JSON at runtime.

### Project layout (key paths)
- **Scene**: `Assets/Scenes/scene_Sandbox.unity`
- **Scripts (runtime)**: `Assets/Scripts/Sandbox/`
- **UI**: `Assets/_EcoKnow/UI/`
- **Icons/Resources**: `Assets/_EcoKnow/Resources/`
- **Maps (CSV)**: `Assets/_EcoKnow/Maps/`
- **Matrices (CSV)**: `Assets/_EcoKnow/Matrices/`
- **Scenario Graph assets**: `Assets/_EcoKnow/Scenario Editor/` (e.g., `TestScenarioGraph.asset`)

### FIRST TIME SETUP
- **Unity Editor version**: Use Unity `6000.0.49f1` (Unity 6). Opening with a different version may cause compatibility issues.
- **Steps**:
  1. Clone the repository.
  2. Open Unity Hub, add the project folder, and select Editor `6000.0.49f1`.
  3. Unity will import and resolve packages automatically (internet required for Git-based packages).
  4. Open the sandbox scene: `Assets/Scenes/scene_Sandbox.unity`.
  5. Press Play.

### Packages used (auto-installed via Package Manager)
- **com.github.siccity.xnode**: BSD-3-Clause — Graph editor framework used by the Scenario Editor.
- **com.yasirkula.simplefilebrowser**: MIT — Runtime file dialogs for loading/saving JSON.
- **com.unity.uiextensions**: BSD-3-Clause — Additional UI components used by the in-game UI.
- **com.unity.nuget.newtonsoft-json**: MIT — JSON serialization (Newtonsoft.Json via Unity NuGet bridge).
- **Unity official packages** (e.g., `com.unity.ugui`, `com.unity.feature.2d`, `com.unity.timeline`, `com.unity.test-framework`, `com.unity.visualscripting`, editor integrations, and `com.unity.modules.*`): Licensed by Unity (typically under the Unity Companion License and/or Unity Terms). See each package’s License entry in the Unity Package Manager for details.

### Running the sandbox
- On Play, a file dialog prompts you to load a Scenario JSON.
- If you cancel (in the Unity Editor), the game will fall back to using the currently assigned `ScenarioNodeGraph` asset to start a scenario.
- Each round:
  - Populations and movement are simulated.
  - You receive action points to interact (harvest/introduce) via the UI.
  - Win conditions are evaluated and progress is shown in the UI.

### Scenario authoring (Graph Editor)
- Author scenarios using the node graph system (xNode-based): assets live under `Assets/_EcoKnow/Scenario Editor/`.
- A scenario includes: name, rounds, actions per round, start currency, seed, Entities, Items, Win Conditions, Matrix, and Map Layout.
- Export a scenario to JSON from the graph (Editor context command provided by the graph script). The exported JSON can be loaded at runtime.

### Data export
- The game records events each round, including: total populations by species, inventory, per‑cell populations, and win condition results.
- Press `P` during play to export the current session data as JSON to Unity's `persistentDataPath` (platform‑specific). The project also provides a save dialog method that can be invoked from UI to choose a destination.

### Controls and tips
- Use the UI entity list to select a species for actions.
- Advance the round using the on‑screen control. Action points are replenished each round.
- Token borders and colors indicate per‑cell species state; totals are shown in the side panel.

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
