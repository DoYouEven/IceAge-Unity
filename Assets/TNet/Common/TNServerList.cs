//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.Net;
using System.IO;

namespace TNet
{
/// <summary>
/// Server list is a helper class containing a list of servers.
/// </summary>

public class ServerList
{
	public class Entry
	{
		public string name;
		public int playerCount;
		public IPEndPoint internalAddress;
		public IPEndPoint externalAddress;
		public long recordTime;
		public object data;

		public void WriteTo (BinaryWriter writer)
		{
			writer.Write(name);
			writer.Write((ushort)playerCount);
			Tools.Serialize(writer, internalAddress);
			Tools.Serialize(writer, externalAddress);
		}

		public void ReadFrom (BinaryReader reader)
		{
			name = reader.ReadString();
			playerCount = reader.ReadUInt16();
			Tools.Serialize(reader, out internalAddress);
			Tools.Serialize(reader, out externalAddress);
		}
	}

	/// <summary>
	/// List of active server entries. Be sure to lock it before using it,
	/// as it can be changed from a different thread.
	/// </summary>

	public List<Entry> list = new List<Entry>();

	/// <summary>
	/// Add a new entry to the list.
	/// </summary>

	public Entry Add (string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress, long time)
	{
		lock (list)
		{
			for (int i = 0; i < list.size; ++i)
			{
				Entry ent = list[i];

				if (ent.internalAddress.Equals(internalAddress) &&
					ent.externalAddress.Equals(externalAddress))
				{
					ent.name = name;
					ent.playerCount = playerCount;
					ent.recordTime = time;
					list[i] = ent;
					return ent;
				}
			}

			Entry e = new Entry();
			e.name = name;
			e.playerCount = playerCount;
			e.internalAddress = internalAddress;
			e.externalAddress = externalAddress;
			e.recordTime = time;
			list.Add(e);
			return e;
		}
	}

	/// <summary>
	/// Add a new entry.
	/// </summary>

	public Entry Add (Entry newEntry, long time)
	{
		lock (list)
		{
			for (int i = 0; i < list.size; ++i)
			{
				Entry ent = list[i];

				if (ent.internalAddress.Equals(newEntry.internalAddress) &&
					ent.externalAddress.Equals(newEntry.externalAddress))
				{
					ent.name = newEntry.name;
					ent.playerCount = newEntry.playerCount;
					ent.recordTime = time;
					return ent;
				}
			}
			newEntry.recordTime = time;
			list.Add(newEntry);
		}
		return newEntry;
	}

	/// <summary>
	/// Remove an existing entry from the list.
	/// </summary>

	public bool Remove (IPEndPoint internalAddress, IPEndPoint externalAddress)
	{
		lock (list)
		{
			for (int i = 0; i < list.size; ++i)
			{
				Entry ent = list[i];

				if (ent.internalAddress.Equals(internalAddress) &&
					ent.externalAddress.Equals(externalAddress))
				{
					list.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Remove expired entries.
	/// </summary>

	public bool Cleanup (long time)
	{
		time -= 7000;
		bool changed = false;

		lock (list)
		{
			for (int i = 0; i < list.size; )
			{
				Entry ent = list[i];

				if (ent.recordTime < time)
				{
					changed = true;
					list.RemoveAt(i);
					continue;
				}
				++i;
			}
		}
		return changed;
	}

	/// <summary>
	/// Clear the list of servers.
	/// </summary>

	public void Clear () { lock (list) list.Clear(); }

	/// <summary>
	/// Save the list of servers to the specified binary writer.
	/// </summary>

	public void WriteTo (BinaryWriter writer)
	{
		writer.Write(GameServer.gameID);

		lock (list)
		{
			writer.Write((ushort)list.size);
			for (int i = 0; i < list.size; ++i)
				list[i].WriteTo(writer);
		}
	}

	/// <summary>
	/// Read a list of servers from the binary reader.
	/// </summary>

	public void ReadFrom (BinaryReader reader, long time)
	{
		if (reader.ReadUInt16() == GameServer.gameID)
		{
			lock (list)
			{
				int count = reader.ReadUInt16();

				for (int i = 0; i < count; ++i)
				{
					Entry ent = new Entry();
					ent.ReadFrom(reader);
					AddInternal(ent, time);
				}
			}
		}
	}

	/// <summary>
	/// Add a new entry. Not thread-safe.
	/// </summary>

	void AddInternal (Entry newEntry, long time)
	{
		for (int i = 0; i < list.size; ++i)
		{
			Entry ent = list[i];

			if (ent.internalAddress.Equals(newEntry.internalAddress) &&
				ent.externalAddress.Equals(newEntry.externalAddress))
			{
				ent.name = newEntry.name;
				ent.playerCount = newEntry.playerCount;
				ent.recordTime = time;
				return;
			}
		}
		newEntry.recordTime = time;
		list.Add(newEntry);
	}
}
}
