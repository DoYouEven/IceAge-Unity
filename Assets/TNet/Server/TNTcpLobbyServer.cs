//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Net;

namespace TNet
{
/// <summary>
/// Optional TCP-based listener that makes it possible for servers to
/// register themselves with a central location for easy lobby by clients.
/// </summary>

public class TcpLobbyServer : LobbyServer
{
	// List of servers that's currently being updated
	ServerList mList = new ServerList();
	long mTime = 0;
	long mLastChange = 0;
	List<TcpProtocol> mTcp = new List<TcpProtocol>();
	TcpListener mListener;
	int mPort = 0;
	Thread mThread;
	bool mInstantUpdates = true;
	Buffer mBuffer;

	/// <summary>
	/// If the number of simultaneous connected clients exceeds this number,
	/// server updates will no longer be instant, but rather delayed instead.
	/// </summary>

	public int instantUpdatesClientLimit = 50;

	/// <summary>
	/// Port used to listen for incoming packets.
	/// </summary>

	public override int port { get { return mPort; } }

	/// <summary>
	/// Whether the server is active.
	/// </summary>

	public override bool isActive { get { return (mListener != null); } }

	/// <summary>
	/// Start listening for incoming connections.
	/// </summary>

	public override bool Start (int listenPort)
	{
		Stop();

		try
		{
			mListener = new TcpListener(IPAddress.Any, listenPort);
			mListener.Start(50);
			mPort = listenPort;
		}
#if STANDALONE
		catch (System.Exception ex)
		{
			Console.WriteLine("ERROR: " + ex.Message);
			return false;
		}
		Console.WriteLine("TCP Lobby Server started on port " + listenPort);
#else
		catch (System.Exception) { return false; }
#endif
		mThread = new Thread(ThreadFunction);
		mThread.Start();
		return true;
	}

	/// <summary>
	/// Stop listening for incoming packets.
	/// </summary>

	public override void Stop ()
	{
		if (mThread != null)
		{
			mThread.Abort();
			mThread = null;
		}

		if (mListener != null)
		{
			mListener.Stop();
			mListener = null;
		}
		mList.Clear();
	}

	/// <summary>
	/// Start the sending process.
	/// </summary>

	BinaryWriter BeginSend (Packet type)
	{
		mBuffer = Buffer.Create();
		BinaryWriter writer = mBuffer.BeginPacket(type);
		return writer;
	}

	/// <summary>
	/// Send the outgoing buffer to the specified player.
	/// </summary>

	void EndSend (TcpProtocol tc)
	{
		mBuffer.EndPacket();
		tc.SendTcpPacket(mBuffer);
		mBuffer.Recycle();
		mBuffer = null;
	}

	/// <summary>
	/// Thread that will be processing incoming data.
	/// </summary>

	void ThreadFunction ()
	{
		for (; ; )
		{
			mTime = DateTime.UtcNow.Ticks / 10000;

			// Accept incoming connections
			while (mListener != null && mListener.Pending())
			{
				TcpProtocol tc = new TcpProtocol();
				tc.StartReceiving(mListener.AcceptSocket());
				mTcp.Add(tc);
			}

			Buffer buffer = null;

			// Process incoming TCP packets
			for (int i = 0; i < mTcp.size; ++i)
			{
				TcpProtocol tc = mTcp[i];

				while (tc.ReceivePacket(out buffer))
				{
					try
					{
						if (!ProcessPacket(buffer, tc))
						{
							RemoveServer(tc);
							tc.Disconnect();
						}
					}
#if STANDALONE
					catch (System.Exception ex)
					{
						Console.WriteLine("ERROR: " + ex.Message);
						RemoveServer(tc);
						tc.Disconnect();
					}
#else
					catch (System.Exception)
					{
						RemoveServer(tc);
						tc.Disconnect();
					}
#endif
					if (buffer != null)
					{
						buffer.Recycle();
						buffer = null;
					}
				}
			}

			// We only want to send instant updates if the number of players is under a specific threshold
			if (mTcp.size > instantUpdatesClientLimit) mInstantUpdates = false;

			// Send the server list to all connected clients
			for (int i = 0; i < mTcp.size; ++i)
			{
				TcpProtocol tc = mTcp[i];
				if (tc.stage != TcpProtocol.Stage.Connected || tc.data == null || !(tc.data is long)) continue;

				long customTimestamp = (long)tc.data;

				// If timestamp was set then the list was already sent previously
				if (customTimestamp != 0)
				{
					// List hasn't changed -- do nothing
					if (customTimestamp >= mLastChange) continue;
					
					// Too many clients: we want the updates to be infrequent
					if (!mInstantUpdates && customTimestamp + 4000 > mTime) continue;
				}

				// Create the server list packet
				if (buffer == null)
				{
					buffer = Buffer.Create();
					BinaryWriter writer = buffer.BeginPacket(Packet.ResponseServerList);
					mList.WriteTo(writer);
					buffer.EndPacket();
				}
				tc.SendTcpPacket(buffer);
				tc.data = mTime;
			}

			if (buffer != null)
			{
				buffer.Recycle();
				buffer = null;
			}
			Thread.Sleep(1);
		}
	}

	/// <summary>
	/// Process an incoming packet.
	/// </summary>

	bool ProcessPacket (Buffer buffer, TcpProtocol tc)
	{
		// TCP connections must be verified first to ensure that they are using the correct protocol
		if (tc.stage == TcpProtocol.Stage.Verifying)
		{
			if (tc.VerifyRequestID(buffer, false)) return true;
#if STANDALONE
			Console.WriteLine(tc.address + " has failed the verification step");
#endif
			return false;
		}

		BinaryReader reader = buffer.BeginReading();
		Packet request = (Packet)reader.ReadByte();

		switch (request)
		{
			case Packet.RequestPing:
			{
				BeginSend(Packet.ResponsePing);
				EndSend(tc);
				break;
			}
			case Packet.RequestServerList:
			{
				if (reader.ReadUInt16() != GameServer.gameID) return false;
				tc.data = (long)0;
				return true;
			}
			case Packet.RequestAddServer:
			{
				if (reader.ReadUInt16() != GameServer.gameID) return false;
				ServerList.Entry ent = new ServerList.Entry();
				ent.ReadFrom(reader);

				if (ent.externalAddress.Address.Equals(IPAddress.None))
					ent.externalAddress = tc.tcpEndPoint;

				mList.Add(ent, mTime).data = tc;
				mLastChange = mTime;
#if STANDALONE
				Console.WriteLine(tc.address + " added a server (" + ent.internalAddress + ", " + ent.externalAddress + ")");
#endif
				return true;
			}
			case Packet.RequestRemoveServer:
			{
				if (reader.ReadUInt16() != GameServer.gameID) return false;
				IPEndPoint internalAddress, externalAddress;
				Tools.Serialize(reader, out internalAddress);
				Tools.Serialize(reader, out externalAddress);

				if (externalAddress.Address.Equals(IPAddress.None))
					externalAddress = tc.tcpEndPoint;

				RemoveServer(internalAddress, externalAddress);
#if STANDALONE
				Console.WriteLine(tc.address + " removed a server (" + internalAddress + ", " + externalAddress + ")");
#endif
				return true;
			}
			case Packet.Disconnect:
			{
#if STANDALONE
				if (RemoveServer(tc)) Console.WriteLine(tc.address + " has disconnected");
#else
				RemoveServer(tc);
#endif
				mTcp.Remove(tc);
				return true;
			}
			case Packet.RequestSaveFile:
			{
				string fileName = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadInt32());
				SaveFile(fileName, data);
				break;
			}
			case Packet.RequestLoadFile:
			{
				string fn = reader.ReadString();
				byte[] data = LoadFile(fn);

				BinaryWriter writer = BeginSend(Packet.ResponseLoadFile);
				writer.Write(fn);

				if (data != null)
				{
					writer.Write(data.Length);
					writer.Write(data);
				}
				else writer.Write(0);
				EndSend(tc);
				break;
			}
			case Packet.RequestDeleteFile:
			{
				DeleteFile(reader.ReadString());
				break;
			}
			case Packet.Error:
			{
#if STANDALONE
				Console.WriteLine(tc.address + " error: " + reader.ReadString());
#endif
				return false;
			}
		}
#if STANDALONE
		Console.WriteLine(tc.address + " sent a packet not handled by the lobby server: " + request);
#endif
		return false;
	}

	/// <summary>
	/// Remove all entries added by the specified client.
	/// </summary>

	bool RemoveServer (Player player)
	{
		bool changed = false;

		lock (mList.list)
		{
			for (int i = mList.list.size; i > 0; )
			{
				ServerList.Entry ent = mList.list[--i];

				if (ent.data == player)
				{
					mList.list.RemoveAt(i);
					mLastChange = mTime;
					changed = true;
				}
			}
		}
		return changed;
	}

	/// <summary>
	/// Add a new server to the list.
	/// </summary>

	public override void AddServer (string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress)
	{
		mList.Add(name, playerCount, internalAddress, externalAddress, mTime);
		mLastChange = mTime;
	}

	/// <summary>
	/// Remove an existing server from the list.
	/// </summary>

	public override void RemoveServer (IPEndPoint internalAddress, IPEndPoint externalAddress)
	{
		if (mList.Remove(internalAddress, externalAddress))
			mLastChange = mTime;
	}
}
}
