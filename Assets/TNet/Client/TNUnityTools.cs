//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using System.Reflection;

namespace TNet
{
/// <summary>
/// Common Tasharen Network-related functionality and helper functions to be used with Unity.
/// </summary>

static public class UnityTools
{
	/// <summary>
	/// Clear the array references.
	/// </summary>

	static public void Clear (object[] objs)
	{
		for (int i = 0, imax = objs.Length; i < imax; ++i)
			objs[i] = null;
	}

	/// <summary>
	/// Print out useful information about an exception that occurred when trying to call a function.
	/// </summary>

	static void PrintException (System.Exception ex, CachedFunc ent, int funcID, string funcName, params object[] parameters)
	{
		string received = "";

		if (parameters != null)
		{
			for (int b = 0; b < parameters.Length; ++b)
			{
				if (b != 0) received += ", ";
				received += parameters[b].GetType().ToString();
			}
		}

		string expected = "";

		if (ent.parameters != null)
		{
			for (int b = 0; b < ent.parameters.Length; ++b)
			{
				if (b != 0) expected += ", ";
				expected += ent.parameters[b].ParameterType.ToString();
			}
		}

		string err = "[TNet] Failed to call RFC ";
		if (string.IsNullOrEmpty(funcName)) err += "#" + funcID + " on " + (ent.obj != null ? ent.obj.GetType().ToString() : "<null>");
		else err += ent.obj.GetType() + "." + funcName;

		if (ex.InnerException != null) err += ": " + ex.InnerException.Message + "\n";
		else err += ": " + ex.Message + "\n";

		if (received != expected)
		{
			err += "  Expected args: " + expected + "\n";
			err += "  Received args: " + received + "\n\n";
		}

		if (ex.InnerException != null) err += ex.InnerException.StackTrace + "\n";
		else err += ex.StackTrace + "\n";

		Debug.LogError(err);
	}

	/// <summary>
	/// Execute the first function matching the specified ID.
	/// </summary>

	static public bool ExecuteFirst (List<CachedFunc> rfcs, byte funcID, out object retVal, params object[] parameters)
	{
		retVal = null;

		for (int i = 0; i < rfcs.size; ++i)
		{
			CachedFunc ent = rfcs[i];

			if (ent.id == funcID)
			{
				if (ent.parameters == null)
					ent.parameters = ent.func.GetParameters();

				try
				{
					retVal = (ent.parameters.Length == 1 && ent.parameters[0].ParameterType == typeof(object[])) ?
						ent.func.Invoke(ent.obj, new object[] { parameters }) :
						ent.func.Invoke(ent.obj, parameters);
					return (retVal != null);
				}
				catch (System.Exception ex)
				{
					PrintException(ex, ent, funcID, "", parameters);
					return false;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Invoke the function specified by the ID.
	/// </summary>

	static public bool ExecuteAll (List<CachedFunc> rfcs, byte funcID, params object[] parameters)
	{
		for (int i = 0; i < rfcs.size; ++i)
		{
			CachedFunc ent = rfcs[i];

			if (ent.id == funcID)
			{
				if (ent.parameters == null)
					ent.parameters = ent.func.GetParameters();

				try
				{
					if (ent.parameters.Length == 1 && ent.parameters[0].ParameterType == typeof(object[]))
					{
						ent.func.Invoke(ent.obj, new object[] { parameters });
					}
					else ent.func.Invoke(ent.obj, parameters);
					return true;
				}
				catch (System.Exception ex)
				{
					PrintException(ex, ent, funcID, "", parameters);
					return false;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Invoke the function specified by the function name.
	/// </summary>

	static public bool ExecuteAll (List<CachedFunc> rfcs, string funcName, params object[] parameters)
	{
		bool retVal = false;

		for (int i = 0; i < rfcs.size; ++i)
		{
			CachedFunc ent = rfcs[i];

			if (ent.func.Name == funcName)
			{
				retVal = true;

				if (ent.parameters == null)
					ent.parameters = ent.func.GetParameters();

				try
				{
					if (ent.parameters.Length == 1 && ent.parameters[0].ParameterType == typeof(object[]))
					{
						ent.func.Invoke(ent.obj, new object[] { parameters });
					}
					else ent.func.Invoke(ent.obj, parameters);
					return true;
				}
				catch (System.Exception ex)
				{
					PrintException(ex, ent, 0, funcName, parameters);
				}
			}
		}
		return retVal;
	}

	/// <summary>
	/// Call the specified function on all the scripts. It's an expensive function, so use sparingly.
	/// </summary>

	static public void Broadcast (string methodName, params object[] parameters)
	{
		MonoBehaviour[] mbs = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];

		for (int i = 0, imax = mbs.Length; i < imax; ++i)
		{
			MonoBehaviour mb = mbs[i];
			MethodInfo method = mb.GetType().GetMethod(methodName,
				BindingFlags.Instance |
				BindingFlags.NonPublic |
				BindingFlags.Public);

			if (method != null)
			{
#if UNITY_EDITOR
				method.Invoke(mb, parameters);
#else
				try
				{
					method.Invoke(mb, parameters);
				}
				catch (System.Exception ex)
				{
					Debug.LogError(ex.InnerException.Message + " (" + mb.GetType() + "." + methodName + ")\n" +
						ex.InnerException.StackTrace + "\n", mb);
				}
#endif
			}
		}
	}

	/// <summary>
	/// Mathf.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public float SpringLerp (float from, float to, float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		for (int i = 0; i < ms; ++i) from = Mathf.Lerp(from, to, deltaTime);
		return from;
	}

	/// <summary>
	/// Pad the specified rectangle, returning an enlarged rectangle.
	/// </summary>

	static public Rect PadRect (Rect rect, float padding)
	{
		Rect r = rect;
		r.xMin -= padding;
		r.xMax += padding;
		r.yMin -= padding;
		r.yMax += padding;
		return r;
	}

	/// <summary>
	/// Whether the specified game object is a child of the specified parent.
	/// </summary>

	static public bool IsParentChild (GameObject parent, GameObject child)
	{
		if (parent == null || child == null) return false;
		return IsParentChild(parent.transform, child.transform);
	}

	/// <summary>
	/// Whether the specified transform is a child of the specified parent.
	/// </summary>

	static public bool IsParentChild (Transform parent, Transform child)
	{
		if (parent == null || child == null) return false;

		while (child != null)
		{
			if (parent == child) return true;
			child = child.parent;
		}
		return false;
	}

	/// <summary>
	/// Convenience function that instantiates a game object and sets its velocity.
	/// </summary>

	static public GameObject Instantiate (GameObject go, Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity)
	{
		if (go != null)
		{
			go = GameObject.Instantiate(go, pos, rot) as GameObject;
			Rigidbody rb = go.GetComponent<Rigidbody>();

			if (rb != null)
			{
				if (rb.isKinematic)
				{
					rb.isKinematic = false;
					rb.velocity = velocity;
					rb.angularVelocity = angularVelocity;
					rb.isKinematic = true;
				}
				else
				{
					rb.velocity = velocity;
					rb.angularVelocity = angularVelocity;
				}
			}
		}
		return go;
	}
}
}
