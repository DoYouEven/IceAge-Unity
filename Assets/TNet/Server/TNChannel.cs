//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;

namespace TNet
{
/// <summary>
/// A channel contains one or more players.
/// All information broadcast by players is visible by others in the same channel.
/// </summary>

public class Channel
{
	public class RFC
	{
		// Object ID (24 bytes), RFC ID (8 bytes)
		public uint uid;
		public string functionName;
		public Buffer buffer;

		public uint objectID { get { return (uid >> 8); } }
		public uint functionID { get { return (uid & 0xFF); } }
	}

	public class CreatedObject
	{
		public int playerID;
		public ushort objectID;
		public uint uniqueID;
		public byte type;
		public Buffer buffer;
	}

	public int id;
	public string password = "";
	public string level = "";
	public string data = "";
	public bool persistent = false;
	public bool closed = false;
	public ushort playerLimit = 65535;
	public List<TcpPlayer> players = new List<TcpPlayer>();
	public List<RFC> rfcs = new List<RFC>();
	public List<CreatedObject> created = new List<CreatedObject>();
	public List<uint> destroyed = new List<uint>();
	public uint objectCounter = 0xFFFFFF;
	public TcpPlayer host;

	/// <summary>
	/// Whether the channel has data that can be saved.
	/// </summary>

	public bool hasData { get { return rfcs.size > 0 || created.size > 0 || destroyed.size > 0; } }

	/// <summary>
	/// Whether the channel can be joined.
	/// </summary>

	public bool isOpen { get { return !closed && players.size < playerLimit; } }

	/// <summary>
	/// Reset the channel to its initial state.
	/// </summary>

	public void Reset ()
	{
		for (int i = 0; i < rfcs.size; ++i) rfcs[i].buffer.Recycle();
		for (int i = 0; i < created.size; ++i) created[i].buffer.Recycle();

		rfcs.Clear();
		created.Clear();
		destroyed.Clear();
		objectCounter = 0xFFFFFF;
	}

	/// <summary>
	/// Remove the specified player from the channel.
	/// </summary>

	public void RemovePlayer (TcpPlayer p, List<uint> destroyedObjects)
	{
		destroyedObjects.Clear();

		if (players.Remove(p))
		{
			// When the host leaves, clear the host (it gets changed in SendLeaveChannel)
			if (p == host) host = null;

			// Remove all of the non-persistent objects that were created by this player
			for (int i = created.size; i > 0; )
			{
				Channel.CreatedObject obj = created[--i];

				if (obj.playerID == p.id)
				{
					if (obj.type == 2)
					{
						if (obj.buffer != null) obj.buffer.Recycle();
						created.RemoveAt(i);
						destroyedObjects.Add(obj.uniqueID);
						DestroyObjectRFCs(obj.uniqueID);
					}
					else if (players.size != 0)
					{
						// The same operation happens on the client as well
						obj.playerID = players[0].id;
					}
				}
			}

			// Close the channel if it wasn't persistent
			if ((!persistent || playerLimit < 1) && players.size == 0)
			{
				closed = true;

				for (int i = 0; i < rfcs.size; ++i)
				{
					RFC r = rfcs[i];
					if (r.buffer != null) r.buffer.Recycle();
				}
				rfcs.Clear();
			}
		}
	}

	/// <summary>
	/// Remove an object with the specified unique identifier.
	/// </summary>

	public bool DestroyObject (uint uniqueID)
	{
		if (!destroyed.Contains(uniqueID))
		{
			for (int i = 0; i < created.size; ++i)
			{
				Channel.CreatedObject obj = created[i];

				if (obj.uniqueID == uniqueID)
				{
					if (obj.buffer != null) obj.buffer.Recycle();
					created.RemoveAt(i);
					DestroyObjectRFCs(uniqueID);
					return true;
				}
			}
			destroyed.Add(uniqueID);
			DestroyObjectRFCs(uniqueID);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Delete the specified remote function call.
	/// </summary>

	public void DestroyObjectRFCs (uint objectID)
	{
		for (int i = 0; i < rfcs.size; )
		{
			RFC r = rfcs[i];

			if (r.objectID == objectID)
			{
				rfcs.RemoveAt(i);
				r.buffer.Recycle();
				continue;
			}
			++i;
		}
	}

	/// <summary>
	/// Create a new buffered remote function call.
	/// </summary>

	public void CreateRFC (uint inID, string funcName, Buffer buffer)
	{
		if (closed || buffer == null) return;
		buffer.MarkAsUsed();

		for (int i = 0; i < rfcs.size; ++i)
		{
			RFC r = rfcs[i];

			if (r.uid == inID && r.functionName == funcName)
			{
				if (r.buffer != null) r.buffer.Recycle();
				r.buffer = buffer;
				return;
			}
		}

		RFC rfc = new RFC();
		rfc.uid = inID;
		rfc.buffer = buffer;
		rfc.functionName = funcName;
		rfcs.Add(rfc);
	}

	/// <summary>
	/// Delete the specified remote function call.
	/// </summary>

	public void DeleteRFC (uint inID, string funcName)
	{
		for (int i = 0; i < rfcs.size; ++i)
		{
			RFC r = rfcs[i];

			if (r.uid == inID && r.functionName == funcName)
			{
				rfcs.RemoveAt(i);
				r.buffer.Recycle();
			}
		}
	}

	/// <summary>
	/// Save the channel's data into the specified file.
	/// </summary>

	public void SaveTo (BinaryWriter writer)
	{
		writer.Write(Player.version);
		writer.Write(level);
		writer.Write(data);
		writer.Write(objectCounter);
		writer.Write(password);
		writer.Write(persistent);
		writer.Write(playerLimit);

		List<uint> tempObjs = new List<uint>();
		List<CreatedObject> cleanedObjs = new List<CreatedObject>();
		List<RFC> cleanedRFCs = new List<RFC>();

		// Record which objects are temporary and which ones are not
		for (int i = 0; i < created.size; ++i)
		{
			CreatedObject co = created[i];
			if (co.type == 1) cleanedObjs.Add(co);
			else tempObjs.Add(co.uniqueID);
		}

		// Record all RFCs that don't belong to temporary objects
		for (int i = 0; i < rfcs.size; ++i)
		{
			RFC rfc = rfcs[i];
			if (!tempObjs.Contains(rfc.objectID))
				cleanedRFCs.Add(rfc);
		}

		writer.Write(cleanedRFCs.size);

		for (int i = 0; i < cleanedRFCs.size; ++i)
		{
			RFC rfc = cleanedRFCs[i];
			writer.Write(rfc.uid);
			if (rfc.functionID == 0) writer.Write(rfc.functionName);
			writer.Write(rfc.buffer.size);

			if (rfc.buffer.size > 0)
			{
				rfc.buffer.BeginReading();
				writer.Write(rfc.buffer.buffer, rfc.buffer.position, rfc.buffer.size);
			}
		}

		writer.Write(cleanedObjs.size);

		for (int i = 0; i < cleanedObjs.size; ++i)
		{
			CreatedObject co = cleanedObjs[i];
			writer.Write(co.playerID);
			writer.Write(co.uniqueID);
			writer.Write(co.objectID);
			writer.Write(co.buffer.size);

			if (co.buffer.size > 0)
			{
				co.buffer.BeginReading();
				writer.Write(co.buffer.buffer, co.buffer.position, co.buffer.size);
			}
		}

		writer.Write(destroyed.size);
		for (int i = 0; i < destroyed.size; ++i) writer.Write(destroyed[i]);
	}

	/// <summary>
	/// Load the channel's data from the specified file.
	/// </summary>

	public bool LoadFrom (BinaryReader reader)
	{
		int version = reader.ReadInt32();
		if (version != Player.version) return false;

		// Clear all RFCs, just in case
		for (int i = 0; i < rfcs.size; ++i)
		{
			RFC r = rfcs[i];
			if (r.buffer != null) r.buffer.Recycle();
		}
		rfcs.Clear();
		created.Clear();
		destroyed.Clear();

		level = reader.ReadString();
		data = reader.ReadString();
		objectCounter = reader.ReadUInt32();
		password = reader.ReadString();
		persistent = reader.ReadBoolean();
		playerLimit = reader.ReadUInt16();

		int size = reader.ReadInt32();

		for (int i = 0; i < size; ++i)
		{
			RFC rfc = new RFC();
			rfc.uid = reader.ReadUInt32();
			if (rfc.functionID == 0) rfc.functionName = reader.ReadString();
			Buffer b = Buffer.Create();
			b.BeginWriting(false).Write(reader.ReadBytes(reader.ReadInt32()));
			rfc.buffer = b;
			rfcs.Add(rfc);
		}

		size = reader.ReadInt32();

		for (int i = 0; i < size; ++i)
		{
			CreatedObject co = new CreatedObject();
			co.playerID = reader.ReadInt32();
			co.uniqueID = reader.ReadUInt32();
			co.objectID = reader.ReadUInt16();
			co.type = 1;
			Buffer b = Buffer.Create();
			b.BeginWriting(false).Write(reader.ReadBytes(reader.ReadInt32()));
			b.BeginReading();
			co.buffer = b;
			created.Add(co);
		}

		size = reader.ReadInt32();
		for (int i = 0; i < size; ++i) destroyed.Add(reader.ReadUInt32());
		return true;
	}
}
}
