using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public abstract class Node : IEnumerable<Node>
{
   public Node? Parent { get; private set; }
   public Range TokenRange { get; private set; }

   [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
   private List<Node>? m_Children;

   public void SetTokenRange(int beginTokenIndex, int endTokenIndex)
   {
      TokenRange = new Range(beginTokenIndex, endTokenIndex);
   }

   public IEnumerator<Node> GetEnumerator()
   {
      if (m_Children == null)
         return Enumerable.Empty<Node>().GetEnumerator();

      return m_Children.GetEnumerator();
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }

   public bool TryGetChildOfType<T>([NotNullWhen(true)] out T? child) where T : Node
   {
      foreach (Node node in this)
      {
         if (node is T t)
         {
            child = t;
            return true;
         }
      }

      child = null;
      return false;
   }

   public T? GetRoot<T>() where T : Node
   {
      Node node = this;
      while (node.Parent != null)
      {
         node = node.Parent;
      }

      return node as T;
   }

   public IEnumerable<T> GetChildrenOfType<T>() where T : Node
   {
      if (m_Children == null)
         return Enumerable.Empty<T>();

      return m_Children.OfType<T>();
   }

   public void AddChildren(IEnumerable<Node>? nodes)
   {
      if (nodes == null)
         return;

      foreach (Node node in nodes)
      {
         AddChild(node);
      }
   }

   public void AddChild(Node? node)
   {
      if (node == null)
         return;

      if (node.Parent != null)
         throw new InvalidOperationException("Node already has a parent.");

      m_Children ??= [];
      m_Children.Add(node);
      node.Parent = this;
   }
}
