//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System.Net;
using System.IO;
using System.Collections;
using System.Threading;
using UnityEngine;
using TNet;

/// <summary>
/// UDP-based lobby client, designed to communicate with the UdpLobbyServer.
/// </summary>

public class TNUdpLobbyClient : TNLobbyClient
{
	UdpProtocol mUdp = new UdpProtocol();
	Buffer mRequest;
	long mNextSend = 0;
	IPEndPoint mRemoteAddress;
	bool mReEnable = false;

	void Awake ()
	{
#if UNITY_FLASH || UNITY_WEBPLAYER
#if UNITY_EDITOR
		Debug.LogWarning("UDP is not supported on Flash and Web Player platforms. UDP-based server discovery will not function.", this);
#endif
		enabled = false;
#endif
	}

	void OnEnable()
	{
		if (mRequest == null)
		{
			mRequest = Buffer.Create();
			mRequest.BeginPacket(Packet.RequestServerList).Write(GameServer.gameID);
			mRequest.EndPacket();
		}

		if (mRemoteAddress == null)
		{
			mRemoteAddress = string.IsNullOrEmpty(remoteAddress) ?
				new IPEndPoint(IPAddress.Broadcast, remotePort) :
				Tools.ResolveEndPoint(remoteAddress, remotePort);

			if (mRemoteAddress == null)
				mUdp.Error(new IPEndPoint(IPAddress.Loopback, mUdp.listeningPort),
					"Invalid address: " + remoteAddress + ":" + remotePort);
		}

		// Twice just in case the first try falls on a taken port
		if (!mUdp.Start(Tools.randomPort)) mUdp.Start(Tools.randomPort);
	}

	protected override void OnDisable ()
	{
		isActive = false;
		base.OnDisable();

		try
		{
			mUdp.Stop();

			if (mRequest != null)
			{
				mRequest.Recycle();
				mRequest = null;
			}
			if (onChange != null) onChange();
		}
		catch (System.Exception) { }
	}

	void OnApplicationPause (bool paused)
	{
		if (paused)
		{
			if (isActive)
			{
				mReEnable = true;
				OnDisable();
			}
		}
		else if (mReEnable)
		{
			mReEnable = false;
			OnEnable();
		}
	}

	/// <summary>
	/// Keep receiving incoming packets.
	/// </summary>

	void Update ()
	{
		Buffer buffer;
		IPEndPoint ip;
		bool changed = false;
		long time = System.DateTime.UtcNow.Ticks / 10000;

		// Receive and process UDP packets one at a time
		while (mUdp != null && mUdp.ReceivePacket(out buffer, out ip))
		{
			if (buffer.size > 0)
			{
				try
				{
					BinaryReader reader = buffer.BeginReading();
					Packet response = (Packet)reader.ReadByte();

					if (response == Packet.ResponseServerList)
					{
						isActive = true;
						mNextSend = time + 3000;
						knownServers.ReadFrom(reader, time);
						knownServers.Cleanup(time);
						changed = true;
					}
					else if (response == Packet.Error)
					{
						errorString = reader.ReadString();
						Debug.LogWarning(errorString);
						changed = true;
					}
				}
				catch (System.Exception) { }
			}
			buffer.Recycle();
		}

		// Clean up old servers
		if (knownServers.Cleanup(time))
			changed = true;

		// Trigger the listener callback
		if (changed && onChange != null)
		{
			onChange();
		}
		else if (mNextSend < time && mUdp != null)
		{
			// Send out the update request
			mNextSend = time + 3000;
			mUdp.Send(mRequest, mRemoteAddress);
		}
	}
}
