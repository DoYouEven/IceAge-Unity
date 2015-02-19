//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

namespace TNet
{
/// <summary>
/// Class containing basic information about a remote player.
/// </summary>

public class Player
{
	static protected int mPlayerCounter = 0;

	/// <summary>
	/// Protocol version.
	/// </summary>

	public const int version = 12;

	/// <summary>
	/// All players have a unique identifier given by the server.
	/// </summary>

	public int id = 1;

	/// <summary>
	/// All players have a name that they chose for themselves.
	/// </summary>

	public string name = "Guest";

	/// <summary>
	/// Player's custom data. Set via TNManger.playerData.
	/// </summary>

	public object data = null;

#if !STANDALONE
	static DataNode mDummy = new DataNode("Version", version);

	/// <summary>
	/// Player's data converted to DataNode form. Always returns a valid DataNode.
	/// It's a convenience property, intended for read-only purposes.
	/// </summary>

	public DataNode dataNode
	{
		get
		{
			DataNode node = data as DataNode;
			return node ?? mDummy;
		}
	}
#endif

	public Player () { }
	public Player (string playerName) { name = playerName; }
}
}
