using UnityEditor;
using UnityEngine;

public class LevelValidatorMenu
{
    private const string DatabasePath = "Assets/LoopPuzzle/Data/LevelDatabase.asset";

    [MenuItem("Tools/Loop Puzzle/Validate All Levels")]
    public static void ValidateAll()
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError("LevelDatabase not found at " + DatabasePath + ". Run 'Setup Iteration 6' first.");
            return;
        }

        int total = db.levels.Count;
        int failures = 0;
        int seenSignatures = 0;
        var signatures = new System.Collections.Generic.HashSet<string>();
        int duplicates = 0;

        foreach (LevelData lv in db.levels)
        {
            if (lv == null) { failures++; Debug.LogError("Null level in database."); continue; }

            LevelValidator.Result res = LevelValidator.Validate(lv);
            if (!res.ok)
            {
                failures++;
                foreach (string e in res.errors) Debug.LogError($"[{lv.name}] {e}");
            }

            string sig = BuildSignature(lv);
            if (signatures.Contains(sig))
            {
                duplicates++;
                Debug.LogError($"[{lv.name}] DUPLICATE layout - not unique.");
            }
            else
            {
                signatures.Add(sig);
                seenSignatures++;
            }
        }

        if (failures == 0 && duplicates == 0)
        {
            Debug.Log($"<color=#7CFC00>Validate All Levels: all {total} levels valid and unique.</color>");
        }
        else
        {
            Debug.LogError($"Validate All Levels: {failures} validation failure(s), {duplicates} duplicate(s) out of {total} levels.");
        }
    }

    private static string BuildSignature(LevelData lv)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(lv.width).Append('x').Append(lv.height).Append('|');
        for (int i = 0; i < lv.cells.Length; i++)
        {
            CellDefinition c = lv.cells[i];
            if (c != null && c.pieceType != PieceType.None)
            {
                sb.Append(i).Append(':').Append((int)c.pieceType).Append(',').Append(c.rotationSteps).Append(';');
            }
        }
        return sb.ToString();
    }
}
