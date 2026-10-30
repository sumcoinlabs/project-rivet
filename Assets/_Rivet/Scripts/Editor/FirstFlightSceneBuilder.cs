// Editor-only deterministic scene creation. Run from menu or Unity -executeMethod.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectRivet.Editor
{
    public static class FirstFlightSceneBuilder
    {
        public const string ScenePath = "Assets/_Rivet/Scenes/FirstFlight.unity";

        [MenuItem("Project Rivet/Build First Flight Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log("Project Rivet: First Flight scene already exists; refusing to overwrite it.");
                return;
            }
            const string folder = "Assets/_Rivet";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "_Rivet");
            if (!AssetDatabase.IsValidFolder(folder + "/Scenes"))
                AssetDatabase.CreateFolder(folder, "Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("First Flight Game Manager").AddComponent<ProjectRivet.FirstFlightGame>();
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.transform.position = new Vector3(0, 2, -18);
            camera.backgroundColor = new Color(0.49f, 0.76f, 0.96f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            GameObject sun = new GameObject("Sunlight");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            sun.transform.rotation = Quaternion.Euler(40f, -30f, 10f);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.Exception("Could not save " + ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project Rivet: First Flight ready at " + ScenePath + " (3 runtime courses, Input System, URP).");
        }
    }
}
