using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class SyntaxNode : IEnumerable<SyntaxNode>
{
   public SyntaxNode? Parent { get; private set; }

   private List<SyntaxNode>? m_Children;

   public IEnumerator<SyntaxNode> GetEnumerator()
   {
      if (m_Children == null)
         return Enumerable.Empty<SyntaxNode>().GetEnumerator();

      return m_Children.GetEnumerator();
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }

   public bool TryGetChildOfType<T>([NotNullWhen(true)] out T? child) where T : SyntaxNode
   {
      foreach (SyntaxNode node in this)
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

   public IEnumerable<T> GetChildrenOfType<T>() where T : SyntaxNode
   {
      if (m_Children == null)
         return Enumerable.Empty<T>();

      return m_Children.OfType<T>();
   }

   public void AddChildren(IEnumerable<SyntaxNode>? nodes)
   {
      if (nodes == null)
         return;

      foreach (SyntaxNode node in nodes)
      {
         AddChild(node);
      }
   }

   public void AddChild(SyntaxNode? node)
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
