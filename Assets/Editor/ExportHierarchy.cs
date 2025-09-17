// Assets/Editor/ExportHierarchy.cs
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExportHierarchy
{
    [MenuItem("Tools/Dayvive/Export Hierarchy (active scene)")]
    public static void Export()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        var sb = new StringBuilder();
        sb.AppendLine($"# Hierarchy – {scene.name}");
        foreach (var go in roots)
            Dump(go.transform, 0, sb);
        var path = "Assets/Hierarchy_export.txt";
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"Exported hierarchy to: {path}");
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        sb.Append(' ', depth * 2).Append("└─ ").AppendLine(t.name);
        for (int i = 0; i < t.childCount; i++)
            Dump(t.GetChild(i), depth + 1, sb);
    }
}
