﻿using UnityEngine;
using System.Collections.Generic;

delegate void TreeVisitor<T>(T nodeData);

class NTree<T>
{
	private T data;
	private LinkedList<NTree<T>> children;
	
	public NTree(T nData)
	{
		data     = nData;
		children = new LinkedList<NTree<T>>();
	}
	
	public void AddChild(T nData)
	{
		children.AddFirst(new NTree<T>(nData));
	}
	
	public NTree<T> GetChild(int i)
	{
		foreach (NTree<T> n in children)
			if (--i == 0)
				return n;
		return null;
	}
	
	public void Traverse(NTree<T> node, TreeVisitor<T> visitor)
	{
		visitor(node.data);
		foreach (NTree<T> kid in node.children)
			Traverse(kid, visitor);
	}
}

