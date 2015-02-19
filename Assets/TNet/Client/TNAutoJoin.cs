//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using TNet;
using UnityTools = TNet.UnityTools;

/// <summary>
/// Extremely simplified "join a server" functionality. Attaching this script will
/// make it possible to automatically join a remote server when the game starts.
/// It's best to place this script in a clean scene with a message that displays
/// a "Connecting, please wait..." message.
/// </summary>

[RequireComponent(typeof(TNManager))]
public class TNAutoJoin : MonoBehaviour
{
	static public TNAutoJoin instance;

	public string serverAddress = "127.0.0.1";
	public int serverPort = 5127;
	
	public string firstLevel = "Example 1";
	public int channelID = 1;
	public string disconnectLevel;

	public bool allowUDP = true;
	public bool connectOnStart = true;
	public string successFunctionName;
	public string failureFunctionName;

	/// <summary>
	/// Set the instance so this script can be easily found.
	/// </summary>

	void Awake () { if (instance == null) instance = this; }

	/// <summary>
	/// Connect to the server if requested.
	/// </summary>

	void Start () { if (connectOnStart) Connect(); }

	/// <summary>
	/// Connect to the server.
	/// </summary>

	public void Connect ()
	{
		// We don't want mobile devices to dim their screen and go to sleep while the app is running
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// Connect to the remote server
		TNManager.Connect(serverAddress, serverPort);
	}

	/// <summary>
	/// On success -- join a channel.
	/// </summary>

	void OnNetworkConnect (bool result, string message)
	{
		if (result)
		{
			// Make it possible to use UDP using a random port
			if (allowUDP) TNManager.StartUDP(Random.Range(10000, 50000));
			TNManager.JoinChannel(channelID, firstLevel);
		}
		else if (!string.IsNullOrEmpty(failureFunctionName))
		{
			UnityTools.Broadcast(failureFunctionName, message);
		}
		else Debug.LogError(message);
	}

	/// <summary>
	/// Disconnected? Go back to the menu.
	/// </summary>

	void OnNetworkDisconnect ()
	{
		if (!string.IsNullOrEmpty(disconnectLevel) && Application.loadedLevelName != disconnectLevel)
		{
			Application.LoadLevel(disconnectLevel);
		}
	}

	/// <summary>
	/// Joined a channel (or failed to).
	/// </summary>

	void OnNetworkJoinChannel (bool result, string message)
	{
		if (result)
		{
			if (!string.IsNullOrEmpty(successFunctionName))
			{
				UnityTools.Broadcast(successFunctionName);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(failureFunctionName))
			{
				UnityTools.Broadcast(failureFunctionName, message);
			}
			else Debug.LogError(message);

			TNManager.Disconnect();
		}
	}
}
