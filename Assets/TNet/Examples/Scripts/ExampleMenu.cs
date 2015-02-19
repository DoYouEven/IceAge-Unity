//------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//------------------------------------------

using UnityEngine;
using TNet;
using System.IO;
using System.Collections;
using UnityTools = TNet.UnityTools;

/// <summary>
/// This script provides a main menu for all examples.
/// The menu is created in Unity's built-in Immediate Mode GUI system.
/// The menu makes use of the following TNet functions:
/// - TNManager.Connect
/// - TNManager.JoinChannel
/// - TNManager.LeaveChannel
/// - TNManager.Disconnect
/// - TNServerInstance.Start
/// - TNServerInstance.Stop
/// </summary>

[ExecuteInEditMode]
public class ExampleMenu : MonoBehaviour
{
	const float buttonWidth = 150f;
	const float buttonHeight = 40f;

	public int serverTcpPort = 5127;
	public string mainMenu = "Example Menu";
	public string[] examples;
	public GUIStyle button;
	public GUIStyle text;
	public GUIStyle textLeft;
	public GUIStyle input;

	string mAddress = "127.0.0.1";
	string mMessage = "";
	float mAlpha = 0f;

	/// <summary>
	/// Start listening for incoming UDP packets right away.
	/// </summary>

	void Start ()
	{
		if (Application.isPlaying)
		{
			// Start resolving IPs
			Tools.ResolveIPs(null);

			// We don't want mobile devices to dim their screen and go to sleep while the app is running
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			// Make it possible to use UDP using a random port
			TNManager.StartUDP(Random.Range(10000, 50000));
		}
	}

	/// <summary>
	/// Adjust the server list's alpha based on whether it should be shown or not.
	/// </summary>

	void Update ()
	{
		if (Application.isPlaying)
		{
			float target = (TNLobbyClient.knownServers.list.size == 0) ? 0f : 1f;
			mAlpha = UnityTools.SpringLerp(mAlpha, target, 8f, Time.deltaTime);
		}
	}

	/// <summary>
	/// Show the GUI for the examples.
	/// </summary>

	void OnGUI ()
	{
		if (!TNManager.isConnected)
		{
			DrawConnectMenu();
		}
		else
		{
			if (!Application.isPlaying || Application.loadedLevelName == mainMenu)
			{
				DrawSelectionMenu();
			}
			else if (TNManager.isInChannel)
			{
				DrawExampleMenu();
			}
			DrawDisconnectButton();
		}
		DrawDebugInfo();
	}

	/// <summary>
	/// This menu is shown if the client has not yet connected to the server.
	/// </summary>

	void DrawConnectMenu ()
	{
		Rect rect = new Rect(Screen.width * 0.5f - 200f * 0.5f - mAlpha * 120f,
			Screen.height * 0.5f - 100f, 200f, 220f);

		// Show a half-transparent box around the upcoming UI
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.Box(UnityTools.PadRect(rect, 8f), "");
		GUI.color = Color.white;

		GUILayout.BeginArea(rect);
		{
			GUILayout.Label("Server Address", text);
			mAddress = GUILayout.TextField(mAddress, input, GUILayout.Width(200f));

			if (GUILayout.Button("Connect", button))
			{
				// We want to connect to the specified destination when the button is clicked on.
				// "OnNetworkConnect" function will be called sometime later with the result.
				TNManager.Connect(mAddress);
				mMessage = "Connecting...";
			}

			if (TNServerInstance.isActive)
			{
				GUI.backgroundColor = Color.red;

				if (GUILayout.Button("Stop the Server", button))
				{
					// Stop the server, saving all the data
					TNServerInstance.Stop("server.dat");
					mMessage = "Server stopped";
				}
			}
			else
			{
				GUI.backgroundColor = Color.green;

				if (GUILayout.Button("Start a Local Server", button))
				{
#if UNITY_WEBPLAYER
					mMessage = "Can't host from the Web Player due to Unity's security restrictions";
#else
					// Start a local server, loading the saved data if possible
					// The UDP port of the server doesn't matter much as it's optional,
					// and the clients get notified of it via Packet.ResponseSetUDP.
					int udpPort = Random.Range(10000, 40000);
					TNLobbyClient lobby = GetComponent<TNLobbyClient>();

					if (lobby == null)
					{
						TNServerInstance.Start(serverTcpPort, udpPort, "server.dat");
					}
					else
					{
						TNServerInstance.Type type = (lobby is TNUdpLobbyClient) ?
							TNServerInstance.Type.Udp : TNServerInstance.Type.Tcp;
						TNServerInstance.Start(serverTcpPort, udpPort, lobby.remotePort, "server.dat", type);
					}
					mMessage = "Server started";
#endif
				}
			}
			GUI.backgroundColor = Color.white;

			if (!string.IsNullOrEmpty(mMessage)) GUILayout.Label(mMessage, text);
		}
		GUILayout.EndArea();

		if (mAlpha > 0.01f)
		{
			rect.x = rect.x + (Screen.width - rect.xMin - rect.xMax) * mAlpha;
			DrawServerList(rect);
		}
	}

	/// <summary>
	/// This function is called when a connection is either established or it fails to connect.
	/// Connecting to a server doesn't mean that the connected players are now immediately able
	/// to see each other, as they have not yet joined a channel. Only players that have joined
	/// some channel are able to see and interact with other players in the same channel.
	/// You can call TNManager.JoinChannel here if you like, but in this example we let the player choose.
	/// </summary>

	void OnNetworkConnect (bool success, string message) { mMessage = message; }

	/// <summary>
	/// This menu is shown when a connection has been established and the player has not yet joined any channel.
	/// </summary>

	void DrawSelectionMenu ()
	{
		int count = examples.Length;

		Rect rect = new Rect(
			Screen.width * 0.5f - buttonWidth * 0.5f,
			Screen.height * 0.5f - buttonHeight * 0.5f * count,
			buttonWidth, buttonHeight);

		for (int i = 0; i < count; ++i)
		{
			string sceneName = examples[i];

			if (GUI.Button(rect, sceneName, button))
			{
				// When a button is clicked, join the specified channel.
				// Whoever creates the channel also sets the scene that will be loaded by everyone who joins.
				// In this case, we are specifying the name of the scene we've just clicked on.
				TNManager.JoinChannel(i + 1, sceneName);
			}
			rect.y += buttonHeight;
		}
		rect.y += 20f;
	}

	/// <summary>
	/// This menu is shown if the player has joined a channel.
	/// </summary>

	void DrawExampleMenu ()
	{
		Rect rect = new Rect(0f, Screen.height - buttonHeight, buttonWidth, buttonHeight);

		if (GUI.Button(rect, "Main Menu", button))
		{
			// Leaving the channel will cause the "OnNetworkLeaveChannel" to be sent out.
			TNManager.LeaveChannel();
		}
	}

	/// <summary>
	/// We want to return to the menu when we leave the channel.
	/// This message is also sent out when we get disconnected.
	/// </summary>

	void OnNetworkLeaveChannel ()
	{
		Application.LoadLevel(mainMenu);
	}

	/// <summary>
	/// The disconnect button is only shown if we are currently connected.
	/// </summary>

	void DrawDisconnectButton ()
	{
		Rect rect = new Rect(Screen.width - buttonWidth, Screen.height - buttonHeight, buttonWidth, buttonHeight);

		if (GUI.Button(rect, "Disconnect", button))
		{
			// Disconnecting while in some channel will cause "OnNetworkLeaveChannel" to be sent out first,
			// followed by "OnNetworkDisconnect". Disconnecting while not in a channel will only trigger
			// "OnNetworkDisconnect".
			TNManager.Disconnect();
		}
	}

	/// <summary>
	/// Print some additional information such as ping and which type of connection this is.
	/// </summary>

	void DrawDebugInfo ()
	{
		GUILayout.Label("LAN: " + Tools.localAddress.ToString(), textLeft);

		if (Application.isPlaying)
		{
			if (Tools.isExternalIPReliable)
				GUILayout.Label("WAN: " + Tools.externalAddress, textLeft);
			else GUILayout.Label("WAN: Resolving...", textLeft);

			if (TNManager.isConnected)
				GUILayout.Label("Ping: " + TNManager.ping + " (" + (TNManager.canUseUDP ? "TCP+UDP" : "TCP") + ")", textLeft);
		}
	}

	/// <summary>
	/// Draw the list of known LAN servers.
	/// </summary>

	void DrawServerList (Rect rect)
	{
		GUI.color = new Color(1f, 1f, 1f, mAlpha * mAlpha * 0.5f);
		GUI.Box(UnityTools.PadRect(rect, 8f), "");
		GUI.color = new Color(1f, 1f, 1f, mAlpha * mAlpha);

		GUILayout.BeginArea(rect);
		{
			GUILayout.Label("LAN Server List", text);

			// List of discovered servers
			List<ServerList.Entry> list = TNLobbyClient.knownServers.list;

			// Server list example script automatically collects servers that have recently announced themselves
			for (int i = 0; i < list.size; ++i)
			{
				ServerList.Entry ent = list[i];

				// NOTE: I am using 'internalAddress' here because I know all servers are hosted on LAN.
				// If you are hosting outside of your LAN, you should probably use 'externalAddress' instead.
				if (GUILayout.Button(ent.internalAddress.ToString(), button))
				{
					TNManager.Connect(ent.internalAddress, ent.internalAddress);
					mMessage = "Connecting...";
				}
			}
		}
		GUILayout.EndArea();
		GUI.color = Color.white;
	}
}
