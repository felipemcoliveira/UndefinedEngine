using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

/// <summary>
/// On the first stage of source file parsing its rawContent is preprocessed
/// </summary>
public enum SourceFileTextPositionType
{
   /// <summary>
   /// A position before the file raw text has been processed
   /// </summary>
   /// <seealso cref="SourceFileContentPreprocessor"/>
   RawTextPosition,

   /// <summary>
   /// A possiton after the file raw text has been processed
   /// </summary>
   /// <seealso cref="SourceFileContentPreprocessor"/>
   TextPosition
}

public record struct SourceFileTextPosition(SourceFileTextPositionType Type, int Value);

public record struct SourceFileTextPositionMapEntry(int TextPosition, int RawTextPosition, int Line, int Column);

internal class TextPositionComparer : IComparer<SourceFileTextPositionMapEntry>
{
   public int Compare(SourceFileTextPositionMapEntry x, SourceFileTextPositionMapEntry y)
   {
      return x.TextPosition.CompareTo(y.TextPosition);
   }
}

internal class RawTextPositionComparer : IComparer<SourceFileTextPositionMapEntry>
{
   public int Compare(SourceFileTextPositionMapEntry x, SourceFileTextPositionMapEntry y)
   {
      return x.RawTextPosition.CompareTo(y.RawTextPosition);
   }
}

internal class SourceFileTextPositionMap
{
   private static readonly TextPositionComparer s_TextPositionComparer = new();
   private static readonly RawTextPositionComparer s_RawTextPositionComparer = new();

   private readonly List<SourceFileTextPositionMapEntry> m_Entries = new(128);

   internal SourceFileTextPositionMap()
   {
      m_Entries.Add(new SourceFileTextPositionMapEntry(0, 0, 1, 1));
   }

   internal void AddEntry(int textPosition, int rawTextPosition, int line, int column)
   {
      if (m_Entries[^1].TextPosition == textPosition)
      {
         m_Entries[^1] = new SourceFileTextPositionMapEntry(textPosition, rawTextPosition, line, column);
         return;
      }

      m_Entries.Add(new SourceFileTextPositionMapEntry(textPosition, rawTextPosition, line, column));
   }

   public void GetLineAndColumn(SourceFileTextPosition position, out int line, out int column)
   {
      int entryIndex = GetEntryIndex(position);

      int columnOffset = position.Type switch
      {
         SourceFileTextPositionType.TextPosition => position.Value - m_Entries[entryIndex].TextPosition,
         SourceFileTextPositionType.RawTextPosition => position.Value - m_Entries[entryIndex].RawTextPosition,
         _ => throw new NotImplementedException(position.Type.ToString())
      };

      line = m_Entries[entryIndex].Line;
      column = m_Entries[entryIndex].Column + columnOffset;
   }

   private int GetEntryIndex(SourceFileTextPosition position)
   {
      IComparer<SourceFileTextPositionMapEntry> comparer = position.Type switch
      {
         SourceFileTextPositionType.TextPosition => s_TextPositionComparer,
         SourceFileTextPositionType.RawTextPosition => s_RawTextPositionComparer,
         _ => throw new NotImplementedException(position.Type.ToString())
      };

      SourceFileTextPositionMapEntry compareEntry = new(position.Value, position.Value, 0, 0);
      int index = m_Entries.BinarySearch(compareEntry, comparer);
      if (index < 0)
      {
         index = ~index - 1;
      }

      return index;
   }
}
