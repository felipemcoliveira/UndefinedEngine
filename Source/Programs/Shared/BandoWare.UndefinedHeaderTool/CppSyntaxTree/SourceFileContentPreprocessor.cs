using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal record struct SourceFilePreprocessResult(string RawFileContent, string FileContent, SourceFilePositionMap PositionMap);

internal class SourceFileContentPreprocessor
{
   private enum ConsumeMode
   {
      Write = 0,
      Discard
   }

   private bool IsEndOfFile => m_RawContentPosition >= m_FileContent.Length;
   private char CurrentCharacter => m_RawFileContent[m_RawContentPosition];

   private readonly string m_RawFileContent;
   private readonly char[] m_FileContent;
   private int m_RawContentPosition;
   private int m_ContentPosition;
   private int m_CurrentLine;
   private int m_CurrentLineStart;
   private readonly List<SourceFilePositionMapEntry> m_PositionMapEntries;

   public SourceFileContentPreprocessor(string rawFileContent)
   {
      m_PositionMapEntries = new(rawFileContent.Length / 180);
      m_RawFileContent = rawFileContent;

      // it assumes file content length will always be equal or less than the raw file
      // content, because the preprocessing consist of only content stripping (like coments)
      m_FileContent = GC.AllocateUninitializedArray<char>(m_RawFileContent.Length);
   }

   public SourceFilePreprocessResult Preprocess()
   {
      m_CurrentLine = 0;
      m_CurrentLineStart = 0;
      m_PositionMapEntries.Clear();
      m_PositionMapEntries.Add(default);

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

               m_RawContentPosition++;
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

               m_RawContentPosition++;
            }

            if (!terminated)
            {
               throw CreateIllFormedCodeException(m_RawContentPosition, "Unterminated comment.");
            }

            AddPositionMapEntry();

            // multi line comment should be replace with a white space
            m_FileContent[m_ContentPosition++] = ' ';

            continue;
         }

         if (TryConsumeLineBreak() || TryConsumeLineSplicing())
         {
            continue;
         }

         m_FileContent[m_ContentPosition++] = CurrentCharacter;
         m_RawContentPosition++;
      }

      string fileContent = new(m_FileContent, 0, m_ContentPosition);
      SourceFilePositionMap positionMap = new(m_PositionMapEntries);
      return new(m_RawFileContent, fileContent, positionMap);
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
      m_CurrentLineStart = m_RawContentPosition;
      AddPositionMapEntry();
   }

   private void AddPositionMapEntry()
   {
      int column = m_RawContentPosition - m_CurrentLineStart;
      if (m_PositionMapEntries[^1].ContentPosition == m_ContentPosition)
      {
         m_PositionMapEntries[^1] = new(m_ContentPosition, m_RawContentPosition, m_CurrentLine, column);
         return;
      }

      m_PositionMapEntries.Add(new(m_ContentPosition, m_RawContentPosition, m_CurrentLine, column));
   }

   private bool TryConsume(char c, ConsumeMode mode = ConsumeMode.Write)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         m_RawContentPosition++;
         if (mode == ConsumeMode.Write)
         {
            m_FileContent[m_ContentPosition++] = c;
         }

         return true;
      }

      return false;
   }

   private bool TryConsume(ReadOnlySpan<char> value, ConsumeMode mode = ConsumeMode.Write)
   {
      int newPosition = m_RawContentPosition;
      while (!IsEndOfFile && m_FileContent[newPosition] == value[newPosition - m_RawContentPosition])
      {
         newPosition++;
         if (newPosition - m_RawContentPosition == value.Length)
         {
            m_RawContentPosition = newPosition;

            if (mode == ConsumeMode.Write)
            {
               for (int i = 0; i < value.Length; i++)
               {
                  m_FileContent[m_ContentPosition++] = value[i];
               }
            }

            return true;
         }
      }

      return false;
   }

   private static IllFormedCodeException CreateIllFormedCodeException(int contentPosition, string message)
   {
      SourceFilePosition position = new(SourceFilePositionType.RawContentPosition, contentPosition);
      return new IllFormedCodeException(position, message);
   }
}
