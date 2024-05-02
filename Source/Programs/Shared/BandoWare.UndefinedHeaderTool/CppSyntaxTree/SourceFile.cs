using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public class SourceFile
{
   public string RawContent { get; }
   public string Content { get; }
   public string FilePath { get; }

   private SourceFilePositionMap m_PositionMap;

   public SourceFile(string rawContent, string filePath)
   {
      SourceFileContentPreprocessor preprocessor = new(rawContent);
      SourceFilePreprocessResult preprocessResult = preprocessor.Preprocess();

      Content = preprocessResult.FileContent;
      RawContent = preprocessResult.RawFileContent;
      m_PositionMap = preprocessResult.PositionMap;
      FilePath = filePath;
   }

   public SourceFilePositionMapEntry GetPositionMapEntry(SourceFilePosition position)
   {
      return m_PositionMap.GetEntry(position);
   }
}

/// <summary>
/// On the first stage of source file parsing its rawContent is preprocessed
/// </summary>
public enum SourceFilePositionType
{
   /// <summary>
   /// A position before the file raw content has been processed
   /// </summary>
   /// <seealso cref="SourceFileContentPreprocessor"/>
   RawContentPosition,

   /// <summary>
   /// A possiton after the file raw content has been processed
   /// </summary>
   /// <seealso cref="SourceFileContentPreprocessor"/>
   ContentPosition
}

public record struct SourceFilePosition(SourceFilePositionType Type, int Value);

public record struct SourceFilePositionMapEntry(int ContentPosition, int RawContentPosition, int RawLine, int RawColumn);

internal class ContentPositionComparer : IComparer<SourceFilePositionMapEntry>
{
   public int Compare(SourceFilePositionMapEntry x, SourceFilePositionMapEntry y)
   {
      return x.ContentPosition.CompareTo(y.ContentPosition);
   }
}

internal class RawContentPositionComparer : IComparer<SourceFilePositionMapEntry>
{
   public int Compare(SourceFilePositionMapEntry x, SourceFilePositionMapEntry y)
   {
      return x.RawContentPosition.CompareTo(y.RawContentPosition);
   }
}

public class SourceFilePositionMap
{
   private static readonly ContentPositionComparer s_ContentPositionComparer = new();
   private static readonly RawContentPositionComparer s_RawContentPositionComparer = new();

   private readonly SourceFilePositionMapEntry[] m_Entries;

   internal SourceFilePositionMap(List<SourceFilePositionMapEntry> entries)
   {
      m_Entries = [.. entries];
   }

   public SourceFilePositionMapEntry GetEntry(SourceFilePosition position)
   {
      IComparer<SourceFilePositionMapEntry> comparer = position.Type switch
      {
         SourceFilePositionType.ContentPosition => s_ContentPositionComparer,
         SourceFilePositionType.RawContentPosition => s_RawContentPositionComparer,
         _ => throw new NotImplementedException(position.Type.ToString())
      };

      SourceFilePositionMapEntry compareEntry = new(position.Value, position.Value, 0, 0);
      int index = Array.BinarySearch(m_Entries, compareEntry, comparer);
      if (index < 0)
      {
         index = ~index - 1;
      }

      return m_Entries[index];
   }
}
