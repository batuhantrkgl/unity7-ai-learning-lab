using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace UnityLearningLab.Editor
{
    /// <summary>
    /// Demonstrates how to author custom [CliCommand] tools for the Unity Pipeline.
    /// These commands become callable via:
    ///   - CLI: unity command <command_name>
    ///   - MCP: call_mcp_tool in Antigravity
    /// </summary>
    public static class AgentCustomCommands
    {
        [CliCommand("spawn_learning_arena", "Spawns a ground plane, interactive demo cube, and light for agent testing", MainThreadRequired = true)]
        public static string SpawnLearningArena(
            [CliArg("arena_name", "Name of the root arena GameObject")] string arenaName = "LearningArena")
        {
            // Root GameObject
            GameObject root = new GameObject(arenaName);
            Undo.RegisterCreatedObjectUndo(root, "Create Arena Root");

            // Ground Plane
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.parent = root.transform;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
            floor.transform.position = Vector3.zero;

            // Interactive Cube with LearningDemo script
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "DemoCube";
            cube.transform.parent = root.transform;
            cube.transform.position = new Vector3(0f, 1.5f, 0f);
            cube.AddComponent<LearningDemo>();

            // Directional Light (if none exists)
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light", typeof(Light));
                Light light = lightGo.GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightGo.transform.parent = root.transform;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Ensure Scene view focuses on the arena
            SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(0f, 1f, 0f), Vector3.one * 5f), false);

            Debug.Log($"[UnityLearningLab] Successfully spawned '{arenaName}' with Floor, DemoCube, and Light.");
            return $"Spawned {arenaName} with DemoCube and Floor.";
        }

        [CliCommand("get_scene_summary", "Returns GameObject count and active root objects in the scene", MainThreadRequired = true)]
        public static string GetSceneSummary()
        {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int totalGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;

            var names = string.Join(", ", System.Array.ConvertAll(rootObjects, go => go.name));
            return $"Total GameObjects: {totalGameObjects} | Root Objects: [{names}]";
        }
    }
}
