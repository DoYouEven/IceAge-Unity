//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;

namespace TNet
{
/// <summary>
/// Base class for Game and Lobby servers capable of saving and loading files.
/// </summary>

public class FileServer
{
	/// <summary>
	/// You can save files on the server, such as player inventory, Fog of War map updates, player avatars, etc.
	/// </summary>

	Dictionary<string, byte[]> mSavedFiles = new Dictionary<string, byte[]>();

	/// <summary>
	/// Log an error message.
	/// </summary>

	protected void Error (string error)
	{
#if UNITY_EDITOR
		UnityEngine.Debug.LogError("[TNet] Error: " + error);
#elif STANDALONE
		Console.WriteLine("ERROR: " + error);
#endif
	}

	/// <summary>
	/// Save the specified file.
	/// </summary>

	public void SaveFile (string fileName, byte[] data)
	{
		mSavedFiles[fileName] = data;
		Tools.WriteFile(fileName, data);
	}

	/// <summary>
	/// Load the specified file.
	/// </summary>

	public byte[] LoadFile (string fileName)
	{
		byte[] data;

		if (!mSavedFiles.TryGetValue(fileName, out data))
		{
			data = Tools.ReadFile(fileName);
			mSavedFiles[fileName] = data;
		}
		return data;
	}

	/// <summary>
	/// Delete the specified file.
	/// </summary>

	public void DeleteFile (string fileName)
	{
		mSavedFiles.Remove(fileName);
		Tools.DeleteFile(fileName);
	}
}
}
