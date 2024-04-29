using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UndefinedBuildTool;

public class PremakeFileGenerationContext
{
   public int IndentationLevel { get; private set; }

   readonly StringBuilder m_Output = new(1024);

   public string GetGeneratedOutput()
   {
      return m_Output.ToString();
   }

   public PremakeIndentationScope CreateIndentationScope()
   {
      return new PremakeIndentationScope(this);
   }

   public void AppendJoin<T>(IEnumerable<T> collection, string separator, Action<PremakeFileGenerationContext, T> appendElement)
   {
      IEnumerator<T> enumerator = collection.GetEnumerator();

      if (enumerator == null)
         return;

      if (enumerator.MoveNext())
      {
         appendElement(this, enumerator.Current);
         while (enumerator.MoveNext())
         {
            Append(separator);
            appendElement(this, enumerator.Current);
         }
      }
   }

   public int IncreaseIndentation()
   {
      return ++IndentationLevel;
   }

   public int DecreaseIndentation()
   {
      Debug.Assert(IndentationLevel >= 1);
      return --IndentationLevel;
   }

   public void AppendIndentation()
   {
      m_Output.Append(' ', IndentationLevel * 2);
   }

   public void Append(string value)
   {
      m_Output.Append(value);
   }

   public void AppendLine()
   {
      m_Output.AppendLine();
   }

   public void AppendLine(string value)
   {
      m_Output.AppendLine(value);
   }

   public void AppendFormat(string format, params object[] args)
   {
      m_Output.AppendFormat(format, args);
   }

   public void AppendFormatLine(string format, params object[] args)
   {
      m_Output.AppendFormat(format, args);
      m_Output.AppendLine();
   }

   public int GetLength()
   {
      return m_Output.Length;
   }

   public void Replace(int position, char value)
   {
      m_Output[position] = value;
   }

   public void Remove(int startIndex, int length)
   {
      m_Output.Remove(startIndex, length);
   }

   public void Remove(int length)
   {
      m_Output.Remove(m_Output.Length - length, length);
   }
}
