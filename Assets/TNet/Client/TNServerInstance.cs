//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

#define MULTI_THREADED

using UnityEngine;
using TNet;
using System.IO;
using System.Collections;
using System.Net;

/// <summary>
/// Tasharen Network server tailored for Unity.
/// </summary>

[AddComponentMenu("TNet/Network Server (internal)")]
public class TNServerInstance : MonoBehaviour
{
	static TNServerInstance mInstance;

	public enum Type
	{
		Lan,
		Udp,
		Tcp,
	}

	public enum State
	{
		Inactive,
		Starting,
		Active,
	}

	GameServer mGame = new GameServer();
	LobbyServer mLobby;
	UPnP mUp = new UPnP();

	/// <summary>
	/// Instance access is internal only as all the functions are static for convenience purposes.
	/// </summary>

	static TNServerInstance instance
	{
		get
		{
			if (mInstance == null)
			{
				GameObject go = new GameObject("_Server");
				mInstance = go.AddComponent<TNServerInstance>();
				DontDestroyOnLoad(go);
			}
			return mInstance;
		}
	}

	/// <summary>
	/// Whether the server instance is currently active.
	/// </summary>

	static public bool isActive { get { return (mInstance != null) && mInstance.mGame.isActive; } }

	/// <summary>
	/// Whether the server is currently listening for incoming connections.
	/// </summary>

	static public bool isListening { get { return (mInstance != null) && mInstance.mGame.isListening; } }

	/// <summary>
	/// Port used to listen for incoming TCP connections.
	/// </summary>

	static public int listeningPort { get { return (mInstance != null) ? mInstance.mGame.tcpPort : 0; } }

	/// <summary>
	/// Set your server's name.
	/// </summary>

	static public string serverName { get { return (mInstance != null) ? mInstance.mGame.name : null; } set { if (instance != null) mInstance.mGame.name = value; } }

	/// <summary>
	/// How many players are currently connected to the server.
	/// </summary>

	static public int playerCount { get { return (mInstance != null) ? mInstance.mGame.playerCount : 0; } }

	/// <summary>
	/// Active game server.
	/// </summary>

	static public GameServer game { get { return (mInstance != null) ? mInstance.mGame : null; } }

	/// <summary>
	/// Active lobby server.
	/// </summary>

	static public LobbyServer lobby { get { return (mInstance != null) ? mInstance.mLobby : null; } }

	/// <summary>
	/// Start a local server instance listening to the specified port.
	/// </summary>

	static public bool Start (int tcpPort)
	{
		return instance.StartLocal(tcpPort, 0, null, 0, Type.Udp);
	}

	/// <summary>
	/// Start a local server instance listening to the specified port.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort)
	{
		return instance.StartLocal(tcpPort, udpPort, null, 0, Type.Udp);
	}

	/// <summary>
	/// Start a local server instance listening to the specified port and loading the saved data from the specified file.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort, string fileName)
	{
		return instance.StartLocal(tcpPort, udpPort, fileName, 0, Type.Udp);
	}

	[System.Obsolete("Use TNServerInstance.Start(tcpPort, udpPort, lobbyPort, fileName) instead")]
	static public bool Start (int tcpPort, int udpPort, string fileName, int lanBroadcastPort)
	{
		return instance.StartLocal(tcpPort, udpPort, fileName, lanBroadcastPort, Type.Udp);
	}

	/// <summary>
	/// Start a local game and lobby server instances.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort, int lobbyPort, string fileName)
	{
		return instance.StartLocal(tcpPort, udpPort, fileName, lobbyPort, Type.Udp);
	}

	/// <summary>
	/// Start a local game and lobby server instances.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort, int lobbyPort, string fileName, Type type)
	{
		return instance.StartLocal(tcpPort, udpPort, fileName, lobbyPort, type);
	}

	/// <summary>
	/// Start a local game server and connect to a remote lobby server.
	/// </summary>

	static public bool Start (int tcpPort, int udpPort, string fileName, Type type, IPEndPoint remoteLobby)
	{
		return instance.StartRemote(tcpPort, udpPort, fileName, remoteLobby, type);
	}

	/// <summary>
	/// Start a new server.
	/// </summary>

	bool StartLocal (int tcpPort, int udpPort, string fileName, int lobbyPort, Type type)
	{
		// Ensure that everything has been stopped first
		if (mGame.isActive) Disconnect();

		// If there is a lobby port, we should set up the lobby server and/or link first.
		// Doing so will let us inform the lobby that we are starting a new server.

		if (lobbyPort > 0)
		{
			if (type == Type.Tcp) mLobby = new TcpLobbyServer();
			else mLobby = new UdpLobbyServer();

			// Start a local lobby server
			if (mLobby.Start(lobbyPort))
			{
				if (type == Type.Tcp) mUp.OpenTCP(lobbyPort);
				else mUp.OpenUDP(lobbyPort);
			}
			else
			{
				mLobby = null;
				return false;
			}

			// Create the local lobby link
			mGame.lobbyLink = new LobbyServerLink(mLobby);
		}

		// Start the game server
		if (mGame.Start(tcpPort, udpPort))
		{
			mUp.OpenTCP(tcpPort);
			mUp.OpenUDP(udpPort);
			if (!string.IsNullOrEmpty(fileName)) mGame.LoadFrom(fileName);
			return true;
		}

		// Something went wrong -- stop everything
		Disconnect();
		return false;
	}

	/// <summary>
	/// Start a new server.
	/// </summary>

	bool StartRemote (int tcpPort, int udpPort, string fileName, IPEndPoint remoteLobby, Type type)
	{
		if (mGame.isActive) Disconnect();

		if (remoteLobby != null && remoteLobby.Port > 0)
		{
			if (type == Type.Tcp)
			{
				mLobby = new TcpLobbyServer();
				mGame.lobbyLink = new TcpLobbyServerLink(remoteLobby);
			}
			else if (type == Type.Udp)
			{
				mLobby = new UdpLobbyServer();
				mGame.lobbyLink = new UdpLobbyServerLink(remoteLobby);
			}
			else
			{
				Debug.LogWarning("The remote lobby server type must be either UDP or TCP, not LAN");
			}
		}

		if (mGame.Start(tcpPort, udpPort))
		{
			mUp.OpenTCP(tcpPort);
			mUp.OpenUDP(udpPort);
			if (!string.IsNullOrEmpty(fileName)) mGame.LoadFrom(fileName);
			return true;
		}

		Disconnect();
		return false;
	}

	/// <summary>
	/// Stop the server.
	/// </summary>

	static public void Stop () { if (mInstance != null) mInstance.Disconnect(); }

	/// <summary>
	/// Stop the server, saving the current state to the specified file.
	/// </summary>

	static public void Stop (string fileName)
	{
		if (mInstance != null && mInstance.mGame.isActive)
		{
			if (!string.IsNullOrEmpty(fileName))
				mInstance.mGame.SaveTo(fileName);
			Stop();
		}
	}

	/// <summary>
	/// Save the server's current state to the specified file.
	/// </summary>

	static public void SaveTo (string fileName)
	{
		if (mInstance != null && mInstance.mGame.isActive)
		{
			if (!string.IsNullOrEmpty(fileName))
				mInstance.mGame.SaveTo(fileName);
		}
	}

	/// <summary>
	/// Make the server private by no longer accepting new connections.
	/// </summary>

	static public void MakePrivate () { if (mInstance != null) mInstance.mGame.MakePrivate(); }

	/// <summary>
	/// Stop everything.
	/// </summary>

	void Disconnect ()
	{
		mGame.Stop();

		if (mLobby != null)
		{
			mLobby.Stop();
			mLobby = null;
		}
		mUp.Close();
	}

	/// <summary>
	/// Make sure that the servers are stopped when the server instance is destroyed.
	/// </summary>

	void OnDestroy ()
	{
		Disconnect();
		mUp.WaitForThreads();
	}

#if !MULTI_THREADED
	void Update () { if (mGame != null && mGame.isListening) mGame.Update(); }
#endif
}
