//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System.Net;
namespace TNet
{
/// <summary>
/// Abstract class for a lobby server.
/// </summary>

public abstract class LobbyServer : FileServer
{
	/// <summary>
	/// Port used to listen for incoming packets.
	/// </summary>

	public abstract int port { get; }

	/// <summary>
	/// Whether the server is active.
	/// </summary>

	public abstract bool isActive { get; }

	/// <summary>
	/// Start listening for incoming server list requests.
	/// </summary>

	public abstract bool Start (int listenPort);

	/// <summary>
	/// Stop listening for incoming packets.
	/// </summary>

	public abstract void Stop ();

	/// <summary>
	/// Add a new server to the list.
	/// </summary>

	public abstract void AddServer (string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress);

	/// <summary>
	/// Remove an existing server from the list.
	/// </summary>

	public abstract void RemoveServer (IPEndPoint internalAddress, IPEndPoint externalAddress);
}
}
