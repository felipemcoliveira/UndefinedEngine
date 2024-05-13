using System;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal record struct SourceFilePreprocessResult
(
   string RawSourceFileText,
   string ProcessedSourceFileText,
   SourceFileTextPositionMap PositionMap
);

internal class SourceFileContentPreprocessor
{
   private enum ConsumeMode
   {
      Write = 0,
      Discard
   }

   private bool IsEndOfFile => m_RawTextPosition >= m_ProcessedText.Length;
   private char CurrentCharacter => m_RawText[m_RawTextPosition];
   private int CurrentColumn => m_RawTextPosition - m_CurrentLineStart + 1;

   private readonly string m_SourceFilePath;
   private readonly string m_RawText;
   private readonly char[] m_ProcessedText;
   private int m_RawTextPosition;
   private int m_ProcessedTextPosition;
   private int m_CurrentLine;
   private int m_CurrentLineStart;
   private SourceFileTextPositionMap m_TextPositionMap;

   public SourceFileContentPreprocessor(string sourceFileText, string sourceFilePath, SourceFileTextPositionMap sourceFileTextPosition)
   {
      ArgumentNullException.ThrowIfNull(sourceFileText);
      ArgumentNullException.ThrowIfNull(sourceFilePath);
      ArgumentNullException.ThrowIfNull(sourceFileTextPosition);

      m_SourceFilePath = sourceFilePath;
      m_RawText = sourceFileText;
      m_TextPositionMap = sourceFileTextPosition;

      // it assumes processed text length will always be equal or less than the raw file
      // text, because the preprocessing consist of only text stripping (like coments)
      m_ProcessedText = GC.AllocateUninitializedArray<char>(m_RawText.Length);
   }

   public SourceFilePreprocessResult Preprocess()
   {
      m_CurrentLine = 1;
      m_CurrentLineStart = 0;

      while (!IsEndOfFile)
      {
         // single-line comment
         if (TryConsume("//", ConsumeMode.Discard))
         {
            while (!IsEndOfFile)
            {
               // i'm dedicating this comment to my hatred to every developer
               // who has ever used line splicing in a "single-line" comment
               // (furious emoji here)
               if (TryConsumeLineSplicing())
               {
                  continue;
               }

               if (TryConsumeLineBreak())
               {
                  break;
               }

               m_RawTextPosition++;
            }

            AddPositionMapEntry();
            continue;
         }

         // multi-line comment
         if (TryConsume("/*", ConsumeMode.Discard))
         {
            bool terminated = false;
            while (!IsEndOfFile)
            {
               if (terminated = TryConsume("*/", ConsumeMode.Discard))
               {
                  break;
               }

               if (TryConsumeLineBreak(ConsumeMode.Discard) || TryConsumeLineSplicing())
               {
                  continue;
               }

               m_RawTextPosition++;
            }

            if (!terminated)
            {
               throw new IllFormedCodeException(m_CurrentLine, CurrentColumn, m_SourceFilePath, m_RawText, "Unterminated comment.");
            }

            AddPositionMapEntry();

            // multi line comment should be replace with a white space
            m_ProcessedText[m_ProcessedTextPosition++] = ' ';

            continue;
         }

         if (TryConsumeLineBreak() || TryConsumeLineSplicing())
         {
            continue;
         }

         m_ProcessedText[m_ProcessedTextPosition++] = CurrentCharacter;
         m_RawTextPosition++;
      }

      string fileContent = new(m_ProcessedText, 0, m_ProcessedTextPosition);
      return new(m_RawText, fileContent, m_TextPositionMap);
   }

   private bool TryConsumeLineBreak(ConsumeMode mode = ConsumeMode.Write)
   {
      if (!TryConsume('\n', mode) && !TryConsume("\r\n", mode))
      {
         return false;
      }

      OnLineBreak();
      return true;
   }

   private bool TryConsumeLineSplicing()
   {
      if (!TryConsume("\\\r\n", ConsumeMode.Discard) && !TryConsume("\\\n", ConsumeMode.Discard))
      {
         return false;
      }

      OnLineBreak();
      return true;
   }

   private void OnLineBreak()
   {
      m_CurrentLine++;
      m_CurrentLineStart = m_RawTextPosition;
      AddPositionMapEntry();
   }

   private void AddPositionMapEntry()
   {
      m_TextPositionMap.AddEntry(m_ProcessedTextPosition, m_RawTextPosition, m_CurrentLine, m_RawTextPosition - m_CurrentLineStart);
   }

   private bool TryConsume(char c, ConsumeMode mode = ConsumeMode.Write)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         m_RawTextPosition++;
         if (mode == ConsumeMode.Write)
         {
            m_ProcessedText[m_ProcessedTextPosition++] = c;
         }

         return true;
      }

      return false;
   }

   private bool TryConsume(ReadOnlySpan<char> value, ConsumeMode mode = ConsumeMode.Write)
   {
      int newPosition = m_RawTextPosition;
      while (!IsEndOfFile && m_RawText[newPosition] == value[newPosition - m_RawTextPosition])
      {
         newPosition++;
         if (newPosition - m_RawTextPosition == value.Length)
         {
            m_RawTextPosition = newPosition;

            if (mode == ConsumeMode.Write)
            {
               for (int i = 0; i < value.Length; i++)
               {
                  m_ProcessedText[m_ProcessedTextPosition++] = value[i];
               }
            }

            return true;
         }
      }

      return false;
   }
}
