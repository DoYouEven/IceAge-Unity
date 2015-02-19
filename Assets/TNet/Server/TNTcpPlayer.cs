//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace TNet
{
/// <summary>
/// Class containing information about connected players.
/// </summary>

public class TcpPlayer : TcpProtocol
{
	/// <summary>
	/// Channel that the player is currently in.
	/// </summary>

	public Channel channel;

	/// <summary>
	/// UDP end point if the player has one open.
	/// </summary>

	public IPEndPoint udpEndPoint;

	/// <summary>
	/// Whether the UDP has been confirmed as active and usable.
	/// </summary>

	public bool udpIsUsable = false;

	/// <summary>
	/// Channel joining process involves multiple steps. It's faster to perform them all at once.
	/// </summary>

	public void FinishJoiningChannel ()
	{
		Buffer buffer = Buffer.Create();

		// Step 2: Tell the player who else is in the channel
		BinaryWriter writer = buffer.BeginPacket(Packet.ResponseJoiningChannel);
		{
			writer.Write(channel.id);
			writer.Write((short)channel.players.size);

			for (int i = 0; i < channel.players.size; ++i)
			{
				TcpPlayer tp = channel.players[i];
				writer.Write(tp.id);
				writer.Write(string.IsNullOrEmpty(tp.name) ? "Guest" : tp.name);
#if STANDALONE
				if (tp.data == null) writer.Write((byte)0);
				else writer.Write((byte[])tp.data);
#else
				writer.WriteObject(tp.data);
#endif
			}
		}

		// End the first packet, but remember where it ended
		int offset = buffer.EndPacket();

		// Step 3: Inform the player of who is hosting
		if (channel.host == null) channel.host = this;
		buffer.BeginPacket(Packet.ResponseSetHost, offset);
		writer.Write(channel.host.id);
		offset = buffer.EndTcpPacketStartingAt(offset);

		// Step 4: Send the channel's data
		if (!string.IsNullOrEmpty(channel.data))
		{
			buffer.BeginPacket(Packet.ResponseSetChannelData, offset);
			writer.Write(channel.data);
			offset = buffer.EndTcpPacketStartingAt(offset);
		}

		// Step 5: Inform the player of what level we're on
		buffer.BeginPacket(Packet.ResponseLoadLevel, offset);
		writer.Write(string.IsNullOrEmpty(channel.level) ? "" : channel.level);
		offset = buffer.EndTcpPacketStartingAt(offset);

		// Step 6: Send the list of objects that have been created
		for (int i = 0; i < channel.created.size; ++i)
		{
			Channel.CreatedObject obj = channel.created.buffer[i];

			bool isPresent = false;

			for (int b = 0; b < channel.players.size; ++b)
			{
				if (channel.players[b].id == obj.playerID)
				{
					isPresent = true;
					break;
				}
			}

			// If the previous owner is not present, transfer ownership to the host
			if (!isPresent) obj.playerID = channel.host.id;

			buffer.BeginPacket(Packet.ResponseCreate, offset);
			writer.Write(obj.playerID);
			writer.Write(obj.objectID);
			writer.Write(obj.uniqueID);
			writer.Write(obj.buffer.buffer, obj.buffer.position, obj.buffer.size);
			offset = buffer.EndTcpPacketStartingAt(offset);
		}

		// Step 7: Send the list of objects that have been destroyed
		if (channel.destroyed.size != 0)
		{
			buffer.BeginPacket(Packet.ResponseDestroy, offset);
			writer.Write((ushort)channel.destroyed.size);
			for (int i = 0; i < channel.destroyed.size; ++i)
				writer.Write(channel.destroyed.buffer[i]);
			offset = buffer.EndTcpPacketStartingAt(offset);
		}

		// Step 8: Send all buffered RFCs to the new player
		for (int i = 0; i < channel.rfcs.size; ++i)
		{
			Buffer rfcBuff = channel.rfcs[i].buffer;
			rfcBuff.BeginReading();
			buffer.BeginWriting(offset);
			writer.Write(rfcBuff.buffer, 0, rfcBuff.size);
			offset = buffer.EndWriting();
		}

		// Step 9: The join process is now complete
		buffer.BeginPacket(Packet.ResponseJoinChannel, offset);
		writer.Write(true);
		offset = buffer.EndTcpPacketStartingAt(offset);

		// Send the entire buffer
		SendTcpPacket(buffer);
		buffer.Recycle();
	}
}
}
