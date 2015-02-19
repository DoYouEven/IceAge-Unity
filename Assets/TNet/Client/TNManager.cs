//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System.IO;
using UnityEngine;
using TNet;
using System.Net;
using System.Reflection;
using UnityTools = TNet.UnityTools;

/// <summary>
/// Tasharen Network Manager tailored for Unity.
/// </summary>

[AddComponentMenu("TNet/Network Manager")]
public class TNManager : MonoBehaviour
{
	/// <summary>
	/// Notification that will be called when SyncPlayerData() gets called, even in offline mode (for consistency).
	/// </summary>

	static public GameClient.OnPlayerSync onPlayerSync;

	/// <summary>
	/// If set to 'true', the list of custom creation functions will be rebuilt the next time it's accessed.
	/// </summary>

	static public bool rebuildMethodList = true;

	// Cached list of creation functions
	static List<CachedFunc> mRCCs = new List<CachedFunc>();

	// Static player, here just for convenience so that GetPlayer() works the same even if instance is missing.
	static Player mPlayer = new Player("Guest");

	// Player list that will contain only the player in it. Here for the same reason as 'mPlayer'.
	static List<Player> mPlayers;

	// Instance pointer
	static TNManager mInstance;
	static int mObjectOwner = -1;

	/// <summary>
	/// List of objects that can be instantiated by the network.
	/// </summary>

	public GameObject[] objects;

	// Network client
	GameClient mClient = new GameClient();
	bool mJoiningChannel = false;

	/// <summary>
	/// TNet Client used for communication.
	/// </summary>

	static public GameClient client { get { return (mInstance != null) ? mInstance.mClient : null; } }

	/// <summary>
	/// Whether we're currently connected.
	/// </summary>

	static public bool isConnected { get { return mInstance != null && mInstance.mClient.isConnected; } }

	/// <summary>
	/// Whether TNet is currently joining a channel. This gets set to 'true' in JoinChannel,
	/// then 'false' just before OnNetworkJoinChannel was gets out.
	/// </summary>

	static public bool isJoiningChannel { get { return mInstance != null && mInstance.mJoiningChannel; } }

	/// <summary>
	/// Whether we are currently trying to establish a new connection.
	/// </summary>

	static public bool isTryingToConnect { get { return mInstance != null && mInstance.mClient.isTryingToConnect; } }

	/// <summary>
	/// Whether we're currently hosting.
	/// </summary>

	static public bool isHosting { get { return mInstance == null || mInstance.mClient.isHosting; } }

	/// <summary>
	/// Whether the player is currently in a channel.
	/// </summary>

	static public bool isInChannel { get { return mInstance != null && mInstance.mClient.isConnected && mInstance.mClient.isInChannel; } }

	/// <summary>
	/// You can pause TNManager's message processing if you like.
	/// </summary>

	static public bool isActive
	{
		get
		{
			return mInstance != null && mInstance.mClient.isActive;
		}
		set
		{
			if (mInstance != null) mInstance.mClient.isActive = value;
		}
	}

	/// <summary>
	/// Enable or disable the Nagle's buffering algorithm (aka NO_DELAY flag).
	/// Enabling this flag will improve latency at the cost of increased bandwidth.
	/// http://en.wikipedia.org/wiki/Nagle's_algorithm
	/// </summary>

	static public bool noDelay { get { return mInstance != null && mInstance.mClient.noDelay; } set { if (mInstance != null) mInstance.mClient.noDelay = value; } }

	/// <summary>
	/// Current ping to the server.
	/// </summary>

	static public int ping { get { return mInstance != null ? mInstance.mClient.ping : 0; } }

	/// <summary>
	/// Whether we can use unreliable packets (UDP) to communicate with the server.
	/// </summary>

	static public bool canUseUDP { get { return mInstance != null && mInstance.mClient.canUseUDP; } }

	/// <summary>
	/// Listening port for incoming UDP packets. Set via TNManager.StartUDP().
	/// </summary>

	static public int listeningPort { get { return mInstance != null ? mInstance.mClient.listeningPort : 0; } }

	/// <summary>
	/// ID of the player that wanted an object to be created.
	/// Check this in a script's Awake() attached to an network-instantiated object.
	/// </summary>

	static public int objectOwnerID { get { return mObjectOwner == -1 ? hostID : mObjectOwner; } }

	/// <summary>
	/// Call this function in the script's Awake() to determine if this object
	/// was created as a result of the player's TNManager.Create() call.
	/// In most cases you should check tno.isMine instead.
	/// </summary>

	static public bool isThisMyObject { get { return objectOwnerID == playerID; } }

	/// <summary>
	/// Current time on the server.
	/// </summary>

	static public long serverTime { get { return (mInstance != null) ? mInstance.mClient.serverTime : (System.DateTime.UtcNow.Ticks / 10000); } }

	/// <summary>
	/// Address from which the packet was received. Only available during packet processing callbacks.
	/// If null, then the packet arrived via the active connection (TCP).
	/// If the return value is not null, then the last packet arrived via UDP.
	/// </summary>

	static public IPEndPoint packetSource { get { return (mInstance != null) ? mInstance.mClient.packetSource : null; } }

	/// <summary>
	/// Custom data associated with the channel.
	/// </summary>

	static public string channelData
	{
		get
		{
			return (mInstance != null) ? mInstance.mClient.channelData : "";
		}
		set
		{
			if (mInstance != null)
			{
				mInstance.mClient.channelData = value;
			}
		}
	}

	/// <summary>
	/// ID of the channel the player is in.
	/// </summary>

	static public int channelID { get { return isConnected ? mInstance.mClient.channelID : 0; } }

	/// <summary>
	/// ID of the host.
	/// </summary>

	static public int hostID { get { return isConnected ? mInstance.mClient.hostID : mPlayer.id; } }

	/// <summary>
	/// The player's unique identifier.
	/// </summary>

	static public int playerID { get { return isConnected ? mInstance.mClient.playerID : mPlayer.id; } }

	/// <summary>
	/// Get or set the player's name as everyone sees him on the network.
	/// </summary>

	static public string playerName
	{
		get
		{
			return isConnected ? mInstance.mClient.playerName : mPlayer.name;
		}
		set
		{
			if (playerName != value)
			{
				mPlayer.name = value;
				if (isConnected) mInstance.mClient.playerName = value;
			}
		}
	}

	/// <summary>
	/// Get or set the player's data, synchronizing it with the server.
	/// </summary>

	static public object playerData
	{
		get
		{
			return isConnected ? mInstance.mClient.playerData : mPlayer.data;
		}
		set
		{
			mPlayer.data = value;
			if (isConnected) mInstance.mClient.playerData = value;
		}
	}

	/// <summary>
	/// Gets the player's data in DataNode format. It's a convenience property.
	/// After changing the values, call TNManager.SyncPlayerData().
	/// </summary>

	static public DataNode playerDataNode
	{
		get
		{
			DataNode node = playerData as DataNode;
			
			if (node == null)
			{
				node = new DataNode("Version", TNet.Player.version);
				playerData = node;
			}
			return node;
		}
	}

	/// <summary>
	/// List of players in the same channel as the client.
	/// </summary>

	static public List<Player> players
	{
		get
		{
			if (isConnected) return mInstance.mClient.players;
			if (mPlayers == null) mPlayers = new List<Player>();
			return mPlayers;
		}
	}

	/// <summary>
	/// Get the local player.
	/// </summary>

	static public Player player { get { return isConnected ? mInstance.mClient.player : mPlayer; } }

	/// <summary>
	/// Ensure that we have a TNManager to work with.
	/// </summary>

	static TNManager instance
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return mInstance;
#endif
			if (mInstance == null)
			{
				GameObject go = new GameObject("Network Manager");
				mInstance = go.AddComponent<TNManager>();
			}
			return mInstance;
		}
	}

	/// <summary>
	/// Get the player associated with the specified ID.
	/// </summary>

	static public Player GetPlayer (int id)
	{
		if (isConnected) return mInstance.mClient.GetPlayer(id);
		if (id == mPlayer.id) return mPlayer;
		return null;
	}

	/// <summary>
	/// Set the following function to handle this type of packets.
	/// </summary>

	static public void SetPacketHandler (byte packetID, GameClient.OnPacket callback)
	{
		instance.mClient.packetHandlers[packetID] = callback;
	}

	/// <summary>
	/// Set the following function to handle this type of packets.
	/// </summary>

	static public void SetPacketHandler (Packet packet, GameClient.OnPacket callback)
	{
		instance.mClient.packetHandlers[(byte)packet] = callback;
	}

	/// <summary>
	/// Start listening for incoming UDP packets on the specified port.
	/// </summary>

	static public bool StartUDP (int udpPort) { return instance.mClient.StartUDP(udpPort); }

	/// <summary>
	/// Stop listening to incoming UDP packets.
	/// </summary>

	static public void StopUDP () { if (mInstance != null) mInstance.mClient.StopUDP(); }

	/// <summary>
	/// Send a remote ping request to the specified TNet server.
	/// </summary>

	static public void Ping (IPEndPoint udpEndPoint, GameClient.OnPing callback) { instance.mClient.Ping(udpEndPoint, callback); }

	/// <summary>
	/// Connect to the specified remote destination.
	/// </summary>

	static public void Connect (IPEndPoint externalIP, IPEndPoint internalIP)
	{
		instance.mClient.Disconnect();
		instance.mClient.playerName = mPlayer.name;
		instance.mClient.playerData = mPlayer.data;
		instance.mClient.Connect(externalIP, internalIP);
	}

	/// <summary>
	/// Connect to the specified destination.
	/// </summary>

	static public void Connect (string address, int port)
	{
		IPEndPoint ip = TNet.Tools.ResolveEndPoint(address, port);

		if (ip == null)
		{
			instance.OnConnect(false, "Unable to resolve [" + address + "]");
		}
		else
		{
			instance.mClient.playerName = mPlayer.name;
			instance.mClient.playerData = mPlayer.data;
			instance.mClient.Connect(ip, null);
		}
	}

	/// <summary>
	/// Connect to the specified destination with the address and port specified as "255.255.255.255:255".
	/// </summary>

	static public void Connect (string address)
	{
		string[] split = address.Split(new char[] { ':' });
		int port = 5127;
		if (split.Length > 1) int.TryParse(split[1], out port);
		Connect(split[0], port);
	}

	/// <summary>
	/// Disconnect from the specified destination.
	/// </summary>

	static public void Disconnect () { if (mInstance != null) mInstance.mClient.Disconnect(); }

	/// <summary>
	/// Join the specified channel. This channel will be marked as persistent, meaning it will
	/// stay open even when the last player leaves, unless explicitly closed first.
	/// </summary>
	/// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
	/// <param name="levelName">Level that will be loaded first.</param>

	static public void JoinChannel (int channelID, string levelName)
	{
		if (mInstance != null)
		{
			mInstance.mJoiningChannel = true;
			mInstance.mClient.JoinChannel(channelID, levelName, false, 65535, null);
		}
		else Application.LoadLevel(levelName);
	}

	/// <summary>
	/// Join the specified channel.
	/// </summary>
	/// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
	/// <param name="levelName">Level that will be loaded first.</param>
	/// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
	/// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
	/// <param name="password">Password for the channel. First player sets the password.</param>

	static public void JoinChannel (int channelID, string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mJoiningChannel = true;
			mInstance.mClient.JoinChannel(channelID, levelName, persistent, playerLimit, password);
		}
		else Application.LoadLevel(levelName);
	}

	/// <summary>
	/// Join a random open game channel or create a new one. Guaranteed to load the specified level.
	/// </summary>
	/// <param name="levelName">Level that will be loaded first.</param>
	/// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
	/// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
	/// <param name="password">Password for the channel. First player sets the password.</param>

	static public void JoinRandomChannel (string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mJoiningChannel = true;
			mInstance.mClient.JoinChannel(-2, levelName, persistent, playerLimit, password);
		}
	}

	/// <summary>
	/// Create a new channel.
	/// </summary>
	/// <param name="levelName">Level that will be loaded first.</param>
	/// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
	/// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
	/// <param name="password">Password for the channel. First player sets the password.</param>

	static public void CreateChannel (string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mJoiningChannel = true;
			mInstance.mClient.JoinChannel(-1, levelName, persistent, playerLimit, password);
		}
		else Application.LoadLevel(levelName);
	}

	/// <summary>
	/// Close the channel the player is in. New players will be prevented from joining.
	/// Once a channel has been closed, it cannot be re-opened.
	/// </summary>

	static public void CloseChannel () { if (mInstance != null) mInstance.mClient.CloseChannel(); }

	/// <summary>
	/// Leave the channel we're in.
	/// </summary>

	static public void LeaveChannel () { if (mInstance != null) mInstance.mClient.LeaveChannel(); }

	/// <summary>
	/// Change the maximum number of players that can join the channel the player is currently in.
	/// </summary>

	static public void SetPlayerLimit (int max) { if (mInstance != null) mInstance.mClient.SetPlayerLimit(max); }

	/// <summary>
	/// Load the specified level.
	/// </summary>

	static public void LoadLevel (string levelName)
	{
		if (isConnected) mInstance.mClient.LoadLevel(levelName);
		else Application.LoadLevel(levelName);
	}

	/// <summary>
	/// Save the specified file on the server.
	/// </summary>

	static public void SaveFile (string filename, byte[] data)
	{
		if (isConnected)
		{
			mInstance.mClient.SaveFile(filename, data);
		}
		else
		{
			try
			{
				Tools.WriteFile(filename, data);
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message + " (" + filename + ")");
			}
		}
	}

	/// <summary>
	/// Load the specified file residing on the server.
	/// </summary>

	static public void LoadFile (string filename, GameClient.OnLoadFile callback)
	{
		if (callback != null)
		{
			if (isConnected)
			{
				mInstance.mClient.LoadFile(filename, callback);
			}
			else callback(filename, Tools.ReadFile(filename));
		}
	}

	/// <summary>
	/// Change the hosting player.
	/// </summary>

	static public void SetHost (Player player)
	{
		if (mInstance != null) mInstance.mClient.SetHost(player);
	}

	/// <summary>
	/// Set the timeout for the player. By default it's 10 seconds. If you know you are about to load a large level,
	/// and it's going to take, say 60 seconds, set this timeout to 120 seconds just to be safe. When the level
	/// finishes loading, change this back to 10 seconds so that dropped connections gets detected correctly.
	/// </summary>

	static public void SetTimeout (int seconds)
	{
		if (mInstance != null) mInstance.mClient.SetTimeout(seconds);
	}

	/// <summary>
	/// Sync the player's data with the server.
	/// </summary>

	static public void SyncPlayerData ()
	{
		if (isConnected) mInstance.mClient.SyncPlayerData();
		if (onPlayerSync != null) onPlayerSync(player);
	}

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be present in the TNManager's list of objects.
	/// </summary>

	static public void Create (GameObject go, bool persistent = true) { CreateEx(0, persistent, go); }

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be located in the Resources folder.
	/// </summary>

	static public void Create (string path, bool persistent = true) { CreateEx(0, persistent, path); }

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be present in the TNManager's list of objects.
	/// </summary>

	static public void Create (GameObject go, Vector3 pos, Quaternion rot, bool persistent = true) { CreateEx(1, persistent, go, pos, rot); }

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be located in the Resources folder.
	/// </summary>

	static public void Create (string path, Vector3 pos, Quaternion rot, bool persistent = true) { CreateEx(1, persistent, path, pos, rot); }

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be present in the TNManager's list of objects.
	/// </summary>

	static public void Create (GameObject go, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, bool persistent = true)
	{
		CreateEx(2, persistent, go, pos, rot, vel, angVel);
	}

	/// <summary>
	/// Create the specified game object on all connected clients. The object must be located in the Resources folder.
	/// </summary>

	static public void Create (string path, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, bool persistent = true)
	{
		CreateEx(2, persistent, path, pos, rot, vel, angVel);
	}

	/// <summary>
	/// Create a packet that will send a custom object creation call.
	/// It is expected that the first byte that follows will identify which function will be parsing this packet later.
	/// </summary>

	static public void CreateEx (int rccID, bool persistent, GameObject go, params object[] objs)
	{
		if (go != null)
		{
			int index = IndexOf(go);

			if (isConnected)
			{
				if (index != -1)
				{
					BinaryWriter writer = mInstance.mClient.BeginSend(Packet.RequestCreate);
					writer.Write((ushort)index);
					writer.Write(GetFlag(go, persistent));
					writer.Write((byte)rccID);
					writer.WriteArray(objs);
					EndSend();
					return;
				}
				else
				{
					Debug.LogError("\"" + go.name + "\" has not been added to TNManager's list of objects, so it cannot be instantiated.\n" +
						"Consider placing it into the Resources folder and passing its name instead.", go);
				}
			}

			objs = BinaryExtensions.CombineArrays(go, objs);
			UnityTools.ExecuteAll(GetRCCs(), (byte)rccID, objs);
			UnityTools.Clear(objs);
		}
	}

	/// <summary>
	/// Create a packet that will send a custom object creation call.
	/// It is expected that the first byte that follows will identify which function will be parsing this packet later.
	/// </summary>

	static public void CreateEx (int rccID, bool persistent, string path, params object[] objs)
	{
		GameObject go = LoadGameObject(path);

		if (go != null)
		{
			if (isConnected)
			{
				BinaryWriter writer = mInstance.mClient.BeginSend(Packet.RequestCreate);
				byte flag = GetFlag(go, persistent);
				writer.Write((ushort)65535);
				writer.Write(flag);
				writer.Write(path);
				writer.Write((byte)rccID);
				writer.WriteArray(objs);
				EndSend();
				return;
			}

			objs = BinaryExtensions.CombineArrays(go, objs);
			UnityTools.ExecuteAll(GetRCCs(), (byte)rccID, objs);
			UnityTools.Clear(objs);
		}
		else Debug.LogError("Unable to load " + path);
	}

	/// <summary>
	/// Get the list of creation functions, registering default ones as necessary.
	/// </summary>

	static public List<CachedFunc> GetRCCs ()
	{
		if (rebuildMethodList)
		{
			rebuildMethodList = false;

			if (mInstance != null)
			{
				MonoBehaviour[] mbs = mInstance.GetComponentsInChildren<MonoBehaviour>();

				for (int i = 0, imax = mbs.Length; i < imax; ++i)
				{
					MonoBehaviour mb = mbs[i];
					AddRCCs(mb, mb.GetType());
				}
			}
			else
			{
				// Add the built-in remote creation calls
				AddRCCs<TNManager>();
			}
		}
		return mRCCs;
	}

	/// <summary>
	/// Add a new Remote Creation Call.
	/// </summary>

	static public void AddRCCs (MonoBehaviour mb) { AddRCCs(mb, mb.GetType()); }

	/// <summary>
	/// Add a new Remote Creation Call.
	/// </summary>

	static public void AddRCCs<T> () { AddRCCs(null, typeof(T)); }

	/// <summary>
	/// Add a new Remote Creation Call.
	/// </summary>

	static void AddRCCs (object obj, System.Type type)
	{
		MethodInfo[] methods = type.GetMethods(
					BindingFlags.Public |
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.Static);

		for (int b = 0; b < methods.Length; ++b)
		{
			if (methods[b].IsDefined(typeof(RCC), true))
			{
				RCC tnc = (RCC)methods[b].GetCustomAttributes(typeof(RCC), true)[0];

				for (int i = 0; i < mRCCs.size; ++i)
				{
					CachedFunc f = mRCCs[i];

					if (f.id == tnc.id)
					{
						f.obj = obj;
						f.func = methods[b];
						return;
					}
				}

				CachedFunc ent = new CachedFunc();
				ent.obj = obj;
				ent.func = methods[b];
				ent.id = tnc.id;
				mRCCs.Add(ent);
			}
		}
	}

	/// <summary>
	/// Remove a previously registered Remote Creation Call.
	/// </summary>

	static void RemoveRCC (int rccID)
	{
		for (int i = 0; i < mRCCs.size; ++i)
		{
			CachedFunc f = mRCCs[i];

			if (f.id == rccID)
			{
				mRCCs.RemoveAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// Remove previously registered Remote Creation Calls.
	/// </summary>

	static void RemoveRCCs<T> ()
	{
		MethodInfo[] methods = typeof(T).GetMethods(
					BindingFlags.Public |
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.Static);

		for (int b = 0; b < methods.Length; ++b)
		{
			if (methods[b].IsDefined(typeof(RCC), true))
			{
				RCC tnc = (RCC)methods[b].GetCustomAttributes(typeof(RCC), true)[0];
				RemoveRCC(tnc.id);
			}
		}
	}

	/// <summary>
	/// Built-in Remote Creation Calls.
	/// </summary>

	[RCC(0)] static GameObject OnCreate0 (GameObject go) { return Instantiate(go) as GameObject; }
	[RCC(1)] static GameObject OnCreate1 (GameObject go, Vector3 pos, Quaternion rot) { return Instantiate(go, pos, rot) as GameObject; }
	[RCC(2)] static GameObject OnCreate2 (GameObject go, Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity)
	{
		return UnityTools.Instantiate(go, pos, rot, velocity, angularVelocity);
	}

	/// <summary>
	/// Destroy the specified game object.
	/// </summary>

	static public void Destroy (GameObject go)
	{
		if (isConnected)
		{
			TNObject obj = go.GetComponent<TNObject>();

			if (obj != null)
			{
				BinaryWriter writer = mInstance.mClient.BeginSend(Packet.RequestDestroy);
				writer.Write(obj.uid);
				mInstance.mClient.EndSend();
				return;
			}
		}
		GameObject.Destroy(go);
	}

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	static public BinaryWriter BeginSend (Packet type) { return mInstance.mClient.BeginSend(type); }

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	static public BinaryWriter BeginSend (byte packetID) { return mInstance.mClient.BeginSend(packetID); }

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	static public void EndSend () { mInstance.mClient.EndSend(true); }

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	static public void EndSend (bool reliable) { mInstance.mClient.EndSend(reliable); }

	/// <summary>
	/// Broadcast the packet to everyone on the LAN.
	/// </summary>

	static public void EndSend (int port) { mInstance.mClient.EndSend(port); }

	/// <summary>
	/// Broadcast the packet to the specified endpoint via UDP.
	/// </summary>

	static public void EndSend (IPEndPoint target) { mInstance.mClient.EndSend(target); }

#region MonoBehaviour and helper functions -- it's unlikely that you will need to modify these

	/// <summary>
	/// Ensure that there is only one instance of this class present.
	/// </summary>

	void Awake ()
	{
		if (mInstance != null)
		{
			GameObject.Destroy(gameObject);
		}
		else
		{
			mInstance = this;
			rebuildMethodList = true;
			DontDestroyOnLoad(gameObject);

			mClient.onError				= OnError;
			mClient.onConnect			= OnConnect;
			mClient.onDisconnect		= OnDisconnect;
			mClient.onJoinChannel		= OnJoinChannel;
			mClient.onLeftChannel		= OnLeftChannel;
			mClient.onLoadLevel			= OnLoadLevel;
			mClient.onPlayerJoined		= OnPlayerJoined;
			mClient.onPlayerLeft		= OnPlayerLeft;
			mClient.onPlayerSync		= OnPlayerSync;
			mClient.onRenamePlayer		= OnRenamePlayer;
			mClient.onCreate			= OnCreateObject;
			mClient.onDestroy			= OnDestroyObject;
			mClient.onForwardedPacket	= OnForwardedPacket;

#if UNITY_EDITOR
			List<IPAddress> ips = TNet.Tools.localAddresses;

			if (ips != null && ips.size > 0)
			{
				string text = "[TNet] Local IPs: " + ips.size;

				for (int i = 0; i < ips.size; ++i)
				{
					text += "\n  " + (i + 1) + ": " + ips[i];
					if (ips[i] == TNet.Tools.localAddress) text += " (Primary)";
				}
				Debug.Log(text);
			}
			else Debug.LogError("[TNet] IP address cannot be determined!", this);
#endif
		}
	}

	/// <summary>
	/// Make sure we disconnect on exit.
	/// </summary>

	void OnDestroy ()
	{
		if (mInstance == this)
		{
			if (isConnected) mClient.Disconnect();
			mClient.StopUDP();
			mInstance = null;
		}
	}

	/// <summary>
	/// Find the index of the specified game object.
	/// </summary>

	static public int IndexOf (GameObject go)
	{
		if (go != null && mInstance != null && mInstance.objects != null)
		{
			for (int i = 0, imax = mInstance.objects.Length; i < imax; ++i)
				if (mInstance.objects[i] == go) return i;

			Debug.LogError("[TNet] The game object was not found in the TNManager's list of objects. Did you forget to add it?", go);
		}
		return -1;
	}

	/// <summary>
	/// Load a game object at the specified path in the Resources folder.
	/// </summary>

	static GameObject LoadGameObject (string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			Debug.LogError("[TNet] Null path passed to TNManager.LoadGameObject!");
			return null;
		}

		GameObject go = Resources.Load(path, typeof(GameObject)) as GameObject;

		if (go == null)
		{
			Debug.LogError("[TNet] Attempting to create a game object that can't be found in the Resources folder: " + path);
		}
		return go;
	}

	/// <summary>
	/// RequestCreate flag.
	/// 0 = Local-only object. Only echoed to other clients.
	/// 1 = Saved on the server, assigned a new owner when the existing owner leaves.
	/// 2 = Saved on the server, destroyed when the owner leaves.
	/// </summary>

	static byte GetFlag (GameObject go, bool persistent)
	{
		TNObject tno = go.GetComponent<TNObject>();
		if (tno == null) return 0;
		return persistent ? (byte)1 : (byte)2;
	}

	/// <summary>
	/// Notification of a new object being created.
	/// </summary>

	void OnCreateObject (int creator, int index, uint objectID, BinaryReader reader)
	{
		mObjectOwner = creator;
		GameObject go = null;

		if (index == 65535)
		{
			// Load the object from the resources folder
			go = LoadGameObject(reader.ReadString());
		}
		else if (index >= 0 && index < objects.Length)
		{
			// Reference the object from the provided list
			go = objects[index];
		}
		else
		{
			Debug.LogError("Attempting to create an invalid object. Index: " + index);
			return;
		}

		// Create the object
		go = CreateGameObject(go, reader);

		// If an object ID was requested, assign it to the TNObject
		if (go != null && objectID != 0)
		{
			TNObject obj = go.GetComponent<TNObject>();

			if (obj != null)
			{
				obj.uid = objectID;
				obj.Register();
			}
			else
			{
				Debug.LogWarning("[TNet] The instantiated object has no TNObject component. Don't request an ObjectID when creating it.", go);
			}
		}
		mObjectOwner = -1;
	}

	/// <summary>
	/// Create a new game object.
	/// </summary>

	static GameObject CreateGameObject (GameObject prefab, BinaryReader reader)
	{
		if (prefab != null)
		{
			// The first byte is always the type that identifies what kind of data will follow
			byte type = reader.ReadByte();

			if (type == 0)
			{
				// Just a plain game object
				return Instantiate(prefab) as GameObject;
			}
			else
			{
				// Custom creation function
				object[] objs = reader.ReadArray(prefab);
				object retVal;

				if (!UnityTools.ExecuteFirst(GetRCCs(), type, out retVal, objs))
				{
					Debug.LogError("[TNet] Failed to call RCC #" + type + ".\nDid you forget to register it in Awake() via TNManager.AddRCCs?");
					UnityTools.Clear(objs);
					return null;
				}

				UnityTools.Clear(objs);

				if (retVal == null)
				{
					Debug.LogError("[TNet] Instantiating \"" + prefab.name + "\" via RCC #" + type + " returned null.\nDid you forget to return the game object from your RCC?");
				}
				return retVal as GameObject;
			}
		}
		return null;
	}

	/// <summary>
	/// Notification of a network object being destroyed.
	/// </summary>

	void OnDestroyObject (uint objID)
	{
		TNObject obj = TNObject.Find(objID);
		if (obj) GameObject.Destroy(obj.gameObject);
	}

	/// <summary>
	/// If custom functionality is needed, all unrecognized packets will arrive here.
	/// </summary>

	void OnForwardedPacket (BinaryReader reader)
	{
		uint objID;
		byte funcID;
		TNObject.DecodeUID(reader.ReadUInt32(), out objID, out funcID);

		if (funcID == 0)
		{
			TNObject.FindAndExecute(objID, reader.ReadString(), reader.ReadArray());
		}
		else
		{
			TNObject.FindAndExecute(objID, funcID, reader.ReadArray());
		}
	}

	/// <summary>
	/// Process incoming packets in the update function.
	/// </summary>

	void Update () { mClient.ProcessPackets(); }

#endregion
#region Callbacks -- Modify these if you don't like the broadcast approach

	/// <summary>
	/// Error notification.
	/// </summary>

	void OnError (string err) { UnityTools.Broadcast("OnNetworkError", err); }

	/// <summary>
	/// Connection result notification.
	/// </summary>

	void OnConnect (bool success, string message) { UnityTools.Broadcast("OnNetworkConnect", success, message); }

	/// <summary>
	/// Notification that happens when the client gets disconnected from the server.
	/// </summary>

	void OnDisconnect ()
	{
		mJoiningChannel = false;
		UnityTools.Broadcast("OnNetworkDisconnect");
	}

	/// <summary>
	/// Notification sent when attempting to join a channel, indicating a success or failure.
	/// </summary>

	void OnJoinChannel (bool success, string message)
	{
		mJoiningChannel = false;
		UnityTools.Broadcast("OnNetworkJoinChannel", success, message);
	}

	/// <summary>
	/// Notification sent when leaving a channel.
	/// Also sent just before a disconnect (if inside a channel when it happens).
	/// </summary>

	void OnLeftChannel ()
	{
		mJoiningChannel = false;
		UnityTools.Broadcast("OnNetworkLeaveChannel");
	}

	/// <summary>
	/// Notification sent when a level is changing.
	/// </summary>

	void OnLoadLevel (string levelName) { if (!string.IsNullOrEmpty(levelName)) Application.LoadLevel(levelName); }

	/// <summary>
	/// Notification of a new player joining the channel.
	/// </summary>

	void OnPlayerJoined (Player p) { UnityTools.Broadcast("OnNetworkPlayerJoin", p); }

	/// <summary>
	/// Notification of another player leaving the channel.
	/// </summary>

	void OnPlayerLeft (Player p) { UnityTools.Broadcast("OnNetworkPlayerLeave", p); }

	/// <summary>
	/// Notification of player's data changing.
	/// </summary>

	void OnPlayerSync (Player p) { if (onPlayerSync != null) onPlayerSync(p); }

	/// <summary>
	/// Notification of a player being renamed.
	/// </summary>

	void OnRenamePlayer (Player p, string previous)
	{
		mPlayer.name = p.name;
		UnityTools.Broadcast("OnNetworkPlayerRenamed", p, previous);
	}
#endregion
}
