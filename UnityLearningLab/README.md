# Unity 7 Learning Lab: Skills & MCP Integration Guide

Welcome to the **UnityLearningLab** project! This repository is configured to teach you how to control, inspect, and automate the **Unity Editor** using the **Unity CLI** and **Antigravity MCP (Model Context Protocol)** tools.

---

## 🛠️ Architecture Overview

```
                      +-----------------------------------+
                      |     Antigravity AI Agent / IDE    |
                      +-----------------+-----------------+
                                        | (stdio / MCP)
                                        v
                      +-----------------------------------+
                      |       Unity CLI (`unity.exe`)     |
                      |   Starts MCP server (`unity mcp`) |
                      +-----------------+-----------------+
                                        | (Local Pipeline IPC)
                                        v
                      +-----------------------------------+
                      |      Unity 7 Editor (Running)     |
                      |   Package: `com.unity.pipeline`   |
                      +-----------------------------------+
```

1. **Unity 7 (`7000.0.0a10`)**: The modern CoreCLR-based Unity Editor.
2. **`com.unity.pipeline`**: A lightweight local package inside `Packages/manifest.json` that hosts an IPC endpoint inside the running Editor.
3. **Unity CLI (`unity.exe`)**: Communicates with the Pipeline package over localhost.
4. **Unity MCP**: Automatically exposes the Editor's commands (and any custom `[CliCommand]` you write) as dynamic MCP tools directly to Antigravity!

---

## 🚀 Step 1: Open the Project in Unity Hub

1. Open **Unity Hub**.
2. Click on the **Projects** tab.
3. Select **`UnityLearningLab`** to open it with Unity 7.
4. Wait for the Editor to finish importing and reach its main window.

---

## 🕹️ Step 2: Essential Unity CLI Commands

Once the Editor is open, you (or the AI agent) can run these commands from the terminal:

### 1. Check Editor Connection Status
```powershell
unity status
```
*Look for state: `ready`.*

### 2. List Available Commands (Discovered dynamically from Editor)
```powershell
unity list
```
*Lists all tools, parameters, descriptions, and custom tools.*

### 3. Enter & Exit Play Mode
```powershell
# Start Play Mode
unity command editor_play

# Stop Play Mode
unity command editor_play_stop
```

### 4. Run Arbitrary C# Code (Live Eval without Recompiling)
```powershell
# Inspect the Unity version live:
unity command eval 'return Application.unityVersion;'

# Spawn a GameObject dynamically:
unity command eval 'new UnityEngine.GameObject("MyAgentObject");'

# Tune rotation speed on our demo script:
unity command eval 'GameObject.Find("DemoCube")?.GetComponent<UnityLearningLab.LearningDemo>()?.SetSpeed(5f);'
```

### 5. Capture Scene & Game View Screenshots
```powershell
unity command screenshot --output ./screenshot.png --width 1920 --height 1080
```

---

## 🧩 Step 3: Authoring Custom `[CliCommand]` Tools

You can extend the tools available to both the CLI and MCP by adding `static` methods tagged with `[CliCommand]` in an Editor script (see [`AgentCustomCommands.cs`](Assets/Scripts/Editor/AgentCustomCommands.cs)):

```csharp
using Unity.Pipeline.Commands;
using UnityEngine;

public static class MyCommands
{
    [CliCommand("spawn_arena", "Spawns a test arena for AI agents", MainThreadRequired = true)]
    public static string SpawnArena([CliArg("name", "Arena name")] string name = "DefaultArena")
    {
        var go = new GameObject(name);
        return $"Created {go.name}";
    }
}
```

Once written, recompile or switch focus to Unity, and run:
```powershell
unity command spawn_learning_arena
```
It immediately becomes an MCP tool that Antigravity can call!

---

## 🤖 Step 4: Using MCP Inside Antigravity

In Antigravity chat, you can simply ask:
- *"Can you inspect what GameObjects are in the scene?"*
- *"Can you spawn the learning arena and put the editor in Play Mode?"*
- *"Can you take a screenshot of the Game View to verify how the cube looks?"*

The agent calls the Unity MCP server behind the scenes and returns the results directly in the conversation.
