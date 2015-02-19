//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if UNITY_EDITOR || (!UNITY_FLASH && !NETFX_CORE && !UNITY_WP8)
#define REFLECTION_SUPPORT
#endif

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

#if REFLECTION_SUPPORT
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace TNet
{
/// <summary>
/// Implementing the IDataNodeSerializable interface in your class will make it possible to serialize
/// that class into the Data Node format more efficiently.
/// </summary>

public interface IDataNodeSerializable
{
	/// <summary>
	/// Serialize the object's data into Data Node.
	/// </summary>

	void Serialize (DataNode node);

	/// <summary>
	/// Deserialize the object's data from Data Node.
	/// </summary>

	void Deserialize (DataNode node);
}

/// <summary>
/// Data Node is a hierarchical data type containing a name and a value, as well as a variable number of children.
/// Data Nodes can be serialized to and from IO data streams.
/// Think of it as an alternative to having to include a huge 1 MB+ XML parsing library in your project.
/// 
/// Basic Usage:
/// To create a new node: new DataNode (name, value).
/// To add a new child node: dataNode.AddChild("Scale", Vector3.one).
/// To retrieve a Vector3 value: dataNode.GetChild<Vector3>("Scale").
/// </summary>

[Serializable]
public class DataNode
{
	// Actual saved value
	object mValue = null;

	// Temporary flag that gets set to 'true' after text-based deserialization
	[NonSerialized] bool mResolved = true;

	/// <summary>
	/// Data node's name.
	/// </summary>

	public string name;

	/// <summary>
	/// Data node's value.
	/// </summary>

	public object value
	{
		set
		{
			mValue = value;
			mResolved = true;
		}
		get
		{
			// ResolveValue returns 'false' when children were used by the custom data type and should now be ignored.
			if (!mResolved && !ResolveValue(null))
				children.Clear();
			return mValue;
		}
	}

	/// <summary>
	/// Whether this node is serializable or not.
	/// A node must have a value or children for it to be serialized. Otherwise there isn't much point in doing so.
	/// </summary>

	public bool isSerializable { get { return value != null || children.size > 0; } }

	/// <summary>
	/// List of child nodes.
	/// </summary>

	public List<DataNode> children = new List<DataNode>();

	/// <summary>
	/// Type the value is currently in.
	/// </summary>

	public Type type { get { return (value != null) ? mValue.GetType() : typeof(void); } }

	public DataNode () { }
	public DataNode (string name) { this.name = name; }
	public DataNode (string name, object value) { this.name = name; this.value = value; }

	/// <summary>
	/// Clear the value and the list of children.
	/// </summary>

	public void Clear ()
	{
		value = null;
		children.Clear();
	}

	/// <summary>
	/// Get the node's value cast into the specified type.
	/// </summary>

	public object Get (Type type) { return Serialization.ConvertValue(value, type); }

	/// <summary>
	/// Retrieve the value cast into the appropriate type.
	/// </summary>

	public T Get<T> ()
	{
		if (value is T) return (T)mValue;
		object retVal = Get(typeof(T));
		return (mValue != null) ? (T)retVal : default(T);
	}

	/// <summary>
	/// Retrieve the value cast into the appropriate type.
	/// </summary>

	public T Get<T> (T defaultVal)
	{
		if (value is T) return (T)mValue;
		object retVal = Get(typeof(T));
		return (mValue != null) ? (T)retVal : defaultVal;
	}

	/// <summary>
	/// Convenience function to add a new child node.
	/// </summary>

	public DataNode AddChild ()
	{
		DataNode tn = new DataNode();
		children.Add(tn);
		return tn;
	}

	/// <summary>
	/// Add a new child node without checking to see if another child with the same name already exists.
	/// </summary>

	public DataNode AddChild (string name)
	{
		DataNode node = AddChild();
		node.name = name;
		return node;
	}

	/// <summary>
	/// Add a new child node without checking to see if another child with the same name already exists.
	/// </summary>

	public DataNode AddChild (string name, object value)
	{
		DataNode node = AddChild();
		node.name = name;
		node.value = (value is Enum) ? value.ToString() : value;
		return node;
	}

	/// <summary>
	/// Set a child value. Will add a new child if a child with the same name is not already present.
	/// </summary>

	public DataNode SetChild (string name, object value)
	{
		DataNode node = GetChild(name);
		if (node == null) node = AddChild();
		node.name = name;
		node.value = (value is Enum) ? value.ToString() : value;
		return node;
	}

	/// <summary>
	/// Retrieve a child by name.
	/// </summary>

	public DataNode GetChild (string name)
	{
		for (int i = 0; i < children.size; ++i)
			if (children[i].name == name)
				return children[i];
		return null;
	}

	/// <summary>
	/// Retrieve a child by name, optionally creating a new one if the child doesn't already exist.
	/// </summary>

	public DataNode GetChild (string name, bool createIfMissing)
	{
		for (int i = 0; i < children.size; ++i)
			if (children[i].name == name)
				return children[i];

		if (createIfMissing) return AddChild(name);
		return null;
	}

	/// <summary>
	/// Get the value of the existing child.
	/// </summary>

	public T GetChild<T> (string name)
	{
		DataNode node = GetChild(name);
		if (node == null) return default(T);
		return node.Get<T>();
	}

	/// <summary>
	/// Get the value of the existing child or the default value if the child is not present.
	/// </summary>

	public T GetChild<T> (string name, T defaultValue)
	{
		DataNode node = GetChild(name);
		if (node == null) return defaultValue;
		return node.Get<T>();
	}

	/// <summary>
	/// Remove the specified child from the list.
	/// </summary>

	public void RemoveChild (string name)
	{
		for (int i = 0; i < children.size; ++i)
		{
			if (children[i].name == name)
			{
				children.RemoveAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// Clone the DataNode, creating a copy.
	/// </summary>

	public DataNode Clone ()
	{
		DataNode copy = new DataNode(name);
		copy.mValue = mValue;
		copy.mResolved = mResolved;
		for (int i = 0; i < children.size; ++i)
			copy.children.Add(children[i].Clone());
		return copy;
	}

#region Serialization

	/// <summary>
	/// Write the node hierarchy to the specified filename.
	/// </summary>

	public void Write (string path) { Write(path, false); }

	/// <summary>
	/// Read the node hierarchy from the specified file.
	/// </summary>

	static public DataNode Read (string path) { return Read(path, false); }

	/// <summary>
	/// Write the node hierarchy to the specified filename.
	/// </summary>

	public void Write (string path, bool binary)
	{
		if (binary)
		{
			FileStream stream = File.Create(path);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteObject(this);
			writer.Close();
		}
		else
		{
			StreamWriter writer = new StreamWriter(path, false);
			Write(writer, 0);
			writer.Close();
		}
	}

	/// <summary>
	/// Read the node hierarchy from the specified file.
	/// </summary>

	static public DataNode Read (string path, bool binary)
	{
		if (binary)
		{
			FileStream stream = File.OpenRead(path);
			BinaryReader reader = new BinaryReader(stream);
			DataNode node = reader.ReadObject<DataNode>();
			reader.Close();
			return node;
		}
		else
		{
			StreamReader reader = new StreamReader(path);
			DataNode node = Read(reader);
			reader.Close();
			return node;
		}
	}

	/// <summary>
	/// Read the node hierarchy from the specified buffer.
	/// </summary>

	static public DataNode Read (byte[] bytes, bool binary)
	{
		if (bytes == null || bytes.Length == 0) return null;
		MemoryStream stream = new MemoryStream(bytes);

		if (binary)
		{
			BinaryReader reader = new BinaryReader(stream);
			DataNode node = reader.ReadObject<DataNode>();
			reader.Close();
			return node;
		}
		else
		{
			StreamReader reader = new StreamReader(stream);
			DataNode node = Read(reader);
			reader.Close();
			return node;
		}
	}

	/// <summary>
	/// Write the node hierarchy to the stream reader, saving it in text format.
	/// </summary>

	public void Write (StreamWriter writer) { Write(writer, 0); }

	/// <summary>
	/// Read the node hierarchy from the stream reader containing data in text format.
	/// </summary>

	static public DataNode Read (StreamReader reader)
	{
		string line = GetNextLine(reader);
		int offset = CalculateTabs(line);
		DataNode node = new DataNode();
		node.Read(reader, line, ref offset);
		return node;
	}

	/// <summary>
	/// Convenience function for easy debugging -- convert the entire data into the string representation form.
	/// </summary>

	public override string ToString ()
	{
		if (!isSerializable) return "";
		MemoryStream stream = new MemoryStream();
		StreamWriter writer = new StreamWriter(stream);
		Write(writer, 0);

		stream.Seek(0, SeekOrigin.Begin);
		StreamReader reader = new StreamReader(stream);
		string text = reader.ReadToEnd();
		stream.Close();
		return text;
	}
#endregion
#region Private Functions

	/// <summary>
	/// Write the node into the stream writer.
	/// </summary>

	void Write (StreamWriter writer, int tab)
	{
		// Only proceed if this node has some data associated with it
		if (isSerializable)
		{
			// Write down its own data
			Write(writer, tab, name, value, true);

			// Iterate through children
			for (int i = 0; i < children.size; ++i)
				children[i].Write(writer, tab + 1);
		}
		if (tab == 0) writer.Flush();
	}

	/// <summary>
	/// Write the values into the stream writer.
	/// </summary>

	static void Write (StreamWriter writer, int tab, string name, object value, bool writeType)
	{
		if (string.IsNullOrEmpty(name) && value == null) return;

		WriteTabs(writer, tab);

		if (name != null)
		{
			writer.Write(Escape(name));

			if (value == null)
			{
				writer.Write('\n');
				return;
			}
		}

		Type type = value.GetType();

		if (type == typeof(string))
		{
			if (name != null) writer.Write(" = \"");
			writer.Write((string)value);
			if (name != null) writer.Write('"');
			writer.Write('\n');
		}
		else if (type == typeof(bool))
		{
			if (name != null) writer.Write(" = ");
			writer.Write((bool)value ? "true" : "false");
			writer.Write('\n');
		}
		else if (type == typeof(Int32) || type == typeof(float) || type == typeof(UInt32) ||
			type == typeof(byte) || type == typeof(short) || type == typeof(ushort))
		{
			if (name != null) writer.Write(" = ");
			writer.Write(value.ToString());
			writer.Write('\n');
		}
		else if (type == typeof(Vector2))
		{
			Vector2 v = (Vector2)value;
			writer.Write(name != null ? " = (" : "(");
			writer.Write(v.x.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(v.y.ToString(CultureInfo.InvariantCulture));
			writer.Write(")\n");
		}
		else if (type == typeof(Vector3))
		{
			Vector3 v = (Vector3)value;
			writer.Write(name != null ? " = (" : "(");
			writer.Write(v.x.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(v.y.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(v.z.ToString(CultureInfo.InvariantCulture));
			writer.Write(")\n");
		}
		else if (type == typeof(Color))
		{
			Color c = (Color)value;
			writer.Write(name != null ? " = (" : "(");
			writer.Write(c.r.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(c.g.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(c.b.ToString(CultureInfo.InvariantCulture));
			writer.Write(", ");
			writer.Write(c.a.ToString(CultureInfo.InvariantCulture));
			writer.Write(")\n");
		}
		else if (type == typeof(Color32))
		{
			Color32 c = (Color32)value;
			writer.Write(name != null ? " = 0x" : "0x");

			if (c.a == 255)
			{
				int i = (c.r << 16) | (c.g << 8) | c.b;
				writer.Write(i.ToString("X6"));
			}
			else
			{
				int i = (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
				writer.Write(i.ToString("X8"));
			}
			writer.Write('\n');
		}
		else
		{
			if (type == typeof(Vector4))
			{
				Vector4 v = (Vector4)value;
				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(v.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(v.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(v.z.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(v.w.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
			}
			else if (type == typeof(Quaternion))
			{
				Quaternion q = (Quaternion)value;
				Vector3 v = q.eulerAngles;
				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(v.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(v.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(v.z.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
			}
			else if (type == typeof(Rect))
			{
				Rect r = (Rect)value;
				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(r.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(r.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(r.width.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(r.height.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
			}
			else if (value is TList)
			{
				TList list = value as TList;

				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');

				if (list.Count > 0)
				{
					for (int i = 0, imax = list.Count; i < imax; ++i)
						Write(writer, tab + 1, null, list.Get(i), false);
				}
			}
			else if (value is System.Collections.IList)
			{
				System.Collections.IList list = value as System.Collections.IList;

				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');

				if (list.Count > 0)
				{
					for (int i = 0, imax = list.Count; i < imax; ++i)
						Write(writer, tab + 1, null, list[i], false);
				}
			}
			else if (value is IDataNodeSerializable)
			{
				IDataNodeSerializable ser = value as IDataNodeSerializable;
				DataNode node = new DataNode();
				ser.Serialize(node);

				if (name != null) writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');

				for (int i = 0; i < node.children.size; ++i)
				{
					DataNode child = node.children[i];
					child.Write(writer, tab + 1);
				}
			}
			else if (value is GameObject)
			{
				Debug.LogError("It's not possible to save game objects.");
				writer.Write('\n');
			}
			else if ((value as Component) != null)
			{
				Debug.LogError("It's not possible to save components.");
				writer.Write('\n');
			}
			else
			{
				if (writeType)
				{
					if (name != null) writer.Write(" = ");
					writer.Write(Serialization.TypeToName(type));
				}
				writer.Write('\n');

#if REFLECTION_SUPPORT
				List<FieldInfo> fields = type.GetSerializableFields();

				if (fields.size > 0)
				{
					for (int i = 0; i < fields.size; ++i)
					{
						FieldInfo field = fields[i];
						object val = field.GetValue(value);
						if (val != null) Write(writer, tab + 1, field.Name, val, true);
					}
				}
#endif
			}
		}
	}

	static void WriteTabs (StreamWriter writer, int count)
	{
		for (int i = 0; i < count; ++i)
			writer.Write('\t');
	}

	/// <summary>
	/// Read this node and all of its children from the stream reader.
	/// </summary>

	string Read (StreamReader reader, string line, ref int offset)
	{
		if (line != null)
		{
			int expected = offset;
			int divider = line.IndexOf("=", expected);

			if (divider == -1)
			{
				name = Unescape(line.Substring(offset)).Trim();
				value = null;
			}
			else
			{
				name = Unescape(line.Substring(offset, divider - offset)).Trim();
				mValue = line.Substring(divider + 1).Trim();
				mResolved = false;
			}

			line = GetNextLine(reader);
			offset = CalculateTabs(line);

			while (line != null)
			{
				if (offset == expected + 1)
				{
					line = AddChild().Read(reader, line, ref offset);
				}
				else break;
			}
		}
		return line;
	}

	/// <summary>
	/// Process the string values, converting them to proper objects.
	/// Returns whether child nodes should be processed in turn.
	/// </summary>

	bool ResolveValue (Type type)
	{
		mResolved = true;
		string line = mValue as string;

		if (string.IsNullOrEmpty(line))
			return SetValue(line, type, null);

		if (line.Length > 2)
		{
			// If the line starts with a quote, it must also end with a quote
			if (line[0] == '"' && line[line.Length - 1] == '"')
			{
				mValue = line.Substring(1, line.Length - 2);
				return true;
			}
			else if (line[0] == '0' && line[1] == 'x' && line.Length > 7)
			{
				mValue = ParseColor32(line, 2);
				return true;
			}

			// If the line starts with an opening bracket, it must always end with a closing bracket
			if (line[0] == '(' && line[line.Length - 1] == ')')
			{
				line = line.Substring(1, line.Length - 2);
				string[] parts = line.Split(',');

				if (parts.Length == 1) return SetValue(line, typeof(float), null);
				if (parts.Length == 2) return SetValue(line, typeof(Vector2), parts);
				if (parts.Length == 3) return SetValue(line, typeof(Vector3), parts);
				if (parts.Length == 4) return SetValue(line, typeof(Color), parts);

				mValue = line;
				return true;
			}

			bool b;

			if (bool.TryParse(line, out b))
			{
				mValue = b;
				return true;
			}
		}

		int dataStart = line.IndexOf('(');

		// Is there embedded data in brackets?
		if (dataStart == -1)
		{
			// Is it a number?
			if (line[0] == '-' || (line[0] >= '0' && line[0] <= '9'))
			{
				if (line.IndexOf('.') != -1)
				{
					float f;

					if (float.TryParse(line, out f))
					{
						mValue = f;
						return true;
					}
				}
				else
				{
					int i;

					if (int.TryParse(line, out i))
					{
						mValue = i;
						return true;
					}
				}
			}
		}
		else
		{
			// For some odd reason LastIndexOf() fails to find the last character of the string
			int dataEnd = (line[line.Length - 1] == ')') ? line.Length - 1 : line.LastIndexOf(')', dataStart);

			if (dataEnd != -1 && line.Length > 2)
			{
				// Set the type and extract the embedded data
				string strType = line.Substring(0, dataStart);
				type = Serialization.NameToType(strType);
				line = line.Substring(dataStart + 1, dataEnd - dataStart - 1);
			}
			else if (type == null)
			{
				// No type specified, so just treat this line as a string
				type = typeof(string);
				mValue = line;
				return true;
			}
		}

		// Resolve the type and set the value
		if (type == null) type = Serialization.NameToType(line);
		return SetValue(line, type, null);
	}

	/// <summary>
	/// Set the node's value using its text representation.
	/// Returns whether the child nodes should be processed or not.
	/// </summary>

	bool SetValue (string text, Type type, string[] parts)
	{
		if (type == null || type == typeof(void))
		{
			mValue = null;
		}
		else if (type == typeof(string))
		{
			mValue = text;
		}
		else if (type == typeof(bool))
		{
			bool b = false;
			if (bool.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(byte))
		{
			byte b;
			if (byte.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(Int16))
		{
			Int16 b;
			if (Int16.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(UInt16))
		{
			UInt16 b;
			if (UInt16.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(Int32))
		{
			Int32 b;
			if (Int32.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(UInt32))
		{
			UInt32 b;
			if (UInt32.TryParse(text, out b)) mValue = b;
		}
		else if (type == typeof(float))
		{
			float b;
			if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out b)) mValue = b;
		}
		else if (type == typeof(double))
		{
			double b;
			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out b)) mValue = b;
		}
		else if (type == typeof(Vector2))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 2)
			{
				Vector2 v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y))
					mValue = v;
			}
		}
		else if (type == typeof(Vector3))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 3)
			{
				Vector3 v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z))
					mValue = v;
			}
		}
		else if (type == typeof(Vector4))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 4)
			{
				Vector4 v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z) &&
					float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out v.w))
					mValue = v;
			}
		}
		else if (type == typeof(Quaternion))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 3)
			{
				Vector3 v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z))
					mValue = Quaternion.Euler(v);
			}
			else if (parts.Length == 4)
			{
				Quaternion v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z) &&
					float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out v.w))
					mValue = v;
			}
		}
		else if (type == typeof(Color))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 4)
			{
				Color v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.r) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.g) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.b) &&
					float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out v.a))
					mValue = v;
			}
		}
		else if (type == typeof(Rect))
		{
			if (parts == null) parts = text.Split(',');

			if (parts.Length == 4)
			{
				Vector4 v;
				if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x) &&
					float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y) &&
					float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z) &&
					float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out v.w))
					mValue = new Rect(v.x, v.y, v.z, v.w);
			}
		}
		else if (type.Implements(typeof(IDataNodeSerializable)))
		{
			IDataNodeSerializable ds = (IDataNodeSerializable)type.Create();
			ds.Deserialize(this);
			mValue = ds;
			return false;
		}
		else if (!type.IsSubclassOf(typeof(Component)))
		{
			bool isIList = type.Implements(typeof(System.Collections.IList));
			bool isTList = (!isIList && type.Implements(typeof(TList)));
			mValue = (isTList || isIList) ? type.Create(children.size) : type.Create();

			if (mValue == null)
			{
				Debug.LogError("Unable to create a " + type);
				return true;
			}

			if (isTList)
			{
				TList list = mValue as TList;
				Type elemType = type.GetGenericArgument();

				if (elemType != null)
				{
					for (int i = 0; i < children.size; ++i)
					{
						DataNode child = children[i];

						if (child.value == null)
						{
							child.mValue = child.name;
							child.mResolved = false;
							child.ResolveValue(elemType);
							list.Add(child.mValue);
						}
						else if (child.name == "Add")
						{
							child.ResolveValue(elemType);
							list.Add(child.mValue);
						}
						else Debug.LogWarning("Unexpected node in an array: " + child.name);
					}
					return false;
				}
				else Debug.LogError("Unable to determine the element type of " + type);
			}
			else if (isIList)
			{
				// This is for both List<Type> and Type[] arrays.
				System.Collections.IList list = mValue as System.Collections.IList;
				Type elemType = type.GetGenericArgument();
				if (elemType == null) elemType = type.GetElementType();
				bool fixedSize = (list.Count == children.size);

				if (elemType != null)
				{
					for (int i = 0; i < children.size; ++i)
					{
						DataNode child = children[i];

						if (child.value == null)
						{
							child.mValue = child.name;
							child.mResolved = false;
							child.ResolveValue(elemType);
							if (fixedSize) list[i] = child.mValue;
							else list.Add(child.mValue);
						}
						else if (child.name == "Add")
						{
							child.ResolveValue(elemType);
							if (fixedSize) list[i] = child.mValue;
							else list.Add(child.mValue);
						}
						else Debug.LogWarning("Unexpected node in an array: " + child.name);
					}
					return false;
				}
				else Debug.LogError("Unable to determine the element type of " + type);
			}
			else if (type.IsClass)
			{
				for (int i = 0; i < children.size; ++i)
				{
					DataNode child = children[i];
					mValue.SetSerializableField(child.name, child.value);
				}
				return false;
			}
			else Debug.LogError("Unhandled type: " + type);
		}
		return true;
	}
#endregion
#region Static Helper Functions

	/// <summary>
	/// Get the next line from the stream reader.
	/// </summary>

	static string GetNextLine (StreamReader reader)
	{
		string line = reader.ReadLine();

		while (line != null && line.Trim().StartsWith("//"))
		{
			line = reader.ReadLine();
			if (line == null) return null;
		}
		return line;
	}

	/// <summary>
	/// Calculate the number of tabs at the beginning of the line.
	/// </summary>

	static int CalculateTabs (string line)
	{
		if (line != null)
		{
			for (int i = 0; i < line.Length; ++i)
			{
				if (line[i] == '\t') continue;
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Escape the characters in the string.
	/// </summary>

	static string Escape (string val)
	{
		if (!string.IsNullOrEmpty(val))
		{
			val = val.Replace("\n", "\\n");
			val = val.Replace("\t", "\\t");
		}
		return val;
	}

	/// <summary>
	/// Recover escaped characters, converting them back to usable characters.
	/// </summary>

	static string Unescape (string val)
	{
		if (!string.IsNullOrEmpty(val))
		{
			val = val.Replace("\\n", "\n");
			val = val.Replace("\\t", "\t");
		}
		return val;
	}

	/// <summary>
	/// Convert a hexadecimal character to its decimal value.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static int HexToDecimal (char ch)
	{
		switch (ch)
		{
			case '0': return 0x0;
			case '1': return 0x1;
			case '2': return 0x2;
			case '3': return 0x3;
			case '4': return 0x4;
			case '5': return 0x5;
			case '6': return 0x6;
			case '7': return 0x7;
			case '8': return 0x8;
			case '9': return 0x9;
			case 'a':
			case 'A': return 0xA;
			case 'b':
			case 'B': return 0xB;
			case 'c':
			case 'C': return 0xC;
			case 'd':
			case 'D': return 0xD;
			case 'e':
			case 'E': return 0xE;
			case 'f':
			case 'F': return 0xF;
		}
		return 0xF;
	}

	/// <summary>
	/// Parse a RrGgBbAa color encoded in the string.
	/// </summary>

	static Color32 ParseColor32 (string text, int offset)
	{
		byte r = (byte)((HexToDecimal(text[offset]) << 4) | HexToDecimal(text[offset + 1]));
		byte g = (byte)((HexToDecimal(text[offset + 2]) << 4) | HexToDecimal(text[offset + 3]));
		byte b = (byte)((HexToDecimal(text[offset + 4]) << 4) | HexToDecimal(text[offset + 5]));
		byte a = (byte)((offset + 8 <= text.Length) ? (HexToDecimal(text[offset + 6]) << 4) | HexToDecimal(text[offset + 7]) : 255);
		return new Color32(r, g, b, a);
	}
#endregion
}
}
