using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class SourceCode
{
   public string Content { get; }
   public string FilePath { get; }

   private readonly List<SouceCodeRange> m_Ranges;
   private int[]? m_RangeStartPositions;

   internal SourceCode(string content, string filePath, List<SouceCodeRange> ranges)
   {
      Content = content;
      FilePath = filePath;
      m_Ranges = ranges;
   }

   public (int LineNumber, int ColumnNumber) GetLineAndColumn(int position)
   {
      if (m_RangeStartPositions == null)
      {
         m_RangeStartPositions = new int[m_Ranges.Count];
         for (int i = 0; i < m_Ranges.Count; i++)
         {
            m_RangeStartPositions[i] = m_Ranges[i].StartPosition;
         }
      }

      int index = Array.BinarySearch(m_RangeStartPositions, position);
      if (index < 0)
      {
         index = ~index;
      }

      if (index >= m_Ranges.Count)
      {
         index = m_Ranges.Count - 1;
      }

      SouceCodeRange range = m_Ranges[index];
      return (range.Column, range.Column + (position - range.StartPosition));
   }
}
