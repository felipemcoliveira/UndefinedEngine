using System;
using System.Collections;
using System.Collections.Generic;

namespace UndefinedBuildTool;

public abstract class PremakeNode : IEnumerable<PremakeNode>
{
   public IReadOnlyList<PremakeNode> Children => m_Children;

   protected readonly static string CommaWithNewLine = "," + Environment.NewLine;
   readonly List<PremakeNode> m_Children = [];

   public abstract void Write(PremakeFileGenerationContext ctx);

   public void Add(PremakeNode node)
   {
      m_Children.Add(node);
   }

   public bool GetFirstNodeOfType<T>(out T? node, out int index) where T : class
   {
      for (int i = 0; i < m_Children.Count; i++)
      {
         PremakeNode child = m_Children[i];
         if (child is T t)
         {
            node = t;
            index = i;
            return true;
         }
      }

      index = -1;
      node = null;
      return false;
   }

   public bool GetFirstNodeOfType<T>(out T? node) where T : class
   {
      return GetFirstNodeOfType(out node, out _);
   }

   public IEnumerator<PremakeNode> GetEnumerator()
   {
      return m_Children.GetEnumerator();
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
}
