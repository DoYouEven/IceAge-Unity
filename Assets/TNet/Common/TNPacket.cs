//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

namespace TNet
{
/// <summary>
/// Clients send requests to the server and receive responses back. Forwarded calls arrive as-is.
/// </summary>

public enum Packet
{
	/// <summary>
	/// Empty packet. Can be used to keep the connection alive.
	/// </summary>

	Empty,

	/// <summary>
	/// This packet indicates that an error has occurred.
	/// string: Description of the error.
	/// </summary>

	Error,

	/// <summary>
	/// This packet indicates that the connection should be severed.
	/// </summary>

	Disconnect,

	//===================================================================================

	/// <summary>
	/// This should be the very first packet sent by the client.
	/// int32: Protocol version.
	/// string: Player Name.
	/// object: Player data.
	/// </summary>

	RequestID,

	/// <summary>
	/// Clients should send a ping request periodically.
	/// </summary>

	RequestPing,

	/// <summary>
	/// Set the remote UDP port for unreliable packets.
	/// ushort: port.
	/// </summary>

	RequestSetUDP,

	/// <summary>
	/// Join the specified channel.
	/// int32: Channel ID (-1 = new random, -2 = existing random)
	/// string: Channel password.
	/// bool: Whether the channel should be persistent (left open even when the last player leaves).
	/// ushort: Player limit.
	/// </summary>

	RequestJoinChannel,

	/// <summary>
	/// Leave the channel the player is in.
	/// </summary>

	RequestLeaveChannel,

	/// <summary>
	/// Mark the channel as closed. No further players will be able to join and saved data will be deleted.
	/// </summary>

	RequestCloseChannel,

	/// <summary>
	/// Change the number of players that can be in this channel at the same time.
	/// ushort: Player limit.
	/// </summary>

	RequestSetPlayerLimit,

	/// <summary>
	/// Load the specified level.
	/// string: Level Name.
	/// </summary>

	RequestLoadLevel,

	/// <summary>
	/// Player name change.
	/// string: Player name.
	/// </summary>

	RequestSetName,

	/// <summary>
	/// Transfer the host status to the specified player. Only works if the sender is currently hosting.
	/// int32: Player ID.
	/// </summary>

	RequestSetHost,

	/// <summary>
	/// Delete the specified buffered function.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// string: Function Name (only if RFC ID is 0).
	/// </summary>

	RequestRemoveRFC,

	/// <summary>
	/// Instantiate a new object with the specified identifier.
	/// ushort: Index of the object being created (within a static list of prefabs on the client).
	/// byte:
	/// 0 = Local-only object. Only echoed to other clients.
	/// 1 = Saved on the server, assigned a new owner when the existing owner leaves.
	/// 2 = Saved on the server, destroyed when the owner leaves.
	/// Arbitrary amount of data follows. All of it will be passed along with the response call.
	/// </summary>

	RequestCreate,

	/// <summary>
	/// Delete the specified Network Object.
	/// uint32: Object ID.
	/// </summary>

	RequestDestroy,

	/// <summary>
	/// Save the specified data.
	/// string: Filename.
	/// int32: Size of the data in bytes.
	/// Arbitrary amount of data follows.
	/// </summary>

	RequestSaveFile,

	/// <summary>
	/// Load the requested data that was saved previously.
	/// string: Filename.
	/// </summary>

	RequestLoadFile,

	/// <summary>
	/// Delete the specified file.
	/// string: Filename.
	/// </summary>

	RequestDeleteFile,

	/// <summary>
	/// Improve latency of the established connection at the expense of network traffic.
	/// bool: Whether to improve it (enable NO_DELAY)
	/// </summary>

	RequestNoDelay,

	/// <summary>
	/// Set the channel's data field.
	/// string: Custom data
	/// </summary>

	RequestSetChannelData,

	/// <summary>
	/// Request the list of open channels from the server.
	/// </summary>
	
	RequestChannelList,

	//===================================================================================

	/// <summary>
	/// Always the first packet to arrive from the server.
	/// If the protocol version didn't match the client, a disconnect may follow.
	/// int32: Protocol ID.
	/// int32: Player ID (only if the protocol ID matched).
	/// int64: Server time in milliseconds (only if the protocol ID matched).
	/// </summary>

	ResponseID,

	/// <summary>
	/// Response to a ping request.
	/// </summary>

	ResponsePing,

	/// <summary>
	/// Set a UDP port used for communication.
	/// ushort: port. (0 means disabled)
	/// </summary>

	ResponseSetUDP,

	/// <summary>
	/// Inform everyone of this player leaving the channel.
	/// int32: Player ID.
	/// </summary>

	ResponsePlayerLeft,

	/// <summary>
	/// Inform the channel that a new player has joined.
	/// 
	/// Parameters:
	/// int32: Player ID,
	/// string: Player name.
	/// object: Player data.
	/// </summary>

	ResponsePlayerJoined,

	/// <summary>
	/// Start of the channel joining process. Sent to the player who is joining the channel.
	/// 
	/// Parameters:
	/// int32: Channel ID,
	/// int16: Number of players.
	/// 
	/// Then for each player:
	/// int32: Player ID,
	/// string: Player Name.
	/// object: Player data.
	/// </summary>

	ResponseJoiningChannel,

	/// <summary>
	/// Inform the player that they have successfully joined a channel.
	/// bool: Success or failure.
	/// string: Error string (if failed).
	/// </summary>

	ResponseJoinChannel,

	/// <summary>
	/// Inform the player that they have left the channel they were in.
	/// </summary>

	ResponseLeaveChannel,

	/// <summary>
	/// Change the specified player's name.
	/// int32: Player ID,
	/// string: Player name.
	/// </summary>

	ResponseRenamePlayer,

	/// <summary>
	/// Inform the player of who is hosting.
	/// int32: Player ID.
	/// </summary>

	ResponseSetHost,

	/// <summary>
	/// Load the specified level. Should happen before all buffered calls.
	/// string: Name of the level.
	/// </summary>

	ResponseLoadLevel,

	/// <summary>
	/// Create a new persistent entry.
	/// int: ID of the player that requested this object to be created.
	/// ushort: Index of the object being created (within a static list of prefabs on the client).
	/// uint32: Unique Identifier (aka Object ID) if requested, 0 otherwise. 0-16777215 range.
	/// Arbitrary amount of data follows, same data that was passed along with the Create Request.
	/// </summary>

	ResponseCreate,

	/// <summary>
	/// Delete the specified Unique Identifier and its associated entry.
	/// ushort: Number of objects that will follow.
	/// uint32[] Unique Identifiers (aka Object IDs).
	/// </summary>

	ResponseDestroy,

	/// <summary>
	/// Loaded file response.
	/// string: Filename.
	/// int32: Number of bytes to follow.
	/// byte[]: Data.
	/// </summary>

	ResponseLoadFile,

	/// <summary>
	/// The channel's data has been changed.
	/// string: Custom data
	/// </summary>

	ResponseSetChannelData,

	/// <summary>
	/// List open channels on the server.
	/// int32: number of channels to follow
	/// For each channel:
	///   int32: ID
	///   ushort: Number of players
	///   ushort: Player limit
	///   bool: Has a password
	///   bool: Is persistent
	///   string: Level
	///   string: Custom data
	/// </summary>

	ResponseChannelList,

	//===================================================================================

	/// <summary>
	/// Echo the packet to everyone in the room. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToAll,

	/// <summary>
	/// Echo the packet to everyone in the room and everyone who joins later.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToAllSaved,

	/// <summary>
	/// Echo the packet to everyone in the room except the sender. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToOthers,

	/// <summary>
	/// Echo the packet to everyone in the room (except the sender) and everyone who joins later.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToOthersSaved,

	/// <summary>
	/// Echo the packet to the room's host. Interpreting the packet is up to the client.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToHost,

	/// <summary>
	/// Echo the packet to the specified player.
	/// int32: Player ID
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToPlayer,

	/// <summary>
	/// Echo the packet to the specified player and everyone who joins later.
	/// int32: Player ID
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardToPlayerBuffered,

	//===================================================================================

	/// <summary>
	/// Add a new entry to the list of known servers. Used by the Lobby Server.
	/// ushort: Game ID.
	/// string: Server name.
	/// ushort: Number of connected players.
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	RequestAddServer,

	/// <summary>
	/// Remove an existing server list entry. Used by the Lobby Server.
	/// ushort: Game ID.
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	RequestRemoveServer,

	/// <summary>
	/// Request a list of all known servers for the specified game ID. Used by the Lobby Server.
	/// ushort: Game ID.
	/// </summary>

	RequestServerList,

	/// <summary>
	/// Response sent by the Lobby Server, listing servers.
	/// ushort: List size
	/// For each entry:
	/// string: Server name
	/// ushort: Player count
	/// IPEndPoint: Internal address
	/// IPEndPoint: External address
	/// </summary>

	ResponseServerList,

	/// <summary>
	/// By default, the player gets disconnected after 10 seconds of inactivity. You can change this on a per-player basis.
	/// Setting this value to '0' will turn off this functionality altogether -- however it's a good idea to keep it at some
	/// valid non-zero value. If you know the player is going to be loading a level for up to a minute, set it to 2 minutes (120).
	/// int: timeout delay in seconds
	/// </summary>

	RequestSetTimeout,

	//===================================================================================

	/// <summary>
	/// Echo this message to everyone connected to the server.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	Broadcast,

	/// <summary>
	/// Activate UDP functionality on the server for this client. This must be sent via UDP and it has no response.
	/// int: Player ID.
	/// </summary>

	RequestActivateUDP,

	/// <summary>
	/// Sync the specified player's 'data' property. This packet will be echoed to everyone except the sender.
	/// int32: Player ID who's data should be synchronized.
	/// object: Player's data.
	/// </summary>

	SyncPlayerData,

	/// <summary>
	/// Echo the packet to the specified player.
	/// string: Player name.
	/// uint32: Object ID (24 bits), RFC ID (8 bits).
	/// Arbitrary amount of data follows.
	/// </summary>

	ForwardByName,

	/// <summary>
	/// Server notification sent when the target requested by ForwardByName was not found.
	/// string: Player name.
	/// </summary>

	ForwardTargetNotFound,
}
}
