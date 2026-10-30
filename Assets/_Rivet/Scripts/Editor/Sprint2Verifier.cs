// Project Rivet — Sprint 2 edit-mode smoke test, executed after C# compilation.
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectRivet.Editor
{
    public static class Sprint2Verifier
    {
        public static void Verify()
        {
            const string scene = "Assets/_Rivet/Scenes/FirstFlight.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
                throw new Exception("First Flight scene missing: " + scene);
            EditorSceneManager.OpenScene(scene);
            FirstFlightGame game = UnityEngine.Object.FindFirstObjectByType<FirstFlightGame>();
            if (game == null)
                throw new Exception("No FirstFlightGame manager found in saved scene.");
            if (typeof(WingoController).GetMethod("Setup") == null)
                throw new Exception("Expected WingoController API missing.");
            Debug.Log("PROJECT RIVET SPRINT 2 SMOKE TEST PASS — scene opens and updated C# game systems compile.");
        }
    }
}
