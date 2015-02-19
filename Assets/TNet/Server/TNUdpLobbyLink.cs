//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TNet
{
/// <summary>
/// UDP-based lobby server link. Designed to communicate with a remote UdpLobbyServer.
/// You can use this class to register your game server with a remote lobby server.
/// </summary>

public class UdpLobbyServerLink : LobbyServerLink
{
	UdpProtocol mUdp;
	IPEndPoint mRemoteAddress;
	long mNextSend = 0;

	/// <summary>
	/// Create a new link to a remote lobby server.
	/// </summary>

	public UdpLobbyServerLink (IPEndPoint address) : base(null) { mRemoteAddress = address; }

	/// <summary>
	/// Whether the link is currently active.
	/// </summary>

	public override bool isActive { get { return mUdp != null && mUdp.isActive; } }

	/// <summary>
	/// Make sure the socket gets released.
	/// </summary>

	~UdpLobbyServerLink ()
	{
		if (mUdp != null)
		{
			mUdp.Stop();
			mUdp = null;
		}
	}

	/// <summary>
	/// Start the lobby server link.
	/// </summary>

	public override void Start ()
	{
		base.Start();

		if (mUdp == null)
		{
			mUdp = new UdpProtocol();
			mUdp.Start();
		}
	}

	/// <summary>
	/// Send a server update.
	/// </summary>

	public override void SendUpdate (GameServer server)
	{
		if (!mShutdown)
		{
			mNextSend = 0;
			mGameServer = server;

			if (mThread == null)
			{
				mThread = new Thread(ThreadFunction);
				mThread.Start();
			}
		}
	}

	/// <summary>
	/// Send periodic updates.
	/// </summary>

	void ThreadFunction()
	{
		mInternal = new IPEndPoint(Tools.localAddress, mGameServer.tcpPort);
		mExternal = new IPEndPoint(Tools.externalAddress, mGameServer.tcpPort);

		for (; ; )
		{
			long time = DateTime.UtcNow.Ticks / 10000;

			if (mShutdown)
			{
				Buffer buffer = Buffer.Create();
				BinaryWriter writer = buffer.BeginPacket(Packet.RequestRemoveServer);
				writer.Write(GameServer.gameID);
				Tools.Serialize(writer, mInternal);
				Tools.Serialize(writer, mExternal);
				buffer.EndPacket();
				mUdp.Send(buffer, mRemoteAddress);
				buffer.Recycle();
				mThread = null;
				break;
			}

			if (mNextSend < time && mGameServer != null)
			{
				mNextSend = time + 3000;
				Buffer buffer = Buffer.Create();
				BinaryWriter writer = buffer.BeginPacket(Packet.RequestAddServer);
				writer.Write(GameServer.gameID);
				writer.Write(mGameServer.name);
				writer.Write((short)mGameServer.playerCount);
				Tools.Serialize(writer, mInternal);
				Tools.Serialize(writer, mExternal);
				buffer.EndPacket();
				mUdp.Send(buffer, mRemoteAddress);
				buffer.Recycle();
			}
			Thread.Sleep(10);
		}
	}
}
}
