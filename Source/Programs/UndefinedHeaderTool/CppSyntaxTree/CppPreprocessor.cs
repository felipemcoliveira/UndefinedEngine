using System;
using System.Collections.Generic;

namespace UndefinedHeader.SyntaxTree;

public class CppPreprocessor
{
   private bool IsEndOfFile => m_ReadPosition >= m_SourceCode.Length;
   private char CurrentCharacter => m_SourceCode[m_ReadPosition];

   private readonly string m_FilePath;
   private readonly char[] m_SourceCode;
   private int m_ReadPosition;
   private int m_WritePosition;
   private int m_CurrentLine;
   private int m_CurrentLineStart;
   private readonly List<CppSouceCodeRange> m_Ranges;

   public CppPreprocessor(string rawSourceCode, string filePath)
   {
      m_SourceCode = rawSourceCode.ToCharArray();
      m_FilePath = filePath;
      m_Ranges = new(128);
   }

   public CppSourceCode Preprocess()
   {
      m_CurrentLine = 1;
      m_CurrentLineStart = 0;
      m_Ranges.Clear();
      AddRangeEntry();

      while (!IsEndOfFile)
      {
         if (TryConsumeReadOnly("//"))
         {
            while (!IsEndOfFile)
            {
               bool isLineSplicing = false;
               if (CurrentCharacter == '\n' || (isLineSplicing = TryConsumeLineSplicingReadOnly()))
               {
                  m_CurrentLine++;
                  m_CurrentLineStart = m_ReadPosition;
                  AddRangeEntry();

                  if (isLineSplicing)
                  {
                     continue;
                  }

                  break;
               }

               m_ReadPosition++;
            }

            continue;
         }

         if (TryConsumeReadOnly("/*"))
         {
            while (!IsEndOfFile)
            {
               if (TryConsumeReadOnly("*/"))
               {
                  break;
               }

               if (CurrentCharacter == '\n')
               {
                  m_CurrentLine++;
                  m_CurrentLineStart = m_ReadPosition;
               }

               m_ReadPosition++;
            }

            if (IsEndOfFile)
            {
               throw new CppIllFormedCodeException(m_ReadPosition, "Unterminated comment.");
            }

            continue;
         }

         if (TryConsumeLineSplicingReadOnly())
         {
            m_CurrentLine++;
            m_CurrentLineStart = m_ReadPosition;
            AddRangeEntry();
            continue;
         }

         if (TryConsume('\n'))
         {
            m_CurrentLine++;
            m_CurrentLineStart = m_ReadPosition;
            AddRangeEntry();
            continue;
         }

         m_SourceCode[m_WritePosition++] = CurrentCharacter;
         m_ReadPosition++;
      }

      string sourceCode = new(m_SourceCode, 0, m_WritePosition);
      return new CppSourceCode(sourceCode, m_FilePath, m_Ranges);
   }

   private bool TryConsumeLineSplicingReadOnly()
   {
      if (!IsEndOfFile && CurrentCharacter == '\\')
      {
         if (TryConsumeReadOnly("\\\r\n") || TryConsumeReadOnly("\\\n"))
         {
            return true;
         }
      }

      return false;
   }

   private void AddRangeEntry()
   {
      CppSouceCodeRange range = new(m_WritePosition, m_CurrentLine, m_ReadPosition - m_CurrentLineStart + 1);
      if (m_Ranges[^1].StartPosition == range.StartPosition)
      {
         m_Ranges[^1] = range;
         return;
      }

      m_Ranges.Add(new(m_WritePosition, m_CurrentLine, m_ReadPosition - m_CurrentLineStart + 1));
   }

   public bool TryConsume(char c)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         m_ReadPosition++;
         m_SourceCode[m_WritePosition++] = c;
         return true;
      }

      return false;
   }

   public bool TryConsume(ReadOnlySpan<char> value)
   {
      int newPosition = m_ReadPosition;
      while (!IsEndOfFile && m_SourceCode[newPosition] == value[newPosition - m_ReadPosition])
      {
         newPosition++;
         if (newPosition - m_ReadPosition == value.Length)
         {
            m_ReadPosition = newPosition;
            for (int i = 0; i < value.Length; i++)
            {
               m_SourceCode[m_WritePosition++] = value[i];
            }

            return true;
         }
      }

      return false;
   }

   public bool TryConsumeReadOnly(char c)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         m_ReadPosition++;
         return true;
      }

      return false;
   }

   public bool TryConsumeReadOnly(ReadOnlySpan<char> value)
   {
      int newPosition = m_ReadPosition;
      while (!IsEndOfFile && m_SourceCode[newPosition] == value[newPosition - m_ReadPosition])
      {
         newPosition++;
         if (newPosition - m_ReadPosition == value.Length)
         {
            m_ReadPosition = newPosition;
            return true;
         }
      }

      return false;
   }
}
