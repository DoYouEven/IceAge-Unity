//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using TNet;
using System.Collections;
using System.Reflection;

/// <summary>
/// This script makes it really easy to sync some value across all connected clients.
/// Keep in mind that this script should ideally only be used for rapid prototyping.
/// It's still better to create custom to-the-point sync scripts as they will yield
/// better performance.
/// </summary>

[ExecuteInEditMode]
public class TNAutoSync : TNBehaviour
{
	[System.Serializable]
	public class SavedEntry
	{
		public Component target;
		public string propertyName;
	}

	/// <summary>
	/// Serialized synchronized entries.
	/// </summary>

	public System.Collections.Generic.List<SavedEntry> entries = new System.Collections.Generic.List<SavedEntry>();

	/// <summary>
	/// Maximum possible number of updates per second. If the values don't change, nothing will be sent.
	/// If to set it to zero, the value will only be synchronized when new players join.
	/// </summary>

	public int updatesPerSecond = 10;

	/// <summary>
	/// Whether the result will be saved on the server or not. In most cases it should remain as 'true'.
	/// In any case the values will be sent to newly joined players automatically.
	/// </summary>

	public bool isSavedOnServer = true;

	/// <summary>
	/// Whether only the object's owner can send sync messages. In most cases it should remain as 'true'.
	/// </summary>

	public bool onlyOwnerCanSync = true;

	/// <summary>
	/// Whether to send through UDP or TCP. If it's important, TCP will be used. If not, UDP.
	/// If you have a lot of frequent updates, leave it as not important.
	/// </summary>

	public bool isImportant = false;

	class ExtendedEntry : SavedEntry
	{
		public FieldInfo field;
		public PropertyInfo property;
		public object lastValue;
	}

	List<ExtendedEntry> mList = new List<ExtendedEntry>();
	object[] mCached = null;

	/// <summary>
	/// Locate the property that we should be synchronizing.
	/// </summary>

	void Awake ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			TNAutoSync[] tns = GetComponents<TNAutoSync>();

			if (tns.Length > 1 && tns[0] != this)
			{
				Debug.LogError("Can't have more than one " + GetType() + " per game object", gameObject);
				DestroyImmediate(this);
			}
		}
		else
#endif
		{
			// Find all properties, converting the saved list into the usable list of reflected properties
			for (int i = 0, imax = entries.Count; i < imax; ++i)
			{
				SavedEntry ent = entries[i];
				
				if (ent.target != null && !string.IsNullOrEmpty(ent.propertyName))
				{
					FieldInfo field = ent.target.GetType().GetField(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);

					if (field != null)
					{
						ExtendedEntry ext = new ExtendedEntry();
						ext.target = ent.target;
						ext.field = field;
						ext.lastValue = field.GetValue(ent.target);
						mList.Add(ext);
						continue;
					}
					else
					{
						PropertyInfo pro = ent.target.GetType().GetProperty(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);

						if (pro != null)
						{
							ExtendedEntry ext = new ExtendedEntry();
							ext.target = ent.target;
							ext.property = pro;
							ext.lastValue = pro.GetValue(ent.target, null);
							mList.Add(ext);
							continue;
						}
						else Debug.LogError("Unable to find property: '" + ent.propertyName + "' on " + ent.target.GetType());
					}
				}
			}

			if (mList.size > 0)
			{
				if (updatesPerSecond > 0)
					StartCoroutine(PeriodicSync());
			}
			else
			{
				Debug.LogWarning("Nothing to sync", this);
				enabled = false;
			}
		}
	}

	/// <summary>
	/// Sync periodically.
	/// </summary>

	IEnumerator PeriodicSync ()
	{
		for (; ; )
		{
			if (TNManager.isInChannel && updatesPerSecond > 0)
			{
				if (mList.size != 0 && (!onlyOwnerCanSync || tno.isMine) && Cache()) Sync();
				yield return new WaitForSeconds(1f / updatesPerSecond);
			}
			else yield return new WaitForSeconds(0.1f);
		}
	}

	/// <summary>
	/// If this values are not saved on the server, at least send them to the newly joined player.
	/// </summary>

	void OnNetworkPlayerJoin (Player p)
	{
		if (mList.size != 0 && !isSavedOnServer && TNManager.isHosting)
		{
			if (Cache()) Sync();
			else tno.Send(255, p, mCached);
		}
	}

	/// <summary>
	/// Immediately cache all synchronized values and return whether something actually changed.
	/// </summary>

	bool Cache ()
	{
		bool initial = false;
		bool changed = false;

		if (mCached == null)
		{
			initial = true;
			mCached = new object[mList.size];
		}

		for (int i = 0; i < mList.size; ++i)
		{
			ExtendedEntry ext = mList[i];

			object val = (ext.field != null) ?
				val = ext.field.GetValue(ext.target) :
				val = ext.property.GetValue(ext.target, null);

			if (!val.Equals(ext.lastValue))
				changed = true;

			if (initial || changed)
			{
				ext.lastValue = val;
				mCached[i] = val;
			}
		}
		return changed;
	}

	/// <summary>
	/// Immediately synchronize all data by sending current values to everyone else.
	/// </summary>

	public void Sync ()
	{
		if (TNManager.isInChannel && mList.size != 0)
		{
			if (isImportant) tno.Send(255, isSavedOnServer ? Target.OthersSaved : Target.Others, mCached);
			else tno.SendQuickly(255, isSavedOnServer ? Target.OthersSaved : Target.Others, mCached);
		}
	}

	/// <summary>
	/// The actual synchronization function function.
	/// </summary>

	[RFC(255)]
	void OnSync (object[] val)
	{
		if (enabled)
		{
			for (int i = 0; i < mList.size; ++i)
			{
				ExtendedEntry ext = mList[i];
				ext.lastValue = val[i];
				if (ext.field != null) ext.field.SetValue(ext.target, ext.lastValue);
				else ext.property.SetValue(ext.target, ext.lastValue, null);
			}
		}
	}
}
