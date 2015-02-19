//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

// Unity has an outdated version of Mono that doesn't have the NetworkInformation namespace.
#if !UNITY_3_4 && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6
using System.Net.NetworkInformation;
#endif

namespace TNet
{
/// <summary>
/// Universal Plug & Play functionality: auto-detect external IP and open external ports.
/// Technically this class would be a fair bit shorter if I had used an XML parser...
/// However I'd rather not, as adding the XML library also adds 1 megabyte to the executable's size in Unity.
/// 
/// Example usage:
/// UPnP p = new UPnP();
/// p.OpenTCP(5127);
/// 
/// Don't worry about closing ports. This class will do it for you when its instance gets destroyed.
/// </summary>

public class UPnP
{
	public enum Status
	{
		Inactive,
		Searching,
		Success,
		Failure,
	}

	Status mStatus = Status.Inactive;
	IPAddress mGatewayAddress = IPAddress.None;
	Thread mDiscover = null;
	
	string mGatewayURL = null;
	string mControlURL = null;
	string mServiceType = null;
	List<Thread> mThreads = new List<Thread>();
	List<int> mPorts = new List<int>();

	public delegate void OnPortRequest (UPnP up, int port, ProtocolType protocol, bool success);

	class ExtraParams
	{
		public Thread th;
		public string action;
		public string request;
		public int port;
		public ProtocolType protocol;
		public OnPortRequest callback;
	}

	/// <summary>
	/// Name that will show up on the gateway's list.
	/// </summary>

	public string name = "TNetServer";

	/// <summary>
	/// Current UPnP status.
	/// </summary>

	public Status status { get { return mStatus; } }

	/// <summary>
	/// Gateway's IP address, such as 192.168.1.1
	/// </summary>

	public IPAddress gatewayAddress { get { return mGatewayAddress; } }

	/// <summary>
	/// Whether there are threads active.
	/// </summary>

	public bool hasThreadsActive { get { return mThreads.size > 0; } }

	/// <summary>
	/// Start the Universal Plug and Play lobby process.
	/// </summary>

	public UPnP ()
	{
		Thread th = new Thread(ThreadDiscover);
		mDiscover = th;
		mThreads.Add(th);
		th.Start(th);
	}

	/// <summary>
	/// Wait for all threads to finish.
	/// </summary>

	~UPnP () { mDiscover = null; Close(); WaitForThreads(); }

	/// <summary>
	/// Close all ports that we've opened.
	/// </summary>

	public void Close ()
	{
		lock (mThreads)
		{
			for (int i = mThreads.size; i > 0; )
			{
				Thread th = mThreads[--i];

				if (th != mDiscover)
				{
					th.Abort();
					mThreads.RemoveAt(i);
				}
			}
		}

		for (int i = mPorts.size; i > 0; )
		{
			int id = mPorts[--i];
			int port = (id >> 8);
			bool tcp = ((id & 1) == 1);
			Close(port, tcp, null);
		}
	}

	/// <summary>
	/// Wait for all threads to finish.
	/// </summary>

	public void WaitForThreads ()
	{
		for (int i = 0; mThreads.size > 0 && i < 2000; ++i)
			Thread.Sleep(1);
	}

	/// <summary>
	/// Gateway lobby logic is done on a separate thread so that it's not blocking the main thread.
	/// </summary>

	void ThreadDiscover (object obj)
	{
		Thread th = (Thread)obj;

		string request = "M-SEARCH * HTTP/1.1\r\n" +
						"HOST: 239.255.255.250:1900\r\n" +
						"ST:upnp:rootdevice\r\n" +
						"MAN:\"ssdp:discover\"\r\n" +
						"MX:3\r\n\r\n";

		byte[] requestBytes = Encoding.ASCII.GetBytes(request);
		int port = 10000 + (int)(DateTime.UtcNow.Ticks % 45000);
		List<IPAddress> ips = Tools.localAddresses;

		// UPnP discovery should happen on all network interfaces
		for (int i = 0; i < ips.size; ++i)
		{
			IPAddress ip = ips[i];
			mStatus = Status.Searching;
			UdpClient receiver = null;

			try
			{
				UdpClient sender = new UdpClient(new IPEndPoint(ip, port));

				sender.Connect(IPAddress.Broadcast, 1900);
				sender.Send(requestBytes, requestBytes.Length);
				sender.Close();

				receiver = new UdpClient(new IPEndPoint(ip, port));
				receiver.Client.ReceiveTimeout = 3000;

				IPEndPoint sourceAddress = new IPEndPoint(IPAddress.Any, 0);

				for (; ; )
				{
					byte[] data = receiver.Receive(ref sourceAddress);

					if (ParseResponse(Encoding.ASCII.GetString(data, 0, data.Length)))
					{
						receiver.Close();

						lock (mThreads)
						{
							mGatewayAddress = sourceAddress.Address;
#if UNITY_EDITOR
							UnityEngine.Debug.Log("[TNet] UPnP Gateway: " + mGatewayAddress);
#endif
							mStatus = Status.Success;
							mThreads.Remove(th);
						}
						mDiscover = null;
						return;
					}
				}
			}
			catch (System.Exception) {}

			if (receiver != null) receiver.Close();

			lock (mThreads)
			{
				mStatus = Status.Failure;
				mThreads.Remove(th);
			}
			mDiscover = null;

			// If we found one, we're done
			if (mStatus == Status.Success) break;
		}

		if (mStatus != Status.Success)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.LogWarning("[TNet] UPnP discovery failed. TNet won't be able to open ports automatically.");
#else
			Console.WriteLine("UPnP discovery failed. TNet won't be able to open ports automatically.");
#endif
		}
	}

	/// <summary>
	/// Parse the response to the UPnP discovery message.
	/// </summary>

	bool ParseResponse (string response)
	{
		// Find the "Location" header
		int index = response.IndexOf("LOCATION:", StringComparison.OrdinalIgnoreCase);
		if (index == -1) return false;
		index += 9;
		int end = response.IndexOf('\r', index);
		if (end == -1) return false;

		// Base URL: http://192.168.1.1:2555/upnp/f3710630-b3ce-348c-b5a5-4c9d74f6ee99/desc.xml
		string baseURL = response.Substring(index, end - index).Trim();

		// Gateway URL: http://192.168.1.1:2555
		int offset = baseURL.IndexOf("://");
		offset = baseURL.IndexOf('/', offset + 3);
		mGatewayURL = baseURL.Substring(0, offset);

		// Get the port control URL
		return GetControlURL(baseURL);
	}

	/// <summary>
	/// Get the port control URL from the gateway.
	/// </summary>

	bool GetControlURL (string url)
	{
		string response = Tools.GetResponse(WebRequest.Create(url));
		if (string.IsNullOrEmpty(response)) return false;

		// For me the full hierarchy of nodes was:
		// root -> device -> deviceList -> device (again) -> deviceList (again) -> service,
		// where the <serviceType> node had an identifier ending in one of the prefixes below.
		// The service node with this type then contained <controlURL> node with the actual URL.
		// TLDR: It's just a hell of a lot easier to go straight for the prize instead.

		// IP gateway (Router, cable modem)
		mServiceType = "WANIPConnection";
		int offset = response.IndexOf(mServiceType);

		// PPP gateway (ADSL modem)
		if (offset == -1)
		{
			mServiceType = "WANPPPConnection";
			offset = response.IndexOf(mServiceType);
			if (offset == -1) return false;
		}

		int end = response.IndexOf("</service>", offset);
		if (end == -1) return false;

		int start = response.IndexOf("<controlURL>", offset, end - offset);
		if (start == -1) return false;
		start += 12;

		end = response.IndexOf("</controlURL>", start, end - start);
		if (end == -1) return false;

		// Final URL
		mControlURL = mGatewayURL + response.Substring(start, end - start);
		return true;
	}

	/// <summary>
	/// Send a SOAP request to the gateway.
	/// Some routers (like my NETGEAR RangeMax) seem to fail requests for no reason, so repetition may be needed.
	/// </summary>

	string SendRequest (string action, string content, int timeout, int repeat)
	{
		string request = "<?xml version=\"1.0\"?>\n" +
			"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=" +
			"\"http://schemas.xmlsoap.org/soap/encoding/\">\n<s:Body>\n" +
			"<m:" + action + " xmlns:m=\"urn:schemas-upnp-org:service:" + mServiceType + ":1\">\n";

		if (!string.IsNullOrEmpty(content)) request += content;

		request += "</m:" + action + ">\n</s:Body>\n</s:Envelope>\n";

		byte[] b = Encoding.UTF8.GetBytes(request);

		string response = null;

		try
		{
			for (int i = 0; i < repeat; ++i)
			{
				WebRequest web = HttpWebRequest.Create(mControlURL);
				web.Timeout = timeout;
				web.Method = "POST";
				web.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:" + mServiceType + ":1#" + action + "\"");
				web.ContentType = "text/xml; charset=\"utf-8\"";
				web.ContentLength = b.Length;
				web.GetRequestStream().Write(b, 0, b.Length);
				response = Tools.GetResponse(web);
				if (!string.IsNullOrEmpty(response))
					return response;
			}
		}
		catch (System.Exception) { }
		return null;
	}

	/// <summary>
	/// Open up a TCP port on the gateway.
	/// </summary>

	public void OpenTCP (int port) { Open(port, true, null); }

	/// <summary>
	/// Open up a UDP port on the gateway.
	/// </summary>

	public void OpenUDP (int port) { Open(port, false, null); }

	/// <summary>
	/// Open up a TCP port on the gateway.
	/// </summary>

	public void OpenTCP (int port, OnPortRequest callback) { Open(port, true, callback); }

	/// <summary>
	/// Open up a UDP port on the gateway.
	/// </summary>

	public void OpenUDP (int port, OnPortRequest callback) { Open(port, false, callback); }

	/// <summary>
	/// Open up a port on the gateway.
	/// </summary>

	void Open (int port, bool tcp, OnPortRequest callback)
	{
		int id = (port << 8) | (tcp ? 1 : 0);

		if (port > 0 && !mPorts.Contains(id) && mStatus != Status.Failure)
		{
			string addr = Tools.localAddress.ToString();
			if (addr == "127.0.0.1") return;

			mPorts.Add(id);

			ExtraParams xp = new ExtraParams();
			xp.callback = callback;
			xp.port = port;
			xp.protocol = tcp ? ProtocolType.Tcp : ProtocolType.Udp;
			xp.action = "AddPortMapping";
			xp.request = "<NewRemoteHost></NewRemoteHost>\n" +
				"<NewExternalPort>" + port + "</NewExternalPort>\n" +
				"<NewProtocol>" + (tcp ? "TCP" : "UDP") + "</NewProtocol>\n" +
				"<NewInternalPort>" + port + "</NewInternalPort>\n" +
				"<NewInternalClient>" + addr + "</NewInternalClient>\n" +
				"<NewEnabled>1</NewEnabled>\n" +
				"<NewPortMappingDescription>" + name + "</NewPortMappingDescription>\n" +
				"<NewLeaseDuration>0</NewLeaseDuration>\n";

			xp.th = new Thread(OpenRequest);
			lock (mThreads) mThreads.Add(xp.th);
			xp.th.Start(xp);
		}
		else if (callback != null)
		{
			callback(this, port, tcp ? ProtocolType.Tcp : ProtocolType.Udp, false);
		}
	}

	/// <summary>
	/// Stop port forwarding that was set up earlier.
	/// </summary>

	public void CloseTCP (int port) { Close(port, true, null); }

	/// <summary>
	/// Stop port forwarding that was set up earlier.
	/// </summary>

	public void CloseUDP (int port) { Close(port, false, null); }

	/// <summary>
	/// Stop port forwarding that was set up earlier.
	/// </summary>

	public void CloseTCP (int port, OnPortRequest callback) { Close(port, true, callback); }

	/// <summary>
	/// Stop port forwarding that was set up earlier.
	/// </summary>

	public void CloseUDP (int port, OnPortRequest callback) { Close(port, false, callback); }

	/// <summary>
	/// Stop port forwarding that was set up earlier.
	/// </summary>

	void Close (int port, bool tcp, OnPortRequest callback)
	{
		int id = (port << 8) | (tcp ? 1 : 0);

		if (port > 0 && mPorts.Remove(id) && mStatus == Status.Success)
		{
			ExtraParams xp = new ExtraParams();
			xp.callback = callback;
			xp.port = port;
			xp.protocol = tcp ? ProtocolType.Tcp : ProtocolType.Udp;
			xp.action = "DeletePortMapping";
			xp.request = "<NewRemoteHost></NewRemoteHost>\n" +
				"<NewExternalPort>" + port + "</NewExternalPort>\n" +
				"<NewProtocol>" + (tcp ? "TCP" : "UDP") + "</NewProtocol>\n";

			if (callback != null)
			{
				xp.th = new Thread(CloseRequest);

				lock (mThreads)
				{
					mThreads.Add(xp.th);
					xp.th.Start(xp);
				}
			}
			else CloseRequest(xp);
		}
		else if (callback != null)
		{
			callback(this, port, tcp ? ProtocolType.Tcp : ProtocolType.Udp, false);
		}
	}

	/// <summary>
	/// Thread callback that requests a port to be opened.
	/// </summary>

	void OpenRequest (object obj)
	{
		while (mStatus == Status.Searching) Thread.Sleep(1);
		SendRequest((ExtraParams)obj);
	}

	/// <summary>
	/// Thread callback that requests a port to be closed.
	/// </summary>

	void CloseRequest (object obj) { SendRequest((ExtraParams)obj); }

	/// <summary>
	/// Open or close request.
	/// </summary>

	void SendRequest (ExtraParams xp)
	{
		string response = (mStatus == Status.Success) ? SendRequest(xp.action, xp.request, 10000, 3) : null;
		if (xp.callback != null)
			xp.callback(this, xp.port, xp.protocol, !string.IsNullOrEmpty(response));
		if (xp.th != null) lock (mThreads) mThreads.Remove(xp.th);
	}
}
}
