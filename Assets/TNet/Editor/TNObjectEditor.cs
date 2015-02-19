//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TNObject), true)]
public class TNObjectEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		TNObject obj = target as TNObject;

		if (Application.isPlaying)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField("ID", obj.uid.ToString());
			EditorGUILayout.LabelField("Player Owner", obj.ownerID.ToString());
			EditorGUILayout.Toggle("Is Mine?", obj.isMine);
			if (obj.parent != null) EditorGUILayout.ObjectField("Parent", obj.parent, typeof(TNObject), true);
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			serializedObject.Update();

			SerializedProperty sp = serializedObject.FindProperty("id");
			EditorGUILayout.PropertyField(sp, new GUIContent("ID"));
			serializedObject.ApplyModifiedProperties();

			if (obj.uid == 0)
			{
				EditorGUILayout.HelpBox("Object ID of '0' means this object must be dynamically instantiated via TNManager.Create.", MessageType.Info);
			}
			else
			{
				TNObject[] tnos = FindObjectsOfType<TNObject>();

				foreach (TNObject o in tnos)
				{
					if (o == obj || o.parent != null) continue;

					if (o.uid == obj.uid)
					{
						EditorGUILayout.HelpBox("This ID is shared with other TNObjects. A unique ID is required in order for Remote Function Calls to function properly.", MessageType.Error);
						break;
					}
				}
			}
		}
	}
}
