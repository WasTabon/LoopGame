using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Iteration4Setup
{
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 4")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        Camera cam = Camera.main;
        Debug.Assert(cam != null, "Main Camera missing on Game scene.");
        if (cam.GetComponent<CameraShake>() == null)
        {
            cam.gameObject.AddComponent<CameraShake>();
        }

        GridManager grid = Object.FindObjectOfType<GridManager>();
        Debug.Assert(grid != null, "GridManager missing. Run earlier setups first.");

        LevelController controller = Object.FindObjectOfType<LevelController>();
        Debug.Assert(controller != null, "LevelController missing. Run Setup Iteration 3 first.");

        LoopFlowAnimator animator = Object.FindObjectOfType<LoopFlowAnimator>();
        if (animator == null)
        {
            GameObject animGo = new GameObject("LoopFlowAnimator");
            animator = animGo.AddComponent<LoopFlowAnimator>();
        }
        animator.gridManager = grid;

        controller.flowAnimator = animator;

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(cam.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Iteration 4 setup complete. Loop flow animation and camera shake wired.");
    }
}
