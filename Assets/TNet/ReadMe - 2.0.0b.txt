-----------------------------------------------------
        TNet: Tasharen Networking Framework
    Copyright © 2012-2014 Tasharen Entertainment
                  Version 2.0.0b
       http://www.tasharen.com/?page_id=4518
-----------------------------------------------------

Thank you for buying TNet!

If you have any questions, suggestions, comments or feature requests, please
drop by the NGUI forum, found here: http://www.tasharen.com/forum/index.php

Full class documentation can be found here: http://www.tasharen.com/tnet/docs/

-----------------------------------------------------
  Basic Usage
-----------------------------------------------------

Q: How to start and stop a server from in-game?

TNServerInstance.Start(tcpPort, udpPort, [fileToLoad]);
TNServerInstance.Stop([fileToSave]]);

Q: How to connect/disconnect?

TNManager.Connect(address);
TNManager.Disconnect();

Q: How to join/leave a channel?

TNManager.JoinChannel(id, levelToLoad);
TNManager.LeaveChannel();

Q: How to instantiate new objects and then destroy them?

TNManager.Create(gameObject, position, rotation);
TNManager.Destroy(gameObject);

Q: How to send a remote function call?

TNObject tno = GetComponent<TNObject>(); // You can skip this line if you derived your script from TNBehaviour
tno.Send("FunctionName", target, <parameters>);

Q: What built-in notifications are there?

OnNetworkConnect (success, error);
OnNetworkDisconnect()
OnNetworkJoinChannel (success, error)
OnNetworkLeaveChannel()
OnNetworkPlayerJoin (player)
OnNetworkPlayerLeave (player)
OnNetworkPlayerRenamed (player, previousName)
OnNetworkError (error)

-----------------------------------------------------
  Stand-Alone Server
-----------------------------------------------------

You can build a stand-alone server by extracting the contents of the "TNetServer.zip" file
into the project's root folder (outside the Assets folder), then opening up the TNServer
solution or csproj file. A pre-compiled stand-alone windows executable is also included
in the ZIP file for your convenience.

-----------------------------------------------------
  More information:
-----------------------------------------------------

http://www.tasharen.com/?page_id=4518

-----------------------------------------------------
 Version History
-----------------------------------------------------

2.0.0
- NEW: Added the ability to send messages players by name rather than ID (think private chat messages).
- NEW: Saved/loaded files should now be kept in a dictionary for faster lookup.
- FIX: TNSerializer's WriteInt didn't work for negative values.
- FIX: Custom RCCs didn't seem to work quite right on the stand-alone server.
- FIX: More tweaks regarding object ownership transfer.

1.9.9
- NEW: TNManager.serverTime (in milliseconds)
- NEW: Added automatic serialization support for long, ulong, long[] and ulong[] types.
- NEW: TNObjects now have a DestroySelf function which TNBehaviours call that ensures the object's destruction.
- FIX: tno.isMine was not set properly for new players after the original owner left.
- DEL: Removed the setter from TNObject.ownerID, as it's handled properly on the server now.

1.9.8
- NEW: TNBehaviour's DestroySelf() function is now virtual.
- NEW: TNManager.onPlayerSync and TNManager.SyncPlayerData().
- NEW: String arrays are now serialized more efficiently within the DataNode.
- NEW: TNSyncRigidbody's updatesPerSecond is now a float so you can have 1 update per X seconds.
- NEW: TNManager.isJoiningChannel, set to 'true' between JoinChannel and OnNetworkJoinChannel.
- NEW: TNet's server instance can now be single-threaded for easier debugging in Unity.
- NEW: You can now pass TNObjects as RFC parameters.
- FIX: It's now possible to save the server properly even while it's running.
- FIX: TNet will no longer save non-persistent game objects when saved to disk.
- FIX: Int values can now be auto-converted to floats.
- FIX: Quite a few DataNode serialization changes/fixes.

1.9.7
- NEW: TNet is now fully Unity 5-compliant.
- NEW: SendRFC sent to the player's self will now result in immediate execution (think Target.Host).
- NEW: Added better/more informative error messages when RFCs or RCCs fail.
- NEW: TNObject inspector will now show its player owner and whether the player owns this object (at run time).
- FIX: TNManager.JoinChannel will now load the level even without TNManager.
- FIX: TNObjects will now have valid IDs even without TNManager.
- FIX: Added a null check in PrintException for when working with static RCC functions.
- FIX: OnNetworkDisconnect will now be called when the connection is shut down in a non-graceful manner.
- FIX: DataNode should have been clearing the child list after resolving custom data types.

1.9.6
- NEW: TNet will now use UDP multicasting instead of broadcasting by default.
- NEW: Added convenience methods to retrieve player data in DataNode form.
- NEW: Faster way of getting the external IP address.
- NEW: Example menu now shows your internal and external IPs.
- NEW: TNet.Tools.ResolveIPs can now be called by itself with no callback.
- NEW: TNet.UdpProtocol will now choose the default network interface on its own.
- FIX: LAN server list is now no longer cleared every time a new server arrives.
- FIX: Read/write/delete file functions are now wrapped in try/catch blocks.
- FIX: Fixed the TCP lobby server (it was throwing exceptions).
- FIX: Fixed the ability to host a local TCP-based lobby server alongside the game server.
- FIX: Added Ping packet handling to the lobby servers.

1.9.5
- NEW: TNManager.SaveFile, TNManager.LoadFile, TNManager.Ping.
- NEW: TNManager.playerData, synchronized across the network. SyncPlayerData() to sync it if modified via TNManager.playerDataNode.
- NEW: Added DataNode.Read(byte[] data, bool binary) for creating a data node tree from raw data.
- NEW: Added OnPing, OnPlayerSync, and OnLoadFile notifications to the Game Client.
- NEW: Custom packet handlers will now be checked first, in case you want to overwrite the default handling.
- NEW: TNServerInstance.SaveTo can now be used to save the server's state manually.
- FIX: Variety of serialization-related fixes and additions.
- FIX: Better error handling when connecting and better error messages.

1.9.1
- FIX: Error about TNObjects with ID of 0 will now only show up when playing the game.
- FIX: If an RFC cannot be executed, the error message will explain why.

1.9.0
- NEW: TNManager no longer needs to be present in the scene for you to use TNet.
- NEW: You can now send just about any type of data across the network via RFCs, not just specific types.
- NEW: Added custom serialization functionality to send custom classes via RFCs more efficiently.
- NEW: Added a DataNode tree-like data structure that's capable of serializing both to binary and to text format.
- NEW: AutoSync can now be set to only sync when new players join.
- NEW: Added support for multiple network interfaces (Hamachi etc).
- NEW: Added a bunch of serialization extension methods to BinaryWriter.
- NEW: TNet will now show the inner exception when an RFC fails.
- FIX: Better handling of mis-matched protocol IDs.

1.8.5
- NEW: It's now possible to add RCCs via TNManager.AddRCCs function that are not under TNManager.
- NEW: TNSyncRigidbody now has the "isImportant" flag just like TNAutoSync.
- FIX: TNManager.isActive set to false no longer prevents ping requests from being sent out.
- FIX: Added an extra step before enabling UDP traffic to avoid cases where it gets enabled erroneously.
- FIX: TNet.Tools.localAddress will now use GetHostAddresses instead of GetHostEntry.
- FIX: Unity 4.5 and 4.6 compile fixes.

1.8.4
- FIX: Host player will now assume ownership of TNObjects with no owner when joining a persistent channel.

1.8.3
- FIX: Eliminated obsolete warnings in the latest version of Unity.

1.8.2
- NEW: Added Target.Broadcast for when you want to send an RFC call to everyone connected to the server (ex: world chat).

1.8.1
- FIX: Executing remote function calls while offline should now work as expected.
- FIX: Default TNManager.Create function for pos/rot/vel/angVel should now work correctly again.

1.8.0
- NEW: Redesigned the object creation code. It's now fully extensible.
- NEW: It's now possible to do TNManager.Create using a string name of an object in the Resources folder.
- FIX: TNBehaviours being enabled now force TNObjects to rebuild the list of RFCs.

1.7.3
- NEW: Added the ability to specify player timeout on a per-player basis.
- FIX: SyncRigidbody was a bit out of date.
- FIX: Updated the server executable.

1.7.2
- NEW: It's now possible to have nested TNObjects on prefabs.
- FIX: Now only open channels will be returned by RequestChannelList.
- FIX: TNObject's delayed function calls weren't being used. Now they are.
- FIX: Fixed an issue with web player connectivity.

1.7.1
- FIX: iOS Local Address resolving fix.
- FIX: Connection fallback for certain routers.
- FIX: NAT Loopback failure work-around.
- FIX: TNManager.player's name will now always match TNManager.playerName.

1.7.0
- NEW: Added TNObject.ownerID.
- FIX: Joining a channel now defaults to non-persistent.
- FIX: TNServerInstance.StartRemote now has correct return parameters.
- FIX: Non-windows platforms should now be able to properly join LAN servers on LANs that have no public IP access.

1.6.9
- NEW: It's now possible to set the external IP discovery URL.
- NEW: It's now possible to perform the IP discovery asynchronously when desired.
- FIX: Starting the server should no longer break UPnP discovery.
- FIX: A few exception-related fixes.

1.6.8
- NEW: TCP lobby client can now handle file save/load requests.
- FIX: Flat out disabled UDP in the Unity web player, since every UDP request requires the policy file.
- FIX: Fixed an issue with how UDP packets were sent.
- FIX: Fixed an issue with how UPnP would cause Unity to hang for a few seconds when the server would be stopped.

1.6.6
- NEW: Restructured the server app to make it possible to use either TCP and UDP for the lobby.
- FIX: Variety of tweaks and fixes resulted from my development of Star Dots.

1.6.5
- NEW: TNManager.channelID, in case you want to know what channel you're in.
- NEW: Added the ability to specify a custom string with each channel that can be used to add information about the channel.
- NEW: You will now get an error message in Unity when trying to execute an RFC function that doesn't exist.
- FIX: Saved files will no longer be loaded if their version doesn't match.
- FIX: TcpChannel is now just Channel, as it has nothing to do with TCP.
- FIX: TNManager.isInChannel will now only return 'true' if the player is connected and in a channel.
- FIX: Many cases of "if connected, send data" were replaced with "if in channel, send data", which is more correct.
- FIX: Assortment of other minor fixes.

1.6.0
- NEW: Added a script that can instantiate an object when the player enters the scene (think: player avatar).
- NEW: It's now possible to create temporary game objects: they will be destroyed when the player that created them leaves.

1.5.0
- NEW: Added Universal Plug & Play functionality to easily open ports on the gateway/router.
- NEW: TNet Server app now supports port parameters and can also start the discovery server.
- NEW: Added TNObject.isMine flag that will only be 'true' on the client that instantiated it (or the host if that player leaves).
- NEW: Redesigned the discovery client. There is now several game Lobby server / clients instead.
- NEW: Game server can now automatically register itself with a remote lobby server.
- NEW: Added Tools.externalAddress that will return your internet-visible IP.
- FIX: TNet will no longer silently stop using UDP on the web player. UDP in the web player is simply no longer supported.
- MOD: Moved localAddress and IsLocalAddress() functions into Tools and made them static.

1.3.2
- NEW: Server list now contains the number of players on the server.
- FIX: Some minor tweaks.

1.3.1
- FIX: Unified usage of Object IDs -- they are now all UINTs.
- FIX: Minor tweaks to how things work.

1.3.0
- NEW: Added a way to join a random existing channel.
- NEW: Added a way to limit the number of players in the channel.

1.2.0
- NEW: Added TNManager.CloseChannel.
- FIX: TNManager.isHosting was not correct if the host was alone.
- FIX: TNAutoSync will now start properly on runtime-instantiated objects.

1.1.0
- NEW: Added AutoSync script that can automatically synchronize properties of your choice.
- NEW: Added AutoJoin script that can quickly join a server when the scene starts.
- NEW: Added a pair of new scenes to test the Auto scripts.
- NEW: It's now possible to figure out which player requested an object to be created when the ResponseCreate packet arrives.
- NEW: You can quickly check TNManager.isThisMyObject in a script's Awake function to determine if you're the one who created it.
- NEW: You can now instantiate objects with velocity.
- NEW: Added native support for ushort and uint data types (and their arrays).
- FIX: Fixed a bug with sending data directly to the specified player.
- FIX: Resolving a player address will no longer result in an exception with an invalid address.
- FIX: Changed the order of some notifications. A new host will always be chosen before "player left" notification, for example.
