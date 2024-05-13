using System;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[Serializable]
public class IllFormedCodeException : Exception
{
   public int Line { get; set; }
   public int Column { get; set; }
   public string SourceFilePath { get; set; }
   public string SourceFileText { get; set; }

   internal IllFormedCodeException(int line, int column, string sourceFilePath, string sourceFileText, string message)
   : base(message)
   {
      Line = line;
      Column = column;
      SourceFilePath = sourceFilePath;
      SourceFileText = sourceFileText;
   }

   internal IllFormedCodeException
   (
      int position,
      SourceFileTextPositionType textPositionType,
      SourceFileTextPositionMap textPositionMap,
      string sourceFilePath,
      string sourceFileText,
      string message
   ) : base(message)
   {
      SourceFileTextPosition textPosition = new(textPositionType, position);
      textPositionMap.GetLineAndColumn(textPosition, out int line, out int column);

      Line = line;
      Column = column;
      SourceFilePath = sourceFilePath;
      SourceFileText = sourceFileText;
   }
}

