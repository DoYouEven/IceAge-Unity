//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace TNet
{
/// <summary>
/// Client-side logic.
/// </summary>

public class GameClient
{
	public delegate void OnPing (IPEndPoint ip, int milliSeconds);
	public delegate void OnError (string message);
	public delegate void OnConnect (bool success, string message);
	public delegate void OnDisconnect ();
	public delegate void OnJoinChannel (bool success, string message);
	public delegate void OnLeftChannel ();
	public delegate void OnLoadLevel (string levelName);
	public delegate void OnPlayerJoined (Player p);
	public delegate void OnPlayerLeft (Player p);
	public delegate void OnPlayerSync (Player p);
	public delegate void OnRenamePlayer (Player p, string previous);
	public delegate void OnSetHost (bool hosting);
	public delegate void OnSetChannelData (string data);
	public delegate void OnCreate (int creator, int index, uint objID, BinaryReader reader);
	public delegate void OnDestroy (uint objID);
	public delegate void OnForwardedPacket (BinaryReader reader);
	public delegate void OnPacket (Packet response, BinaryReader reader, IPEndPoint source);
	public delegate void OnLoadFile (string filename, byte[] data);

	/// <summary>
	/// Custom packet listeners. You can set these to handle custom packets.
	/// </summary>

	public Dictionary<byte, OnPacket> packetHandlers = new Dictionary<byte, OnPacket>();

	/// <summary>
	/// Ping notification.
	/// </summary>

	public OnPing onPing;

	/// <summary>
	/// Error notification.
	/// </summary>

	public OnError onError;

	/// <summary>
	/// Connection attempt result indicating success or failure.
	/// </summary>

	public OnConnect onConnect;

	/// <summary>
	/// Notification sent after the connection terminates for any reason.
	/// </summary>

	public OnDisconnect onDisconnect;

	/// <summary>
	/// Notification sent when attempting to join a channel, indicating a success or failure.
	/// </summary>

	public OnJoinChannel onJoinChannel;

	/// <summary>
	/// Notification sent when leaving a channel.
	/// Also sent just before a disconnect (if inside a channel when it happens).
	/// </summary>

	public OnLeftChannel onLeftChannel;

	/// <summary>
	/// Notification sent when changing levels.
	/// </summary>

	public OnLoadLevel onLoadLevel;

	/// <summary>
	/// Notification sent when a new player joins the channel.
	/// </summary>

	public OnPlayerJoined onPlayerJoined;

	/// <summary>
	/// Notification sent when a player leaves the channel.
	/// </summary>

	public OnPlayerLeft onPlayerLeft;

	/// <summary>
	/// Notification sent when player data gets synchronized.
	/// </summary>

	public OnPlayerSync onPlayerSync;

	/// <summary>
	/// Notification of some player changing their name.
	/// </summary>

	public OnRenamePlayer onRenamePlayer;

	/// <summary>
	/// Notification sent when the channel's host changes.
	/// </summary>

	public OnSetHost onSetHost;

	/// <summary>
	/// Notification of the channel's custom data changing.
	/// </summary>

	public OnSetChannelData onSetChannelData;

	/// <summary>
	/// Notification of a new object being created.
	/// </summary>

	public OnCreate onCreate;

	/// <summary>
	/// Notification of the specified object being destroyed.
	/// </summary>

	public OnDestroy onDestroy;

	/// <summary>
	/// Notification of a client packet arriving.
	/// </summary>

	public OnForwardedPacket onForwardedPacket;

	/// <summary>
	/// List of players in the same channel as the client.
	/// </summary>

	public List<Player> players = new List<Player>();

	/// <summary>
	/// Whether the game client should be actively processing messages or not.
	/// </summary>

	public bool isActive = true;

	// Same list of players, but in a dictionary format for quick lookup
	Dictionary<int, Player> mDictionary = new Dictionary<int, Player>();

	// TCP connection is the primary method of communication with the server.
	TcpProtocol mTcp = new TcpProtocol();

#if !UNITY_WEBPLAYER
	// UDP can be used for transmission of frequent packets, network broadcasts and NAT requests.
	// UDP is not available in the Unity web player because using UDP packets makes Unity request the
	// policy file every time the packet gets sent... which is obviously quite retarded.
	UdpProtocol mUdp = new UdpProtocol();
	bool mUdpIsUsable = false;
#endif

	// ID of the host
	int mHost = 0;
	int mChannelID = 0;

	// Current time, time when the last ping was sent out, and time when connection was started
	long mTimeDifference = 0;
	long mMyTime = 0;
	long mPingTime = 0;

	// Last ping, and whether we can ping again
	int mPing = 0;
	bool mCanPing = false;
	bool mIsInChannel = false;
	string mData = "";

	// Each LoadFile() call can specify its own callback
	List<OnLoadFile> mLoadFiles = new List<OnLoadFile>();

	// Server's UDP address
	IPEndPoint mServerUdpEndPoint;

	// Source of the UDP packet (available during callbacks)
	IPEndPoint mPacketSource;

	// Temporary, not important
	static Buffer mBuffer;

	/// <summary>
	/// ID of the channel we're in.
	/// </summary>

	public int channelID { get { return mChannelID; } }

	/// <summary>
	/// Current time on the server.
	/// </summary>

	public long serverTime { get { return mTimeDifference + (System.DateTime.UtcNow.Ticks / 10000); } }

	/// <summary>
	/// ID of the host.
	/// </summary>

	public int hostID { get { return mTcp.isConnected ? mHost : mTcp.id; } }

	/// <summary>
	/// Whether the client is currently connected to the server.
	/// </summary>

	public bool isConnected { get { return mTcp.isConnected; } }

	/// <summary>
	/// Whether we are currently trying to establish a new connection.
	/// </summary>

	public bool isTryingToConnect { get { return mTcp.isTryingToConnect; } }

	/// <summary>
	/// Whether this player is hosting the game.
	/// </summary>

	public bool isHosting { get { return !mIsInChannel || mHost == mTcp.id; } }

	/// <summary>
	/// Whether the client is currently in a channel.
	/// </summary>

	public bool isInChannel { get { return mIsInChannel; } }

	/// <summary>
	/// Port used to listen for incoming UDP packets. Set via StartUDP().
	/// </summary>

	public int listeningPort
	{
		get
		{
#if UNITY_WEBPLAYER
			return 0;
#else
			return mUdp.listeningPort;
#endif
		}
	}

	/// <summary>
	/// Source of the last packet.
	/// </summary>

	public IPEndPoint packetSource { get { return mPacketSource != null ? mPacketSource : mTcp.tcpEndPoint; } }

	/// <summary>
	/// Set the custom data associated with the channel we're in.
	/// </summary>

	public string channelData
	{
		get
		{
			return isInChannel ? mData : "";
		}
		set
		{
			if (isHosting && isInChannel && !mData.Equals(value))
			{
				mData = value;
				BeginSend(Packet.RequestSetChannelData).Write(value);
				EndSend();
			}
		}
	}

	/// <summary>
	/// Enable or disable the Nagle's buffering algorithm (aka NO_DELAY flag).
	/// Enabling this flag will improve latency at the cost of increased bandwidth.
	/// http://en.wikipedia.org/wiki/Nagle's_algorithm
	/// </summary>

	public bool noDelay
	{
		get
		{
			return mTcp.noDelay;
		}
		set
		{
			if (mTcp.noDelay != value)
			{
				mTcp.noDelay = value;
				
				// Notify the server as well so that the server does the same
				BeginSend(Packet.RequestNoDelay).Write(value);
				EndSend();
			}
		}
	}

	/// <summary>
	/// Current ping to the server.
	/// </summary>

	public int ping { get { return isConnected ? mPing : 0; } }

	/// <summary>
	/// Whether we can communicate with the server via UDP.
	/// </summary>

	public bool canUseUDP
	{
		get
		{
#if UNITY_WEBPLAYER
			return false;
#else
			return mUdp.isActive && mServerUdpEndPoint != null;
#endif
		}
	}

	/// <summary>
	/// Return the local player.
	/// </summary>

	public Player player { get { return mTcp; } }

	/// <summary>
	/// The player's unique identifier.
	/// </summary>

	public int playerID { get { return mTcp.id; } }

	/// <summary>
	/// Name of this player.
	/// </summary>

	public string playerName
	{
		get
		{
			return mTcp.name;
		}
		set
		{
			if (mTcp.name != value)
			{
				if (isConnected)
				{
					BinaryWriter writer = BeginSend(Packet.RequestSetName);
					writer.Write(value);
					EndSend();
				}
				else mTcp.name = value;
			}
		}
	}

	/// <summary>
	/// Get or set the player's data.
	/// </summary>

	public object playerData
	{
		get
		{
			return mTcp.data;
		}
		set
		{
			mTcp.data = value;
			SyncPlayerData();
		}
	}

	/// <summary>
	/// Immediately sync the player's data.
	/// </summary>

	public void SyncPlayerData ()
	{
		if (isConnected)
		{
			BinaryWriter writer = BeginSend(Packet.SyncPlayerData);
			writer.Write(mTcp.id);
			writer.WriteObject(mTcp.data);
			EndSend();
		}
	}

	/// <summary>
	/// Retrieve a player by their ID.
	/// </summary>

	public Player GetPlayer (int id)
	{
		if (id == mTcp.id) return mTcp;

		if (isConnected)
		{
			Player player = null;
			mDictionary.TryGetValue(id, out player);
			return player;
		}
		return null;
	}

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	public BinaryWriter BeginSend (Packet type)
	{
		mBuffer = Buffer.Create();
		return mBuffer.BeginPacket(type);
	}

	/// <summary>
	/// Begin sending a new packet to the server.
	/// </summary>

	public BinaryWriter BeginSend (byte packetID)
	{
		mBuffer = Buffer.Create();
		return mBuffer.BeginPacket(packetID);
	}

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	public void EndSend ()
	{
		if (mBuffer != null)
		{
			mBuffer.EndPacket();
			mTcp.SendTcpPacket(mBuffer);
			mBuffer.Recycle();
			mBuffer = null;
		}
	}

	/// <summary>
	/// Send the outgoing buffer.
	/// </summary>

	public void EndSend (bool reliable)
	{
		mBuffer.EndPacket();
#if UNITY_WEBPLAYER
		mTcp.SendTcpPacket(mBuffer);
#else
		if (reliable || !mUdpIsUsable || mServerUdpEndPoint == null || !mUdp.isActive)
		{
			mTcp.SendTcpPacket(mBuffer);
		}
		else mUdp.Send(mBuffer, mServerUdpEndPoint);
#endif
		mBuffer.Recycle();
		mBuffer = null;
	}

	/// <summary>
	/// Broadcast the outgoing buffer to the entire LAN via UDP.
	/// </summary>

	public void EndSend (int port)
	{
		mBuffer.EndPacket();
#if !UNITY_WEBPLAYER
		mUdp.Broadcast(mBuffer, port);
#endif
		mBuffer.Recycle();
		mBuffer = null;
	}

	/// <summary>
	/// Send this packet to a remote UDP listener.
	/// </summary>

	public void EndSend (IPEndPoint target)
	{
		mBuffer.EndPacket();
#if !UNITY_WEBPLAYER
		mUdp.Send(mBuffer, target);
#endif
		mBuffer.Recycle();
		mBuffer = null;
	}

	/// <summary>
	/// Try to establish a connection with the specified address.
	/// </summary>

	public void Connect (IPEndPoint externalIP, IPEndPoint internalIP)
	{
		Disconnect();
		mTcp.Connect(externalIP, internalIP);
	}

	/// <summary>
	/// Disconnect from the server.
	/// </summary>

	public void Disconnect () { mTcp.Disconnect(); }

	/// <summary>
	/// Start listening to incoming UDP packets on the specified port.
	/// </summary>

	public bool StartUDP (int udpPort)
	{
#if !UNITY_WEBPLAYER
		if (mUdp.Start(udpPort))
		{
			if (isConnected)
			{
				BeginSend(Packet.RequestSetUDP).Write((ushort)udpPort);
				EndSend();
			}
			return true;
		}
#endif
		return false;
	}

	/// <summary>
	/// Stop listening to incoming broadcasts.
	/// </summary>

	public void StopUDP ()
	{
#if !UNITY_WEBPLAYER
		if (mUdp.isActive)
		{
			if (isConnected)
			{
				BeginSend(Packet.RequestSetUDP).Write((ushort)0);
				EndSend();
			}
			mUdp.Stop();
			mUdpIsUsable = false;
		}
#endif
	}

	/// <summary>
	/// Join the specified channel.
	/// </summary>
	/// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
	/// <param name="levelName">Level that will be loaded first.</param>
	/// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
	/// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
	/// <param name="password">Password for the channel. First player sets the password.</param>

	public void JoinChannel (int channelID, string levelName, bool persistent, int playerLimit, string password)
	{
		if (isConnected)
		{
			BinaryWriter writer = BeginSend(Packet.RequestJoinChannel);
			writer.Write(channelID);
			writer.Write(string.IsNullOrEmpty(password) ? "" : password);
			writer.Write(string.IsNullOrEmpty(levelName) ? "" : levelName);
			writer.Write(persistent);
			writer.Write((ushort)playerLimit);
			EndSend();
		}
	}

	/// <summary>
	/// Close the channel the player is in. New players will be prevented from joining.
	/// Once a channel has been closed, it cannot be re-opened.
	/// </summary>

	public void CloseChannel ()
	{
		if (isConnected && isInChannel)
		{
			BeginSend(Packet.RequestCloseChannel);
			EndSend();
		}
	}

	/// <summary>
	/// Leave the current channel.
	/// </summary>

	public void LeaveChannel ()
	{
		if (isConnected && isInChannel)
		{
			BeginSend(Packet.RequestLeaveChannel);
			EndSend();
		}
	}

	/// <summary>
	/// Change the maximum number of players that can join the channel the player is currently in.
	/// </summary>

	public void SetPlayerLimit (int max)
	{
		if (isConnected && isInChannel)
		{
			BeginSend(Packet.RequestSetPlayerLimit).Write((ushort)max);
			EndSend();
		}
	}

	/// <summary>
	/// Switch the current level.
	/// </summary>

	public void LoadLevel (string levelName)
	{
		if (isConnected && isInChannel)
		{
			BeginSend(Packet.RequestLoadLevel).Write(levelName);
			EndSend();
		}
	}

	/// <summary>
	/// Change the hosting player.
	/// </summary>

	public void SetHost (Player player)
	{
		if (isConnected && isInChannel && isHosting)
		{
			BinaryWriter writer = BeginSend(Packet.RequestSetHost);
			writer.Write(player.id);
			EndSend();
		}
	}

	/// <summary>
	/// Set the timeout for the player. By default it's 10 seconds. If you know you are about to load a large level,
	/// and it's going to take, say 60 seconds, set this timeout to 120 seconds just to be safe. When the level
	/// finishes loading, change this back to 10 seconds so that dropped connections gets detected correctly.
	/// </summary>

	public void SetTimeout (int seconds)
	{
		if (isConnected)
		{
			BeginSend(Packet.RequestSetTimeout).Write(seconds);
			EndSend();
		}
	}

	/// <summary>
	/// Send a remote ping request to the specified TNet server.
	/// </summary>

	public void Ping (IPEndPoint udpEndPoint, OnPing callback)
	{
		onPing = callback;
		mPingTime = DateTime.UtcNow.Ticks / 10000;
		BeginSend(Packet.RequestPing);
		EndSend(udpEndPoint);
	}

	/// <summary>
	/// Load the specified file from the server.
	/// </summary>

	public void LoadFile (string filename, OnLoadFile callback)
	{
		mLoadFiles.Add(callback);
		BinaryWriter writer = BeginSend(Packet.RequestLoadFile);
		writer.Write(filename);
		EndSend();
	}

	/// <summary>
	/// Save the specified file on the server.
	/// </summary>

	public void SaveFile (string filename, byte[] data)
	{
		BinaryWriter writer = BeginSend(Packet.RequestSaveFile);
		writer.Write(filename);
		writer.Write(data.Length);
		writer.Write(data);
		EndSend();
	}

	/// <summary>
	/// Process all incoming packets.
	/// </summary>

	public void ProcessPackets ()
	{
		mMyTime = DateTime.UtcNow.Ticks / 10000;

		// Request pings every so often, letting the server know we're still here.
		if (mTcp.isConnected && mCanPing && mPingTime + 4000 < mMyTime)
		{
			mCanPing = false;
			mPingTime = mMyTime;
			BeginSend(Packet.RequestPing);
			EndSend();
		}

		Buffer buffer = null;
		bool keepGoing = true;

#if !UNITY_WEBPLAYER
		IPEndPoint ip = null;

		while (keepGoing && isActive && mUdp.ReceivePacket(out buffer, out ip))
		{
			mUdpIsUsable = true;
			keepGoing = ProcessPacket(buffer, ip);
			buffer.Recycle();
		}
#endif
		while (keepGoing && isActive && mTcp.ReceivePacket(out buffer))
		{
			keepGoing = ProcessPacket(buffer, null);
			buffer.Recycle();
		}
	}

	/// <summary>
	/// Process a single incoming packet. Returns whether we should keep processing packets or not.
	/// </summary>

	bool ProcessPacket (Buffer buffer, IPEndPoint ip)
	{
		mPacketSource = ip;
		BinaryReader reader = buffer.BeginReading();
		if (buffer.size == 0) return true;

		int packetID = reader.ReadByte();
		Packet response = (Packet)packetID;

		// Verification step must be passed first
		if (response == Packet.ResponseID || mTcp.stage == TcpProtocol.Stage.Verifying)
		{
			if (mTcp.VerifyResponseID(response, reader))
			{
				mTimeDifference = reader.ReadInt64() - (System.DateTime.UtcNow.Ticks / 10000);

#if !UNITY_WEBPLAYER
				if (mUdp.isActive)
				{
					// If we have a UDP listener active, tell the server
					BeginSend(Packet.RequestSetUDP).Write((ushort)mUdp.listeningPort);
					EndSend();
				}
#endif
				mCanPing = true;
				if (onConnect != null) onConnect(true, null);
			}
			return true;
		}

//#if !UNITY_EDITOR // DEBUG
//		if (response != Packet.ResponsePing) Console.WriteLine("Client: " + response + " " + buffer.position + " of " + buffer.size + ((ip == null) ? " (TCP)" : " (UDP)"));
//#else
//		if (response != Packet.ResponsePing) UnityEngine.Debug.Log("Client: " + response + " " + buffer.position + " of " + buffer.size + ((ip == null) ? " (TCP)" : " (UDP)"));
//#endif

		OnPacket callback;

		if (packetHandlers.TryGetValue((byte)response, out callback) && callback != null)
		{
			callback(response, reader, ip);
			return true;
		}

		switch (response)
		{
			case Packet.Empty: break;
			case Packet.ForwardToAll:
			case Packet.ForwardToOthers:
			case Packet.ForwardToAllSaved:
			case Packet.ForwardToOthersSaved:
			case Packet.ForwardToHost:
			case Packet.Broadcast:
			{
				if (onForwardedPacket != null) onForwardedPacket(reader);
				break;
			}
			case Packet.ForwardToPlayer:
			{
				// Skip the player ID
				reader.ReadInt32();
				if (onForwardedPacket != null) onForwardedPacket(reader);
				break;
			}
			case Packet.ForwardByName:
			{
				// Skip the player name
				reader.ReadString();
				if (onForwardedPacket != null) onForwardedPacket(reader);
				break;
			}
			case Packet.SyncPlayerData:
			{
				Player target = GetPlayer(reader.ReadInt32());

				if (target != null)
				{
					target.data = reader.ReadObject();
					if (onPlayerSync != null) onPlayerSync(target);
				}
				break;
			}
			case Packet.ResponsePing:
			{
				int ping = (int)(mMyTime - mPingTime);

				if (ip != null)
				{
					if (onPing != null && ip != null) onPing(ip, ping);
				}
				else
				{
					mCanPing = true;
					mPing = ping;
				}
				break;
			}
			case Packet.ResponseSetUDP:
			{
#if !UNITY_WEBPLAYER
				// The server has a new port for UDP traffic
				ushort port = reader.ReadUInt16();

				if (port != 0)
				{
					IPAddress ipa = new IPAddress(mTcp.tcpEndPoint.Address.GetAddressBytes());
					mServerUdpEndPoint = new IPEndPoint(ipa, port);

					// Send the first UDP packet to the server
					if (mUdp.isActive)
					{
						mBuffer = Buffer.Create();
						mBuffer.BeginPacket(Packet.RequestActivateUDP).Write(playerID);
						mBuffer.EndPacket();
						mUdp.Send(mBuffer, mServerUdpEndPoint);
						mBuffer.Recycle();
						mBuffer = null;
					}
				}
				else mServerUdpEndPoint = null;
#endif
				break;
			}
			case Packet.ResponseJoiningChannel:
			{
				mIsInChannel = true;
				mDictionary.Clear();
				players.Clear();

				mChannelID = reader.ReadInt32();
				int count = reader.ReadInt16();

				for (int i = 0; i < count; ++i)
				{
					Player p = new Player();
					p.id = reader.ReadInt32();
					p.name = reader.ReadString();
					p.data = reader.ReadObject();
					mDictionary.Add(p.id, p);
					players.Add(p);
				}
				break;
			}
			case Packet.ResponseLoadLevel:
			{
				// Purposely return after loading a level, ensuring that all future callbacks happen after loading
				if (onLoadLevel != null) onLoadLevel(reader.ReadString());
				return false;
			}
			case Packet.ResponsePlayerLeft:
			{
				Player p = GetPlayer(reader.ReadInt32());
				if (p != null) mDictionary.Remove(p.id);
				players.Remove(p);
				if (onPlayerLeft != null) onPlayerLeft(p);
				break;
			}
			case Packet.ResponsePlayerJoined:
			{
				Player p = new Player();
				p.id = reader.ReadInt32();
				p.name = reader.ReadString();
				p.data = reader.ReadObject();
				mDictionary.Add(p.id, p);
				players.Add(p);
				if (onPlayerJoined != null) onPlayerJoined(p);
				break;
			}
			case Packet.ResponseSetHost:
			{
				mHost = reader.ReadInt32();
				if (onSetHost != null) onSetHost(isHosting);
				break;
			}
			case Packet.ResponseSetChannelData:
			{
				mData = reader.ReadString();
				if (onSetChannelData != null) onSetChannelData(mData);
				break;
			}
			case Packet.ResponseJoinChannel:
			{
				mIsInChannel = reader.ReadBoolean();
				if (onJoinChannel != null) onJoinChannel(mIsInChannel, mIsInChannel ? null : reader.ReadString());
				break;
			}
			case Packet.ResponseLeaveChannel:
			{
				mData = "";
				mChannelID = 0;
				mIsInChannel = false;
				mDictionary.Clear();
				players.Clear();
				if (onLeftChannel != null) onLeftChannel();
				break;
			}
			case Packet.ResponseRenamePlayer:
			{
				Player p = GetPlayer(reader.ReadInt32());
				string oldName = p.name;
				if (p != null) p.name = reader.ReadString();
				if (onRenamePlayer != null) onRenamePlayer(p, oldName);
				break;
			}
			case Packet.ResponseCreate:
			{
				if (onCreate != null)
				{
					int playerID = reader.ReadInt32();
					ushort index = reader.ReadUInt16();
					uint objID = reader.ReadUInt32();
					onCreate(playerID, index, objID, reader);
				}
				break;
			}
			case Packet.ResponseDestroy:
			{
				if (onDestroy != null)
				{
					int count = reader.ReadUInt16();
					for (int i = 0; i < count; ++i) onDestroy(reader.ReadUInt32());
				}
				break;
			}
			case Packet.Error:
			{
				if (mTcp.stage != TcpProtocol.Stage.Connected && onConnect != null)
				{
					onConnect(false, reader.ReadString());
				}
				else if (onError != null)
				{
					onError(reader.ReadString());
				}
				break;
			}
			case Packet.Disconnect:
			{
				mData = "";
				if (isInChannel && onLeftChannel != null) onLeftChannel();
				players.Clear();
				mDictionary.Clear();
				mTcp.Close(false);
				mLoadFiles.Clear();
				if (onDisconnect != null) onDisconnect();
				break;
			}
			case Packet.ResponseLoadFile:
			{
				string filename = reader.ReadString();
				int size = reader.ReadInt32();
				byte[] data = reader.ReadBytes(size);
				OnLoadFile lfc = mLoadFiles.Pop();
				if (lfc != null) lfc(filename, data);
				break;
			}
		}
		return true;
	}
}
}
