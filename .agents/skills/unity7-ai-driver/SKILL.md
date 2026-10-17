---
name: unity7-ai-driver
description: Complete guide and operational playbook for controlling Unity 7 via Unity CLI and Antigravity MCP. Covers live C# eval, scene hierarchy inspection, GameObject and component manipulation, PlayMode verification loops, visual viewport capture, custom [CliCommand] authoring, and full project/editor lifecycle management (create, install, remove, delete).
---

# Unity 7 & Antigravity MCP Integration Playbook

This skill provides the operational guide for pair-programming with Unity 7 using the **Unity CLI** and **Antigravity MCP (Model Context Protocol)**.

---

## 1. Architecture & Mental Model

```
+------------------------------------+
|     Antigravity AI Agent / IDE     |
+-----------------+------------------+
                  | (stdio / MCP tools)
                  v
+------------------------------------+
|       Unity CLI (`unity.exe`)      |
|    Starts MCP server (`unity mcp`) |
+-----------------+------------------+
                  | (Localhost IPC / port 7800)
                  v
+------------------------------------+
|       Unity 7 Editor (Running)     |
|    Package: `com.unity.pipeline`   |
|         (CoreCLR Runtime)          |
+------------------------------------+
```

### Why Unity 7 + Pipeline is Fast
- **Zero Domain Reload:** Unity 7's CoreCLR architecture uses incremental code reload.
- **Warm Roundtrips:** Agent commands (`eval`, `set_transform`, `create_gameobject`) round-trip through the resident Editor in **200–500 ms** without triggering compilation or assembly unloading.
- **Dynamic MCP Discovery:** Tools are discovered dynamically from the running Editor. When you add a new custom C# command, it appears in the agent toolset without restarting the IDE.

---

## 2. Health & Status Commands

### Check Editor Connection
```bash
unity status --json
```
**Expected Response:**
- `state`: `"ready"` (or `"playing"` during Play Mode)
- `port`: Default `7800`
- `pid`: Process ID of `Unity.exe`

### Discover All Connected Tools
```bash
unity list --json
```
Lists every callable command, including parameters, descriptions, and custom tools registered in your project.

### Detailed Editor Diagnostics
```bash
unity command editor_status --json
```
Returns `status`, `compiling`, `domainReloadInProgress`, `playMode`, and `unityVersion`.

---

## 3. Scene & GameObject Manipulation Cookbook

### 3.1 Inspect Full Hierarchy
```bash
unity command get_scene_hierarchy --json
```
Returns a complete JSON tree containing:
- `sceneName`, `scenePath`, `isDirty`
- `roots`: List of root GameObjects with their `instanceId`, `hierarchyPath`, `activeSelf`, and `components`.

### 3.2 Create GameObjects & Primitives
```bash
# Empty GameObject
unity command create_gameobject --name "MyFolder"

# Primitive under a parent
unity command create_gameobject --primitive "cube" --name "DemoCube" --parent "/MyFolder"

# Supported primitives: cube, sphere, capsule, cylinder, plane, quad
```

### 3.3 Set Transform (Position, Rotation, Scale)
```bash
unity command set_transform \
  --target "/MyFolder/DemoCube" \
  --position "[0, 2, 0]" \
  --rotation "[0, 45, 0]" \
  --scale "[1.5, 1.5, 1.5]"
```

### 3.4 Read Component Properties
```bash
unity command get_component_properties \
  --target "/MyFolder/DemoCube" \
  --type "Transform"
```

### 3.5 Add Components
```bash
# Add physics Rigidbody
unity command add_component --target "/MyFolder/DemoCube" --type "Rigidbody"

# Add custom gameplay script
unity command add_component --target "/MyFolder/DemoCube" --type "UnityLearningLab.LearningDemo"
```

### 3.6 Save Scene & Project Changes
```bash
unity command save_scene
unity command save_all
```

---

## 4. Live C# Script Evaluation (`eval`)

Run arbitrary C# expressions in the live engine without saving or compiling files:

```bash
# Query active Unity version
unity command eval 'return Application.unityVersion;'

# Inspect GameObject position
unity command eval 'return GameObject.Find("DemoCube").transform.position.ToString();'

# Change component properties live
unity command eval 'GameObject.Find("DemoCube").GetComponent<MeshRenderer>().material.color = Color.red;'

# Instantiate or spawn objects dynamically
unity command eval 'new GameObject("DynamicAgentObject");'
```

---

## 5. PlayMode Verification Loop

Entering Play Mode is step 1; verifying real runtime behavior requires an ordered loop:

### Step 1: Start Play Mode
```bash
unity command editor_play
```

### Step 2: Confirm Game Loop is Advancing
Check that frames are ticking:
```bash
unity command eval 'return $"Frame: {Time.frameCount}, Time: {Time.time:F2}";'
```

### Step 3: Inspect Logs & Errors
```bash
unity command console --json
```
Inspect `counts.error`, `counts.warn`, and recent log messages from your scripts.

### Step 4: Capture Viewport Screenshot
```bash
unity command screenshot --output ./game_view.png --width 1920 --height 1080
```

### Step 5: Pause or Stop Play Mode
```bash
# Pause
unity command editor_pause

# Stop
unity command editor_stop
```

---

## 6. Authoring Custom `[CliCommand]` Tools

You can extend the tools exposed to the CLI and Antigravity MCP by adding `static` methods tagged with `[CliCommand]` in any `Editor/` script:

```csharp
using Unity.Pipeline.Commands;
using UnityEngine;

namespace MyProject.Editor
{
    public static class GameCommands
    {
        [CliCommand("spawn_enemy_wave", "Spawns a wave of enemies at the specified location", MainThreadRequired = true)]
        public static string SpawnEnemyWave(
            [CliArg("count", "Number of enemies to spawn")] int count = 5,
            [CliArg("enemy_type", "Enemy prefab name")] string enemyType = "Zombie")
        {
            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"{enemyType}_{i}";
                go.transform.position = new Vector3(i * 2f, 1f, 0f);
            }
            return $"Spawned {count} {enemyType} units successfully.";
        }
    }
}
```

### Rules for Custom Commands:
1. **Namespace:** `Unity.Pipeline.Commands`
2. **Method:** Must be `public static`
3. **Location:** Must be placed in an `Editor/` folder or an assembly definition targeting the Editor.
4. **MainThreadRequired:** Set to `true` (default) whenever interacting with GameObjects, components, or scenes.
5. **Auto-Discovery:** Once compiled, run `unity list`—your command will appear automatically and be immediately available as an MCP tool.

---

## 7. Project Lifecycle Management (Create, Open, Register, Delete)

### 7.1 Create a New Project (Agent-Ready)
Always pass `--with-pipeline` so AI agents can drive the Editor, and `--no-cloud` to avoid interactive OAuth cloud prompts:

```bash
# Create with specific template and editor version:
unity projects create MyGame \
  --path "C:/Projects" \
  --editor-version 7000.0.0a10 \
  --template com.unity.template.urp-blank \
  --with-pipeline \
  --no-cloud
```

### 7.2 Open an Existing Project
```bash
# Open by registered project name
unity open MyGame

# Open by full filesystem path
unity open "C:/Projects/MyGame"

# Open with specific Editor version
unity open "C:/Projects/MyGame" --editor-version 7000.0.0a10
```

### 7.3 Close an Open Project (Graceful or Force)
```bash
# Graceful close (waits up to 30s)
unity close MyGame

# Force close if unresponsive
unity close MyGame --force
```

### 7.4 Register an Existing Folder in Unity Hub
```bash
unity projects add "C:/Projects/DownloadedProject"
```

### 7.5 Remove from Hub Registry vs. Complete Deletion
* **Remove from Hub only (Keeps files on disk):**
  ```bash
  unity projects remove "C:/Projects/MyGame" --force
  ```
* **Completely Delete from Disk & Hub:**
  ```powershell
  unity projects remove "C:/Projects/MyGame" --force
  Remove-Item -Path "C:/Projects/MyGame" -Recurse -Force
  ```

### 7.6 Upgrade a Project to a Newer Editor Version
```bash
unity projects upgrade "C:/Projects/MyGame" --to 7000.0.0a10
```

---

## 8. Editor Lifecycle Management (Search, Install, Set Default, Uninstall, Prune)

### 8.1 List Available Releases
```bash
# List all online releases (LTS, Beta, Alpha)
unity releases --json

# List only installed Editors
unity editors list --installed --json
```

### 8.2 Install a New Editor via CLI
```bash
# Unattended install with automatic license agreement
unity install 7000.0.0a10 -y --accept-eula

# Avoid Windows UAC elevation issues by changing install path:
unity install-path --set "C:/Unity/Hub/Editor"
unity install 7000.0.0a10 -y --accept-eula --no-elevate
```

### 8.3 Set Default Editor
```bash
unity editors default 7000.0.0a10
```

### 8.4 Register an External / Manually Extracted Editor
If an Editor was downloaded manually or moved:
```bash
unity editors add "C:/Unity/Hub/Editor/7000.0.0a10/Editor/Unity.exe"
```

### 8.5 Uninstall an Editor
```bash
# Uninstall specific version
unity uninstall 7000.0.0a10 -y

# Prune all editors that no registered project is using
unity editors prune --remove --yes --force
```

### 8.6 Handling Missing Uninstaller on Windows
If an Editor install was copied manually or is missing `Uninstall.exe` in `C:\Program Files`, Windows UAC will block standard deletion scripts.
**Solution:**
1. Exclude the folder from Unity Hub detection via `$env:APPDATA\UnityHub\excludedEditorFolders.json`.
2. Delete the folder manually in File Explorer with administrator confirmation (Shift + Delete).

---

## 9. Troubleshooting & Edge Cases

- **Editor not detected (`STATUS_NO_INSTANCES`):** Make sure the Unity Editor has the project open and is not stuck in a modal dialog.
- **Compile Errors (Safe Mode):** When C# compile errors exist, the Editor boots into Safe Mode where the Pipeline package does not load. Check `Packages/manifest.json` and fix script syntax before restarting Unity.
- **Multiple Editors Open (`AMBIGUOUS_EDITOR`):** Pass `--project-path <full_path>` to all commands to target the specific project instance.
- **Windows Elevation (`İstek desteklenmiyor`):** Avoid installing into `C:\Program Files` from non-interactive agent scripts; use a user-writable path like `C:\Unity\Hub\Editor` via `unity install-path --set`.
