#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(RebindActionRow))]
public class RebindActionRowEditor : Editor
{
    private SerializedProperty _actionPath;
    private SerializedProperty _bindingIndex;
    private SerializedProperty _actionsAsset;

    private List<string> _paths = new();
    private int _pathIndex = -1;

    private void OnEnable()
    {
        _actionPath = serializedObject.FindProperty("actionPath");
        _bindingIndex = serializedObject.FindProperty("bindingIndex");
        _actionsAsset = serializedObject.FindProperty("actionsAsset");

        RebuildPaths();
        SyncIndexFromPath();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 基础 UI 引用（默认 Inspector 会显示其它 SerializeField，我们这里聚焦 Binding）
        EditorGUILayout.LabelField("Rebind Action Row", EditorStyles.boldLabel);

        DrawActionsAssetField();
        DrawActionDropdown();
        DrawBindingDropdown();

        // 其它字段照常显示（UI Text/Button 引用等）
        EditorGUILayout.Space(8);
        DrawPropertiesExcluding(serializedObject,
            "m_Script",
            "actionPath",
            "bindingIndex",
            "actionsAsset"
        );

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawActionsAssetField()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_actionsAsset, new GUIContent("Input Actions Asset"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            RebuildPaths();
            SyncIndexFromPath();
            serializedObject.Update();
        }

        if (_actionsAsset.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "Assign your InputActionAsset here (the .inputactions file). " +
                "Then you can select actions from the dropdown.",
                MessageType.Info);

            if (GUILayout.Button("Auto-Find First InputActionAsset in Project"))
            {
                var found = FindFirstInputActionAsset();
                if (found != null)
                {
                    _actionsAsset.objectReferenceValue = found;
                    serializedObject.ApplyModifiedProperties();
                    RebuildPaths();
                    SyncIndexFromPath();
                    serializedObject.Update();
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Found", "No InputActionAsset found in this project.", "OK");
                }
            }
        }
    }

    private void DrawActionDropdown()
    {
        using (new EditorGUI.DisabledScope(_actionsAsset.objectReferenceValue == null))
        {
            if (GUILayout.Button("Refresh Action List"))
            {
                RebuildPaths();
                SyncIndexFromPath();
            }

            if (_paths.Count == 0)
            {
                EditorGUILayout.HelpBox("No actions found in the selected InputActionAsset.", MessageType.Warning);
                return;
            }

            var currentPath = _actionPath.stringValue ?? "";
            _pathIndex = Mathf.Clamp(_pathIndex, 0, _paths.Count - 1);

            // 如果当前 path 不在列表里，用一个“自定义”提示
            int indexFromPath = _paths.IndexOf(currentPath);
            if (indexFromPath >= 0) _pathIndex = indexFromPath;

            EditorGUI.BeginChangeCheck();
            _pathIndex = EditorGUILayout.Popup("Action (Map/Action)", _pathIndex, _paths.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                _actionPath.stringValue = _paths[_pathIndex];
                // 选新 action 后，bindingIndex 如果越界，先归零
                _bindingIndex.intValue = 0;
                serializedObject.ApplyModifiedProperties();

                // 重新绘制 binding 列表
                serializedObject.Update();
            }

            EditorGUILayout.LabelField("actionPath", _actionPath.stringValue);
        }
    }

    private void DrawBindingDropdown()
    {
        var asset = _actionsAsset.objectReferenceValue as InputActionAsset;
        if (asset == null) return;

        var action = asset.FindAction(_actionPath.stringValue, throwIfNotFound: false);
        if (action == null)
        {
            EditorGUILayout.HelpBox("Action not found in this asset. Please select a valid action.", MessageType.Warning);
            return;
        }

        var bindingOptions = new List<string>();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];

            string flags = "";
            if (b.isComposite) flags = " [Composite]";
            else if (b.isPartOfComposite) flags = " [Part]";

            // display string（人类可读）
            string display = action.GetBindingDisplayString(i);
            if (string.IsNullOrEmpty(display)) display = b.effectivePath;

            bindingOptions.Add($"{i}: {display}{flags}");
        }

        if (bindingOptions.Count == 0)
        {
            EditorGUILayout.HelpBox("This action has no bindings.", MessageType.Warning);
            return;
        }

        int idx = Mathf.Clamp(_bindingIndex.intValue, 0, bindingOptions.Count - 1);

        EditorGUI.BeginChangeCheck();
        idx = EditorGUILayout.Popup("Binding Index", idx, bindingOptions.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            _bindingIndex.intValue = idx;
        }

        // 提示：Composite 绑定需要更高级的重绑（我们之后可以做）
        if (action.bindings[idx].isComposite || action.bindings[idx].isPartOfComposite)
        {
            EditorGUILayout.HelpBox(
                "You selected a Composite/Part binding. Simple rebinding may not behave as expected for Move (2DVector). " +
                "Recommend rebinding Button-type actions first (Jump/Interact/Pause).",
                MessageType.Info);
        }
    }

    private void RebuildPaths()
    {
        _paths.Clear();

        var asset = _actionsAsset.objectReferenceValue as InputActionAsset;
        if (asset == null) return;

        foreach (var map in asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                _paths.Add($"{map.name}/{action.name}");
            }
        }

        _paths = _paths.Distinct().OrderBy(p => p).ToList();
    }

    private void SyncIndexFromPath()
    {
        if (_paths.Count == 0) { _pathIndex = -1; return; }
        var p = _actionPath.stringValue ?? "";
        int idx = _paths.IndexOf(p);
        _pathIndex = idx >= 0 ? idx : 0;
    }

    private static InputActionAsset FindFirstInputActionAsset()
    {
        var guids = AssetDatabase.FindAssets("t:InputActionAsset");
        if (guids == null || guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
    }
}
#endif
