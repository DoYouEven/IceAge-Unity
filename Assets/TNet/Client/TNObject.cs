//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System.IO;
using System.Reflection;
using UnityEngine;
using TNet;
using UnityTools = TNet.UnityTools;

/// <summary>
/// Tasharen Network Object makes it possible to easily send and receive remote function calls.
/// Unity networking calls this type of object a "Network View".
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("TNet/Network Object")]
public sealed class TNObject : MonoBehaviour
{
	static int mDummyID = 0;

	/// <summary>
	/// Remote function calls that can't be executed immediately get stored,
	/// and will be executed when an appropriate Object ID gets added.
	/// </summary>

	class DelayedCall
	{
		public uint objID;
		public byte funcID;
		public string funcName;
		public object[] parameters;
	}

	// List of network objs to iterate through
	static List<TNObject> mList = new List<TNObject>();

	// List of network objs to quickly look up
	static System.Collections.Generic.Dictionary<uint, TNObject> mDictionary =
		new System.Collections.Generic.Dictionary<uint, TNObject>();

	// List of delayed calls -- calls that could not execute at the time of the call
	static List<DelayedCall> mDelayed = new List<DelayedCall>();

	/// <summary>
	/// Unique Network Identifier. All TNObjects have them and is how messages arrive at the correct destination.
	/// The ID is supposed to be a 'uint', but Unity is not able to serialize 'uint' types. Sigh.
	/// </summary>

	[SerializeField] int id = 0;

	/// <summary>
	/// Object's unique identifier (Static object IDs range 1 to 32767. Dynamic object IDs range from 32768 to 2^24-1).
	/// </summary>

	public uint uid
	{
		get
		{
			return (mParent != null) ? mParent.uid : (uint)id;
		}
		set
		{
			if (mParent != null) mParent.uid = value;
			else id = (int)(value & 0xFFFFFF);
		}
	}

	/// <summary>
	/// TNObject's parent, if it has any.
	/// </summary>

	public TNObject parent { get { return mParent; } }

	/// <summary>
	/// When set to 'true', it will cause the list of remote function calls to be rebuilt next time they're needed.
	/// </summary>

	[System.NonSerialized][HideInInspector] public bool rebuildMethodList = true;

	// Cached RFC functions
	[System.NonSerialized] List<CachedFunc> mRFCs = new List<CachedFunc>();

	// Whether the object has been registered with the lists
	[System.NonSerialized] bool mIsRegistered = false;

	// ID of the object's owner
	[System.NonSerialized] int mOwner = -1;

	// Child objects don't get their own unique IDs, so if we have a parent TNObject, that's the object that will be getting all events.
	[System.NonSerialized] TNObject mParent = null;

	/// <summary>
	/// Whether this object belongs to the player.
	/// </summary>

	public bool isMine { get { return (mOwner == -1) ? TNManager.isThisMyObject : mOwner == TNManager.playerID; } }

	/// <summary>
	/// ID of the player that owns this object.
	/// </summary>

	public int ownerID { get { return (mParent != null) ? mParent.ownerID : mOwner; } }

	/// <summary>
	/// Destroy this game object on all connected clients and remove it from the server.
	/// </summary>

	public void DestroySelf ()
	{
		StartCoroutine(EnsureDestroy());
		TNManager.Destroy(gameObject);
	}

	/// <summary>
	/// If this function is still here in 5 seconds then something went wrong, so force-destroy the object.
	/// </summary>

	System.Collections.IEnumerator EnsureDestroy ()
	{
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
	}

	/// <summary>
	/// Remember the object's ownership, for convenience.
	/// </summary>

	void Awake ()
	{
		// Used for offline mode
		if (id == 0 && !TNManager.isConnected)
			id = ++mDummyID;

		mOwner = TNManager.objectOwnerID;

		if (TNManager.players.size == 0)
		{
			mOwner = TNManager.playerID;
		}
		else if (TNManager.GetPlayer(mOwner) == null)
		{
#if UNITY_EDITOR
			// This shouldn't happen anymore with the latest server/client version
			Debug.LogWarning("Object is missing its owner, " + mOwner, this);
#endif
			mOwner = TNManager.hostID;
		}
	}

	/// <summary>
	/// Automatically transfer the ownership. The same action happens on the server.
	/// </summary>

	void OnNetworkPlayerLeave (Player p) { if (p != null && mOwner == p.id) mOwner = TNManager.hostID; }

	/// <summary>
	/// Retrieve the Tasharen Network Object by ID.
	/// </summary>

	static public TNObject Find (uint tnID)
	{
		if (mDictionary == null) return null;
		TNObject tno = null;
		mDictionary.TryGetValue(tnID, out tno);
		return tno;
	}

#if UNITY_EDITOR
	// Last used ID
	static uint mLastID = 0;

	/// <summary>
	/// Helper function that returns the game object's hierarchy in a human-readable format.
	/// </summary>

	static public string GetHierarchy (GameObject obj)
	{
		string path = obj.name;

		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return "\"" + path + "\"";
	}

	/// <summary>
	/// Get a new unique object identifier.
	/// </summary>

	static uint GetUniqueID ()
	{
		TNObject[] tns = (TNObject[])FindObjectsOfType(typeof(TNObject));

		for (int i = 0, imax = tns.Length; i < imax; ++i)
		{
			TNObject ts = tns[i];
			if (ts != null && ts.uid > mLastID && ts.uid < 32768) mLastID = ts.uid;
		}
		return ++mLastID;
	}

	/// <summary>
	/// Make sure that this object's ID is actually unique.
	/// </summary>

	void UniqueCheck ()
	{
		if (id < 0) id = -id;
		TNObject tobj = Find(uid);

		if (id == 0 || tobj != null)
		{
			if (Application.isPlaying && TNManager.isInChannel)
			{
				if (tobj != null)
				{
					Debug.LogError("Network ID " + id + " is already in use by " +
						GetHierarchy(tobj.gameObject) +
						".\nPlease make sure that the network IDs are unique.", this);
				}
				else
				{
					Debug.LogError("Network ID of 0 is used by " + GetHierarchy(gameObject) +
						"\nPlease make sure that a unique non-zero ID is given to all objects.", this);
				}
			}
			uid = GetUniqueID();
		}
	}

	/// <summary>
	/// This usually happens after scripts get recompiled.
	/// When this happens, static variables are erased, so the list of objects has to be rebuilt.
	/// </summary>

	void OnEnable ()
	{
		if (!Application.isPlaying && id != 0)
		{
			Unregister();
			Register();
		}
	}
#endif

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>

	static TNObject FindParent (Transform t)
	{
		while (t != null)
		{
			TNObject tno = t.gameObject.GetComponent<TNObject>();
			if (tno != null) return tno;
			t = t.parent;
		}
		return null;
	}

	/// <summary>
	/// Register the object with the lists.
	/// </summary>

	void Start ()
	{
		if (id == 0)
		{
			mParent = FindParent(transform.parent);
			if (!TNManager.isConnected) return;

			if (mParent == null && Application.isPlaying)
			{
				Debug.LogError("Objects that are not instantiated via TNManager.Create must have a non-zero ID.", this);
				return;
			}
		}
		else
		{
			Register();

			// Have there been any delayed function calls for this object? If so, execute them now.
			for (int i = 0; i < mDelayed.size; )
			{
				DelayedCall dc = mDelayed[i];

				if (dc.objID == uid)
				{
					if (!string.IsNullOrEmpty(dc.funcName)) Execute(dc.funcName, dc.parameters);
					else Execute(dc.funcID, dc.parameters);
					mDelayed.RemoveAt(i);
					continue;
				}
				++i;
			}
		}
	}

	/// <summary>
	/// Remove this object from the list.
	/// </summary>

	void OnDestroy () { Unregister(); }

	/// <summary>
	/// Register the network object with the lists.
	/// </summary>

	public void Register ()
	{
		if (!mIsRegistered && uid != 0 && mParent == null)
		{
#if UNITY_EDITOR
			UniqueCheck();
#endif
			mDictionary[uid] = this;
			mList.Add(this);
			mIsRegistered = true;
		}
	}

	/// <summary>
	/// Unregister the network object.
	/// </summary>

	void Unregister ()
	{
		if (mIsRegistered)
		{
			if (mDictionary != null) mDictionary.Remove(uid);
			if (mList != null) mList.Remove(this);
			mIsRegistered = false;
		}
	}

	/// <summary>
	/// Invoke the function specified by the ID.
	/// </summary>

	public bool Execute (byte funcID, params object[] parameters)
	{
		if (mParent != null) return mParent.Execute(funcID, parameters);
		if (rebuildMethodList) RebuildMethodList();
		return UnityTools.ExecuteAll(mRFCs, funcID, parameters);
	}

	/// <summary>
	/// Invoke the function specified by the function name.
	/// </summary>

	public bool Execute (string funcName, params object[] parameters)
	{
		if (mParent != null) return mParent.Execute(funcName, parameters);
		if (rebuildMethodList) RebuildMethodList();
		return UnityTools.ExecuteAll(mRFCs, funcName, parameters);
	}

	/// <summary>
	/// Invoke the specified function. It's unlikely that you will need to call this function yourself.
	/// </summary>

	static public void FindAndExecute (uint objID, byte funcID, params object[] parameters)
	{
		TNObject obj = TNObject.Find(objID);

		if (obj != null)
		{
			if (!obj.Execute(funcID, parameters))
			{
#if UNITY_EDITOR
				Debug.LogError("[TNet] Unable to execute function with ID of '" + funcID + "'. Make sure there is a script that can receive this call.\n" +
					"GameObject: " + GetHierarchy(obj.gameObject), obj.gameObject);
#endif
			}
		}
		else if (TNManager.isInChannel)
		{
			DelayedCall dc = new DelayedCall();
			dc.objID = objID;
			dc.funcID = funcID;
			dc.parameters = parameters;
			mDelayed.Add(dc);
		}
#if UNITY_EDITOR
		else Debug.LogError("[TNet] Trying to execute a function " + funcID + " on TNObject #" + objID +
			" before it has been created.");
#endif
	}

	/// <summary>
	/// Invoke the specified function. It's unlikely that you will need to call this function yourself.
	/// </summary>

	static public void FindAndExecute (uint objID, string funcName, params object[] parameters)
	{
		TNObject obj = TNObject.Find(objID);

		if (obj != null)
		{
			if (!obj.Execute(funcName, parameters))
			{
#if UNITY_EDITOR
				Debug.LogError("[TNet] Unable to execute function '" + funcName + "'. Did you forget an [RFC] prefix, perhaps?\n" +
					"GameObject: " + GetHierarchy(obj.gameObject), obj.gameObject);
#endif
			}
		}
		else if (TNManager.isInChannel)
		{
			DelayedCall dc = new DelayedCall();
			dc.objID = objID;
			dc.funcName = funcName;
			dc.parameters = parameters;
			mDelayed.Add(dc);
		}
#if UNITY_EDITOR
		else Debug.LogError("[TNet] Trying to execute a function '" + funcName + "' on TNObject #" + objID +
			" before it has been created.");
#endif
	}

	/// <summary>
	/// Rebuild the list of known RFC calls.
	/// </summary>

	void RebuildMethodList ()
	{
		rebuildMethodList = false;
		mRFCs.Clear();
		MonoBehaviour[] mbs = GetComponentsInChildren<MonoBehaviour>(true);

		for (int i = 0, imax = mbs.Length; i < imax; ++i)
		{
			if (methods[b].IsDefined(typeof(RFC), true))
				{
					CachedFunc ent = new CachedFunc();
					ent.obj = mb;
					ent.func = methods[b];

					RFC tnc = (RFC)ent.func.GetCustomAttributes(typeof(RFC), true)[0];
					ent.id = tnc.id;
					mRFCs.Add(ent);
				}
			}
		}
	}

	/// <summary>
	/// Send a remote function call.
	/// </summary>

	public void Send (byte rfcID, Target target, params object[] objs) { SendRFC(rfcID, null, target, true, objs); }

	/// <summary>
	/// Send a remote function call.
	/// Note that you should not use this version of the function if you care about performance (as it's much slower than others),
	/// or if players can have duplicate names, as only one of them will actually receive this message.
	/// </summary>

	public void Send (byte rfcID, string targetName, params object[] objs) { SendRFC(rfcID, null, targetName, true, objs); }

	/// <summary>
	/// Send a remote function call.
	/// </summary>

	public void Send (string rfcName, Target target, params object[] objs) { SendRFC(0, rfcName, target, true, objs); }

	/// <summary>
	/// Send a remote function call.
	/// Note that you should not use this version of the function if you care about performance (as it's much slower than others),
	/// or if players can have duplicate names, as only one of them will actually receive this message.
	/// </summary>

	public void Send (string rfcName, string targetName, params object[] objs) { SendRFC(0, rfcName, targetName, true, objs); }

	/// <summary>
	/// Send a remote function call.
	/// </summary>

	public void Send (byte rfcID, Player target, params object[] objs)
	{
		if (target != null) SendRFC(rfcID, null, target, true, objs);
		else SendRFC(rfcID, null, Target.All, true, objs);
	}

	/// <summary>
	/// Send a remote function call.
	/// </summary>

	public void Send (string rfcName, Player target, params object[] objs)
	{
		if (target != null) SendRFC(0, rfcName, target, true, objs);
		else SendRFC(0, rfcName, Target.All, true, objs);
	}

	/// <summary>
	/// Send a remote function call via UDP (if possible).
	/// </summary>

	public void SendQuickly (byte rfcID, Target target, params object[] objs) { SendRFC(rfcID, null, target, false, objs); }

	/// <summary>
	/// Send a remote function call via UDP (if possible).
	/// </summary>

	public void SendQuickly (string rfcName, Target target, params object[] objs) { SendRFC(0, rfcName, target, false, objs); }

	/// <summary>
	/// Send a remote function call via UDP (if possible).
	/// </summary>

	public void SendQuickly (byte rfcID, Player target, params object[] objs)
	{
		if (target != null) SendRFC(rfcID, null, target, false, objs);
		else SendRFC(rfcID, null, Target.All, false, objs);
	}

	/// <summary>
	/// Send a remote function call via UDP (if possible).
	/// </summary>

	public void SendQuickly (string rfcName, Player target, params object[] objs) { SendRFC(0, rfcName, target, false, objs); }

	/// <summary>
	/// Send a broadcast to the entire LAN. Does not require an active connection.
	/// </summary>

	public void BroadcastToLAN (int port, byte rfcID, params object[] objs) { BroadcastToLAN(port, rfcID, null, objs); }

	/// <summary>
	/// Send a broadcast to the entire LAN. Does not require an active connection.
	/// </summary>

	public void BroadcastToLAN (int port, string rfcName, params object[] objs) { BroadcastToLAN(port, 0, rfcName, objs); }

	/// <summary>
	/// Remove a previously saved remote function call.
	/// </summary>

	public void Remove (string rfcName) { RemoveSavedRFC(uid, 0, rfcName); }

	/// <summary>
	/// Remove a previously saved remote function call.
	/// </summary>

	public void Remove (byte rfcID) { RemoveSavedRFC(uid, rfcID, null); }

	/// <summary>
	/// Convert object and RFC IDs into a single UINT.
	/// </summary>

	static uint GetUID (uint objID, byte rfcID)
	{
		return (objID << 8) | rfcID;
	}

	/// <summary>
	/// Decode object ID and RFC IDs encoded in a single UINT.
	/// </summary>

	static public void DecodeUID (uint uid, out uint objID, out byte rfcID)
	{
		rfcID = (byte)(uid & 0xFF);
		objID = (uid >> 8);
	}

	/// <summary>
	/// Send a new RFC call to the specified target.
	/// </summary>

	void SendRFC (byte rfcID, string rfcName, Target target, bool reliable, params object[] objs)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		bool executeLocally = false;

		if (target == Target.Broadcast)
		{
			if (TNManager.isConnected)
			{
				BinaryWriter writer = TNManager.BeginSend(Packet.Broadcast);
				writer.Write(GetUID(uid, rfcID));
				if (rfcID == 0) writer.Write(rfcName);
				writer.WriteArray(objs);
				TNManager.EndSend(reliable);
			}
			else executeLocally = true;
		}
		else if (target == Target.Host && TNManager.isHosting)
		{
			// We're the host, and the packet should be going to the host -- just echo it locally
			executeLocally = true;
		}
		else if (TNManager.isInChannel)
		{
			// We want to echo UDP-based packets locally instead of having them bounce through the server
			if (!reliable)
			{
				if (target == Target.All)
				{
					target = Target.Others;
					executeLocally = true;
				}
				else if (target == Target.AllSaved)
				{
					target = Target.OthersSaved;
					executeLocally = true;
				}
			}

			byte packetID = (byte)((int)Packet.ForwardToAll + (int)target);
			BinaryWriter writer = TNManager.BeginSend(packetID);
			writer.Write(GetUID(uid, rfcID));
			if (rfcID == 0) writer.Write(rfcName);
			writer.WriteArray(objs);
			TNManager.EndSend(reliable);
		}
		else if (!TNManager.isConnected && (target == Target.All || target == Target.AllSaved))
		{
			// Offline packet
			executeLocally = true;
		}

		if (executeLocally)
		{
			if (rfcID != 0) Execute(rfcID, objs);
			else Execute(rfcName, objs);
		}
	}

	/// <summary>
	/// Send a new RFC call to the specified target.
	/// </summary>

	void SendRFC (byte rfcID, string rfcName, string targetName, bool reliable, params object[] objs)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (string.IsNullOrEmpty(targetName)) return;

		if (targetName == TNManager.playerName)
		{
			if (rfcID != 0) Execute(rfcID, objs);
			else Execute(rfcName, objs);
		}
		else
		{
			BinaryWriter writer = TNManager.BeginSend(Packet.ForwardByName);
			writer.Write(targetName);
			writer.Write(GetUID(uid, rfcID));
			if (rfcID == 0) writer.Write(rfcName);
			writer.WriteArray(objs);
			TNManager.EndSend(reliable);
		}
	}

	/// <summary>
	/// Send a new remote function call to the specified player.
	/// </summary>

	void SendRFC (byte rfcID, string rfcName, Player target, bool reliable, params object[] objs)
	{
		if (TNManager.isConnected)
		{
			BinaryWriter writer = TNManager.BeginSend(Packet.ForwardToPlayer);
			writer.Write(target.id);
			writer.Write(GetUID(uid, rfcID));
			if (rfcID == 0) writer.Write(rfcName);
			writer.WriteArray(objs);
			TNManager.EndSend(reliable);
		}
		else if (target == TNManager.player)
		{
			if (rfcID != 0) Execute(rfcID, objs);
			else Execute(rfcName, objs);
		}
	}

	/// <summary>
	/// Broadcast a remote function call to all players on the network.
	/// </summary>

	void BroadcastToLAN (int port, byte rfcID, string rfcName, params object[] objs)
	{
		BinaryWriter writer = TNManager.BeginSend(Packet.ForwardToAll);
		writer.Write(GetUID(uid, rfcID));
		if (rfcID == 0) writer.Write(rfcName);
		writer.WriteArray(objs);
		TNManager.EndSend(port);
	}

	/// <summary>
	/// Remove a previously saved remote function call.
	/// </summary>

	static void RemoveSavedRFC (uint objID, byte rfcID, string funcName)
	{
		if (TNManager.isInChannel)
		{
			BinaryWriter writer = TNManager.BeginSend(Packet.RequestRemoveRFC);
			writer.Write(GetUID(objID, rfcID));
			if (rfcID == 0) writer.Write(funcName);
			TNManager.EndSend();
		}
	}
}
