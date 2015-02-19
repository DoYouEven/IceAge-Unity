//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using TNet;
using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Inspector class used to view and edit TNAutoSync.
/// </summary>

[CustomEditor(typeof(TNAutoSync))]
public class TNAutoSyncInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		TNAutoSync sync = target as TNAutoSync;

		List<Component> components = GetComponents(sync);
		string[] names = GetComponentNames(components);

		for (int i = 0; i < sync.entries.Count; )
		{
			GUILayout.BeginHorizontal();
			{
				if (DrawTarget(sync, i, components, names))
				{
					DrawProperties(sync, sync.entries[i]);
					++i;
				}
			}
			GUILayout.EndHorizontal();
		}

		GUI.backgroundColor = Color.green;

		if (GUILayout.Button("Add a New Synchronized Property"))
		{
			TNAutoSync.SavedEntry ent = new TNAutoSync.SavedEntry();
			ent.target = components[0];
			sync.entries.Add(ent);
			EditorUtility.SetDirty(sync);
		}
		GUI.backgroundColor = Color.white;

		GUILayout.Space(4f);
		int updates = EditorGUILayout.IntField("Updates Per Second", sync.updatesPerSecond);
		bool persistent = EditorGUILayout.Toggle("Saved On Server", sync.isSavedOnServer);
		bool important = EditorGUILayout.Toggle("Important", sync.isImportant);
		bool owner = EditorGUILayout.Toggle("Only Owner Can Sync", sync.onlyOwnerCanSync);

		if (sync.updatesPerSecond != updates ||
			sync.isSavedOnServer != persistent ||
			sync.isImportant != important ||
			sync.onlyOwnerCanSync != owner)
		{
			sync.updatesPerSecond = updates;
			sync.isSavedOnServer = persistent;
			sync.isImportant = important;
			sync.onlyOwnerCanSync = owner;
			EditorUtility.SetDirty(sync);
		}
	}

	static List<Component> GetComponents (TNAutoSync sync)
	{
		Component[] comps = sync.GetComponents<Component>();

		List<Component> list = new List<Component>();

		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			if (comps[i] != sync && comps[i].GetType() != typeof(TNObject))
			{
				list.Add(comps[i]);
			}
		}
		return list;
	}

	static string[] GetComponentNames (List<Component> list)
	{
		string[] names = new string[list.size + 1];
		names[0] = "<None>";
		for (int i = 0; i < list.size; ++i)
			names[i + 1] = list[i].GetType().ToString();
		return names;
	}

	static bool DrawTarget (TNAutoSync sync, int index, List<Component> components, string[] names)
	{
		TNAutoSync.SavedEntry ent = sync.entries[index];
		
		if (ent.target == null)
		{
			ent.target = components[0];
			EditorUtility.SetDirty(sync);
		}

		int oldIndex = 0;
		string tname = (ent.target != null) ? ent.target.GetType().ToString() : "<None>";
		
		for (int i = 1; i < names.Length; ++i)
		{
			if (names[i] == tname)
			{
				oldIndex = i;
				break;
			}
		}

		GUI.backgroundColor = Color.red;
		bool delete = GUILayout.Button("X", GUILayout.Width(24f));
		GUI.backgroundColor = Color.white;
		int newIndex = EditorGUILayout.Popup(oldIndex, names);

		if (delete)
		{
			sync.entries.RemoveAt(index);
			EditorUtility.SetDirty(sync);
			return false;
		}

		if (newIndex != oldIndex)
		{
			ent.target = (newIndex == 0) ? null : components[newIndex - 1];
			ent.propertyName = "";
			EditorUtility.SetDirty(sync);
		}
		return true;
	}

	static void DrawProperties (TNAutoSync sync, TNAutoSync.SavedEntry saved)
	{
		if (saved.target == null) return;

		FieldInfo[] fields = saved.target.GetType().GetFields(
			BindingFlags.Instance | BindingFlags.Public);

		PropertyInfo[] properties = saved.target.GetType().GetProperties(
			BindingFlags.Instance | BindingFlags.Public);

		int oldIndex = 0;
		List<string> names = new List<string>();
		names.Add("<None>");

		for (int i = 0; i < fields.Length; ++i)
		{
			if (fields[i].Name == saved.propertyName) oldIndex = names.size;
			names.Add(fields[i].Name);
		}
		
		for (int i = 0; i < properties.Length; ++i)
		{
			PropertyInfo pi = properties[i];

			if (pi.CanWrite && pi.CanRead)
			{
				if (pi.Name == saved.propertyName) oldIndex = names.size;
				names.Add(pi.Name);
			}
		}

		int newIndex = EditorGUILayout.Popup(oldIndex, names.ToArray(), GUILayout.Width(90f));

		if (newIndex != oldIndex)
		{
			saved.propertyName = (newIndex == 0) ? "" : names[newIndex];
			EditorUtility.SetDirty(sync);
		}
	}
}
