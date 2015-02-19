//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using System;
using System.Reflection;

namespace TNet
{
/// <summary>
/// Remote Function Call attribute. Used to identify functions that are supposed to be executed remotely.
/// </summary>

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RFC : Attribute
{
	public byte id = 0;

	public RFC () { }
	public RFC (byte rid) { id = rid; }
}

/// <summary>
/// Remote Creation Call attribute. Used to identify functions that are supposed to executed when custom OnCreate packets arrive.
/// </summary>

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RCC : System.Attribute
{
	public byte id;
	public RCC (byte rid) { id = rid; }
}

/// <summary>
/// Remote function calls consist of a method called on some object (such as a MonoBehavior).
/// This method may or may not have an explicitly specified Remote Function ID. If an ID is specified, the function
/// will require less data to be sent across the network as the ID will be sent instead of the function's name.
/// </summary>

public struct CachedFunc
{
	public byte id;
	public object obj;
	public MethodInfo func;
	public ParameterInfo[] parameters;
}
}
