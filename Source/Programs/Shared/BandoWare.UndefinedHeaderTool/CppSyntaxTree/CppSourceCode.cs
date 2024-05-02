using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class CppSourceCode
{
   public string SourceCode { get; }
   public string FilePath { get; }

   private readonly List<CppSouceCodeRange> m_Ranges;
   private int[]? m_RangeStartPositions;

   internal CppSourceCode(string sourceCode, string filePath, List<CppSouceCodeRange> ranges)
   {
      SourceCode = sourceCode;
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

      CppSouceCodeRange range = m_Ranges[index];
      return (range.Column, range.Column + (position - range.StartPosition));
   }
}