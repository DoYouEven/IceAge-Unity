//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System.Net;
using System.Threading;
using System;

namespace TNet
{
/// <summary>
/// The game server cannot communicate directly with a lobby server because that server can be TCP or UDP based,
/// and may also be hosted either locally or on another computer. And so we use a different class to "link" them
/// together -- the LobbyServerLink. This class will link a game server with a local lobby server.
/// </summary>

public class LobbyServerLink
{
	LobbyServer mLobby;
	long mNextSend = 0;

	protected GameServer mGameServer;
	protected Thread mThread;
	protected IPEndPoint mInternal;
	protected IPEndPoint mExternal;

	// Thread-safe flag indicating that the server should shut down at the first available opportunity
	protected bool mShutdown = false;

	/// <summary>
	/// Create a new local lobby server link. Expects a local server to work with.
	/// </summary>

	public LobbyServerLink (LobbyServer lobbyServer) { mLobby = lobbyServer; }

	/// <summary>
	/// Whether the link is currently active.
	/// </summary>

	public virtual bool isActive { get { return (mLobby != null && mExternal != null); } }

	/// <summary>
	/// Start the lobby server link. Establish a connection, if one is required.
	/// </summary>

	public virtual void Start () { mShutdown = false; }

	/// <summary>
	/// Stopping the server should be delayed in order for it to be thread-safe.
	/// </summary>

	public virtual void Stop ()
	{
		if (!mShutdown)
		{
			mShutdown = true;
			if (mExternal != null && mLobby != null)
				mLobby.RemoveServer(mInternal, mExternal);
		}
	}

	/// <summary>
	/// Send an update to the lobby server. Triggered by the game server.
	/// </summary>

	public virtual void SendUpdate (GameServer gameServer)
	{
		if (!mShutdown)
		{
			mGameServer = gameServer;

			if (mExternal != null)
			{
				long time = DateTime.UtcNow.Ticks / 10000;
				mNextSend = time + 3000;
				mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
			}
			else if (mThread == null)
			{
				mThread = new Thread(SendThread);
				mThread.Start();
			}
		}
	}

	/// <summary>
	/// Resolve the IPs and start periodic updates.
	/// </summary>

	void SendThread ()
	{
		mInternal = new IPEndPoint(Tools.localAddress, mGameServer.tcpPort);
		mExternal = new IPEndPoint(Tools.externalAddress, mGameServer.tcpPort);

		// UDP updates need to be periodic, while the TCP update only needs to happen once
		if (mLobby is UdpLobbyServer)
		{
			while (!mShutdown)
			{
				long time = DateTime.UtcNow.Ticks / 10000;

				if (mNextSend < time && mGameServer != null)
				{
					mNextSend = time + 3000;
					mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
				}
				Thread.Sleep(10);
			}
		}
		else mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
		mThread = null;
	}
}
}
