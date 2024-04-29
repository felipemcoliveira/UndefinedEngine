using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


using Range = UndefinedCore.Range;

namespace UndefinedHeader.SyntaxTree;

public class CppSyntaxNode(Range tokensRange, CppLexicalAnalysis lexicalAnalysis) : IEnumerable<CppSyntaxNode>
{
   public Range TokensRange { get; set; } = tokensRange;

   public CppLexicalAnalysis LexicalAnalysis { get; } = lexicalAnalysis;
   public CppSyntaxNode? Parent { get; private set; }

   private List<CppSyntaxNode>? m_Children;

   public IEnumerator<CppSyntaxNode> GetEnumerator()
   {
      if (m_Children == null)
      {
         return Enumerable.Empty<CppSyntaxNode>().GetEnumerator();
      }

      return m_Children.GetEnumerator();
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }

   public bool TryGetChildOfType<T>([NotNullWhen(true)] out T? child) where T : CppSyntaxNode
   {
      foreach (CppSyntaxNode node in this)
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

   public IEnumerable<T> GetChildrenOfType<T>() where T : CppSyntaxNode
   {
      if (m_Children == null)
      {
         return Enumerable.Empty<T>();
      }

      return m_Children.Where(n => n is T).Cast<T>();
   }

   public void AddChildren(IEnumerable<CppSyntaxNode>? nodes)
   {
      if (nodes == null)
      {
         return;
      }

      foreach (CppSyntaxNode node in nodes)
      {
         AddChild(node);
      }
   }

   public void AddChild(CppSyntaxNode? node)
   {
      if (node == null)
      {
         return;
      }

      if (node.Parent != null)
      {
         throw new InvalidOperationException("Node already has a parent.");
      }

      m_Children ??= [];
      m_Children.Add(node);
      node.Parent = this;
   }
}
