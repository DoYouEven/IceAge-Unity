//------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// This is an inspector class for the FillText helper class.
/// </summary>

[CustomEditor(typeof(FillText))]
public class FillTextInspector : Editor
{
	public override void OnInspectorGUI ()
	{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeControls(80f);
#else
		EditorGUIUtility.labelWidth = 80f;
#endif
		FillText fill = target as FillText;

		string text = fill.text;
		text = EditorGUILayout.TextArea(text, GUI.skin.textArea, GUILayout.Height(100f));

		if (text != fill.text)
		{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterUndo(fill, "Fill Text Change");
#else
			Undo.RecordObject(fill, "Fill Text Change");
#endif
			EditorUtility.SetDirty(fill);
			fill.text = text;
		}
	}
}
