using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class SourceFile
{
   public string RawContent { get; }
   public string Content { get; }
   public string FilePath { get; }

   private SourceFileTextPositionMap m_TextPositionMap;

   public SourceFile(string rawContent, string filePath)
   {
      SourceFileContentPreprocessor preprocessor = new(rawContent);
      SourceFilePreprocessResult preprocessResult = preprocessor.Preprocess();

      Content = preprocessResult.FileContent;
      RawContent = preprocessResult.RawFileContent;
      m_TextPositionMap = preprocessResult.PositionMap;
      FilePath = filePath;
   }

   public SourceFileTextPositionMapEntry GetTextPositionMapEntry(SourceFileTextPosition position)
   {
      return m_TextPositionMap.GetEntry(position);
   }
}

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

public record struct SourceFileTextPositionMapEntry(int TextPosition, int RawTextPosition, int RawLineOffset, int RawColumnOffset);

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

public class SourceFileTextPositionMap
{
   private static readonly TextPositionComparer s_TextPositionComparer = new();
   private static readonly RawTextPositionComparer s_RawTextPositionComparer = new();

   private readonly SourceFileTextPositionMapEntry[] m_Entries;

   internal SourceFileTextPositionMap(List<SourceFileTextPositionMapEntry> entries)
   {
      m_Entries = [.. entries];
   }

   public SourceFileTextPositionMapEntry GetEntry(SourceFileTextPosition position)
   {
      IComparer<SourceFileTextPositionMapEntry> comparer = position.Type switch
      {
         SourceFileTextPositionType.TextPosition => s_TextPositionComparer,
         SourceFileTextPositionType.RawTextPosition => s_RawTextPositionComparer,
         _ => throw new NotImplementedException(position.Type.ToString())
      };

      SourceFileTextPositionMapEntry compareEntry = new(position.Value, position.Value, 0, 0);
      int index = Array.BinarySearch(m_Entries, compareEntry, comparer);
      if (index < 0)
      {
         index = ~index - 1;
      }

      return m_Entries[index];
   }
}
