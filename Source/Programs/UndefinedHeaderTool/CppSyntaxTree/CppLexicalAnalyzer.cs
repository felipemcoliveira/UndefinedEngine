using System;
using System.Collections.Generic;
using System.Linq;

namespace UndefinedHeader.SyntaxTree;

internal class CppLexicalAnalyzer
{
   public int EndOfFilePosition => m_SourceCode.Length;
   public bool IsEndOfFile => CurrentPosition >= EndOfFilePosition;
   public char CurrentCharacter => m_SourceCode[CurrentPosition];

   internal int CurrentPosition { get; set; }

   private static readonly Dictionary<int, (string Identifier, CppTokenType Type)> s_AllCppKeywords;
   private static readonly HashSet<char> s_SymbolCharacters;
   private static readonly StringHashSet s_AllSymbols;

   private readonly string m_SourceCode;

   public CppLexicalAnalyzer(string sourceCode)
   {
      m_SourceCode = sourceCode;
      CurrentPosition = 0;
   }

   static CppLexicalAnalyzer()
   {
      s_AllCppKeywords = new
      ([
         CreateSpecialIdentifierEntry("alignas", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("alignof", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("and", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("and_eq", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("asm", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("atomic_cancel", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("atomic_commit", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("atomic_noexcept", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("auto", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("bitand", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("bitor", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("bool", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("break", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("case", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("catch", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("char", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("char8_t", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("char16_t", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("char32_t", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("class", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("compl", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("concept", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("const", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("consteval", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("constexpr", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("constinit", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("const_cast", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("continue", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("co_await", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("co_return", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("co_yield", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("decltype", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("default", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("delete", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("do", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("double", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("dynamic_cast", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("else", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("enum", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("explicit", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("export", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("extern", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("float", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("for", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("friend", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("goto", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("if", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("inline", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("int", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("long", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("mutable", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("namespace", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("new", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("noexcept", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("not", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("not_eq", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("operator", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("or", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("or_eq", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("private", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("protected", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("public", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("reflexpr", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("register", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("reinterpret_cast", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("requires", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("return", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("short", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("signed", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("sizeof", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("static", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("static_assert", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("static_cast", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("struct", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("switch", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("synchronized", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("template", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("this", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("thread_local", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("throw", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("try", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("typedef", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("typeid", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("typename", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("union", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("unsigned", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("using", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("virtual", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("void", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("volatile", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("wchar_t", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("while", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("xor", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("xor_eq", CppTokenType.Keyword),
         CreateSpecialIdentifierEntry("true", CppTokenType.BooleanLiteral),
         CreateSpecialIdentifierEntry("false", CppTokenType.BooleanLiteral),
         CreateSpecialIdentifierEntry("nullptr", CppTokenType.PointerLiteral),
         CreateSpecialIdentifierEntry("UCLASS", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("UENUM", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("USTRUCT", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("UMETHOD", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("UMETA", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("UPROPERTY", CppTokenType.EngineHeader),
         CreateSpecialIdentifierEntry("UFUNCTION", CppTokenType.EngineHeader),
      ]);

      s_AllSymbols =
      [
         "+",
         "-",
         "*",
         "/",
         "%",
         "++",
         "--",
         "=",
         "+=",
         "-=",
         "*=",
         "/=",
         "%=",
         "<<=",
         ">>=",
         "&=",
         "^=",
         "|=",
         "==",
         "!=",
         "<",
         ">",
         "<=",
         "&&",
         "||",
         "!",
         "|",
         "&",
         "^",
         "~",
         "<<",
         ">>",
         "::",
         "(",
         ")",
         "[",
         "]",
         "{",
         "}",
         ";",
         ",",
         ":",
         "...",
         "->",
         "->*",
         ".",
         ".*",
         "?",
      ];

      s_SymbolCharacters = s_AllSymbols!
       .SelectMany(symbol => symbol)
       .ToHashSet();
   }

   public CppLexicalAnalysis Analyze()
   {
      List<CppToken> tokens = new(20 / m_SourceCode.Length);
      List<int> engineHeaderTokenIndices = new(5);

      CurrentPosition = 0;

      while (!IsEndOfFile)
      {
         switch (CurrentCharacter)
         {
            // preprocessor directive
            case '#':
            {
               // ignore preprocessor directive
               ConsumeLine();
               break;
            }

            // comment and "/" operators
            case '/':
            {
               if (TryConsumeComment())
               {
                  break;
               }

               // it will also consume the current character
               TryConsumeSymbol(out int operatorStart, out int operatorLength);
               tokens.Add(new(operatorStart, operatorLength, CppTokenType.Symbol));
               break;
            }

            // white spaces (I dont really know if all of these are really supported by C++ but I'm adding them anyway)
            case '\u0020': // Space (the most common whitespace character)
            case '\u0009': // Character Tabulation or Horizontal Tab
            case '\u000A': // Line Feed
            case '\u000B': // Line Tabulation or Vertical Tab
            case '\u000C': // Form Feed
            case '\u000D': // Carriage Return
            case '\u00A0': // No-Break Space (non-breaking space)
            case '\u1680': // Ogham Space Mark (used in Ogham script)
            case '\u180E': // Mongolian Vowel Separator (deprecated in Unicode 6.3.0 as a space character)
            case '\u2000': // En Quad (space equal to the current font size)
            case '\u2001': // Em Quad (space equal to the current font size; not listed in your sequence, but related)
            case '\u2002': // En Space (half the width of an em space)
            case '\u2003': // Em Space (space equal to the current font size)
            case '\u2004': // Three-Per-Em Space (not listed in your sequence, but related)
            case '\u2005': // Four-Per-Em Space (not listed in your sequence, but related)
            case '\u2006': // Six-Per-Em Space (not listed in your sequence, but related)
            case '\u2007': // Figure Space (space equal to the width of a digit)
            case '\u2008': // Punctuation Space (space equal to the width of a period)
            case '\u2009': // Thin Space (thinner space)
            case '\u200A': // Hair Space (thinnest space)
            case '\u200B': // Zero Width Space (a space that doesn't take up space)
            case '\u202F': // Narrow No-Break Space (narrower version of a no-break space)
            case '\u205F': // Medium Mathematical Space (used in mathematical formulae)
            case '\u2028': // Line Separator
            case '\u2029': // Paragraph Separator
            case '\u3000': // Ideographic Space (used in CJK scripts)
            {
               // it will not only consume the current whitespace character but also all the following ones
               ConsumeWhitespaces();
               break;
            }

            // symbols
            case '(':
            case ')':
            case '[':
            case ']':
            case '{':
            case '}':
            case ';':
            case ',':
            case '!':
            case '%':
            case '&':
            case '*':
            case '-':
            case '+':
            // case '.': -- This case is already handled by the case that also handles numbers
            case '<':
            case '=':
            case '>':
            case '?':
            case '^':
            case '|':
            case '~':
            case ':':
            // case '/': -- This case is already handled by the case that also handles comments
            {
               // it will also consume the current character
               TryConsumeSymbol(out int operatorStart, out int operatorLength);
               tokens.Add(new(operatorStart, operatorLength, CppTokenType.Symbol));
               break;
            }

            case '.':
            {
               int startPosition = CurrentPosition;
               if (TryConsumeNumberLiteral())
               {
                  tokens.Add(new(startPosition, CurrentPosition - startPosition, CppTokenType.NumericLiteral));
                  break;
               }

               // it will always return true since the dot is a symbol
               TryConsumeSymbol(out int operatorStart, out int operatorLength);
               tokens.Add(new(operatorStart, operatorLength, CppTokenType.Symbol));
               break;
            }

            case '"':
            {
               int startPosition = CurrentPosition;
               ConsumeStringLiteral(false);
               tokens.Add(new(startPosition, CurrentPosition - startPosition, CppTokenType.StringLiteral));
               break;
            }

            case >= '0' and <= '9':
            {
               int startPosition = CurrentPosition;
               if (TryConsumeNumberLiteral())
               {
                  tokens.Add(new(startPosition, CurrentPosition - startPosition, CppTokenType.NumericLiteral));
                  break;
               }

               throw new CppIllFormedCodeException(CurrentPosition, "Unexpected character.");
            }

            case '_':
            case >= 'a' and <= 'z':
            case >= 'A' and <= 'Z':
            {
               int startPosition = CurrentPosition;
               TryConsumeIdentifier(out var identifier);

               if (TryConsume('"'))
               {
                  ConsumeStringLiteral(identifier[^-1] == 'R');
                  tokens.Add(new(startPosition, CurrentPosition - startPosition, CppTokenType.StringLiteral));
                  break;
               }

               if (TryConsume('\''))
               {
                  ConsumeCharLiteral();
                  tokens.Add(new(startPosition, CurrentPosition - startPosition, CppTokenType.CharacterLiteral));
                  break;
               }

               var type = GetIdentifierTokenType(identifier);
               if (type == CppTokenType.EngineHeader)
               {
                  engineHeaderTokenIndices.Add(tokens.Count);
               }

               tokens.Add(new(startPosition, CurrentPosition - startPosition, type));
               break;
            }

            default:
            {
               throw new CppIllFormedCodeException(CurrentPosition, "Unexpected character.");
            }
         }
      }

      return new(tokens, engineHeaderTokenIndices, m_SourceCode);
   }

   internal bool TryConsumeNumberLiteral()
   {
      return CppNumericLiteralLexicalAnalysis.TryConsume(this);
   }

   internal bool TryConsumeNumericLiteralDigits(int numericBase)
   {
      int start = CurrentPosition;
      while (!IsEndOfFile)
      {
         int digit = AsciiHexDigitToInt(CurrentCharacter);
         if (digit == -1 || digit >= numericBase)
         {
            break;
         }

         ConsumeCharacter();
      }

      if (!IsEndOfFile && char.IsDigit(CurrentCharacter))
      {
         throw new CppIllFormedCodeException(CurrentPosition, "Invalid digit base in number literal.");
      }

      return CurrentPosition != start;
   }

   internal static KeyValuePair<int, (string Identifier, CppTokenType Type)> CreateSpecialIdentifierEntry(string identifier, CppTokenType type)
   {
      if (type == CppTokenType.EngineHeader && !identifier.StartsWith('U'))
      {
         throw new ArgumentException("Engine identifiers must start with 'U'.");
      }

      return new(identifier.GetHashCode(), (identifier, type));
   }

   private static KeyValuePair<int, string> CreateEngineIdentifierEntry(string identifier)
   {
      if (!identifier.StartsWith('U'))
      {
         throw new ArgumentException("Engine identifiers must start with 'U'.");
      }

      return new(identifier.GetHashCode(), identifier);
   }

   internal void ConsumeCharLiteral()
   {
      if (!TryConsume('\''))
      {
         throw new CppIllFormedCodeException(CurrentPosition, "Invalid character literal.");
      }

      if (TryConsume('\\'))
      {
         ValidateIsNotEndOfFile("Unterminated character literal.");
         switch (CurrentCharacter)
         {
            case 'a':
            case 'b':
            case 'f':
            case 'n':
            case 'r':
            case 't':
            case 'v':
            case '\\':
            case '\'':
            case '"':
            case '?':
            case '0':
            {
               ConsumeCharacter();
               break;
            }
            case 'u':
            case 'U':
            {
               ConsumeCharacter();
               while (!IsEndOfFile && char.IsAsciiHexDigit(CurrentCharacter))
               {
                  ConsumeCharacter();
               }

               if (!TryConsume('\''))
               {
                  throw new CppIllFormedCodeException(CurrentPosition, "Unterminated character literal.");
               }

               break;
            }
            default:
            {
               throw new CppIllFormedCodeException(CurrentPosition, "Invalid escape sequence.");
            }
         }

         ConsumeCharacter();
         if (!TryConsume('\''))
         {
            throw new CppIllFormedCodeException(CurrentPosition, "Unterminated character literal.");
         }
      }
   }

   internal void ConsumeStringLiteral(bool isRawLiteralString)
   {
      if (isRawLiteralString)
      {
         ConsumeRawLiteralString();
         return;
      }

      ConsumeStringLiteral();
   }

   /// <summary>
   /// This method consumes a raw literal string. The method assumes that the
   /// current position is at the beginning (first quote) of the raw literal
   /// string.
   /// </summary>
   /// <param name="start">Position of the first quote.</param>
   /// <exception cref="CppIllFormedCodeException">Thrown when the syntax of the
   ///    raw string literal does not conform to C++ standards. This could be
   ///    due to an incorrect start (missing opening quote), improper delimiter
   ///    usage, or if the raw string literal is not properly terminated before
   ///    the end of the file is reached.</exception>
   /// <remarks>
   /// This method should be used after the literal prefix "R" has been
   /// consumed.
   /// </remarks>
   internal void ConsumeRawLiteralString()
   {
      if (!TryConsume('"'))
      {
         throw new CppIllFormedCodeException(CurrentPosition, "Invalid raw string literal.");
      }

      int quoteCount = 1;
      while (!IsEndOfFile)
      {
         if (TryConsume('"'))
         {
            quoteCount++;
         }

         break;
      }

      TryConsumeIdentifier(out var delimiter);
      if (!TryConsume('('))
      {
         throw new CppIllFormedCodeException(CurrentPosition, "Invalid raw string literal.");
      }

      while (!IsEndOfFile)
      {
         int closeDelimiterStart = CurrentPosition;
         if (TryConsume(')') && TryConsume(delimiter) && TryConsume('"', quoteCount))
         {
            return;
         }

         CurrentPosition = closeDelimiterStart;
         ConsumeCharacter();
      }

      throw new CppIllFormedCodeException(CurrentPosition, "Unterminated string literal.");
   }

   internal void ConsumeStringLiteral()
   {
      if (!TryConsume('"'))
      {
         throw new CppIllFormedCodeException(CurrentPosition, "Invalid string literal.");
      }

      while (!IsEndOfFile)
      {
         if (CurrentCharacter == '"' && !IsCharacterAt(CurrentPosition - 1, '\\'))
         {
            ConsumeCharacter();
            return;
         }

         ConsumeCharacter();
      }
   }

   internal bool TryConsumeSymbol(out int start, out int length)
   {
      start = CurrentPosition;
      length = Math.Min(3, EndOfFilePosition - CurrentPosition);

      while (length > 0)
      {
         var symbol = m_SourceCode.AsSpan().Slice(start, length);
         if (s_AllSymbols.Contains(symbol))
         {
            CurrentPosition += length;
            return true;
         }

         length--;
      }

      return false;
   }

   internal bool TryConsumeDigit(int numericBase)
   {
      int digit = AsciiHexDigitToInt(CurrentCharacter);
      if (digit == -1 || digit >= numericBase)
      {
         return false;
      }

      ConsumeCharacter();
      return true;
   }

   internal static bool IsSymbolCharacter(char c)
   {
      return s_SymbolCharacters.Contains(c);
   }

   internal void ConsumeWhitespaces()
   {
      while (!IsEndOfFile && char.IsWhiteSpace(CurrentCharacter))
      {
         ConsumeCharacter();
      }
   }

   internal static CppTokenType GetIdentifierTokenType(ReadOnlySpan<char> identifier)
   {
      int hashCode = string.GetHashCode(identifier);
      if (!s_AllCppKeywords.TryGetValue(hashCode, out var entry))
      {
         return CppTokenType.Identifier;
      }

      if (!MemoryExtensions.Equals(entry.Identifier.AsSpan(), identifier, StringComparison.Ordinal))
      {
         return CppTokenType.Identifier;
      }

      return entry.Type;
   }

   internal bool TryConsumeIdentifier(out ReadOnlySpan<char> identifier)
   {
      bool value = TryConsumeIdentifier(out int start, out int length);
      identifier = m_SourceCode.AsSpan().Slice(start, length);
      return value;
   }

   internal bool TryConsumeIdentifier(out int start, out int length)
   {
      start = CurrentPosition;
      length = 0;
      if (IsEndOfFile || (CurrentCharacter != '_' && !char.IsLetter(CurrentCharacter)))
      {
         return false;
      }

      while (!IsEndOfFile && (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_'))
      {
         ConsumeCharacter();
      }

      length = CurrentPosition - start;
      return true;
   }

   internal bool TryConsumeComment()
   {
      return TryConsumeSingleLineComment() || TryConsumeMultilineComment();
   }

   internal bool TryConsumeSingleLineComment()
   {
      if (!TryConsume("//"))
      {
         return false;
      }

      ConsumeLine();
      return true;
   }

   internal bool TryConsumeMultilineComment()
   {
      if (!TryConsume("/*"))
      {
         return false;
      }

      int openCommendPosition = CurrentPosition;
      while (!IsEndOfFile)
      {
         if (TryConsume("*/"))
         {
            return true;
         }

         ConsumeCharacter();
      }

      throw new CppIllFormedCodeException(CurrentPosition, "Unterminated comment");
   }

   internal bool TryConsumeSingleNumericSign()
   {
      while (CurrentCharacter is '+' or '-')
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   internal bool TryConsume(char c)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   internal bool TryConsumeIgnoreCase(char c)
   {
      if (!IsEndOfFile && char.ToUpper(CurrentCharacter) == char.ToUpper(c))
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   internal bool TryConsume(char c, int count)
   {
      int newPosition = CurrentPosition;
      while (newPosition < EndOfFilePosition && m_SourceCode[newPosition] == c)
      {
         newPosition++;
         if (newPosition - CurrentPosition == count)
         {
            CurrentPosition = newPosition;
            return true;
         }
      }

      return false;
   }

   internal bool TryConsume(ReadOnlySpan<char> token)
   {
      int newPosition = CurrentPosition;
      while (newPosition < EndOfFilePosition && m_SourceCode[newPosition] == token[newPosition - CurrentPosition])
      {
         newPosition++;
         if (newPosition - CurrentPosition == token.Length)
         {
            CurrentPosition = newPosition;
            return true;
         }
      }

      return false;
   }

   internal bool TryConsumeIgnoreCase(ReadOnlySpan<char> token)
   {
      int newPosition = CurrentPosition;
      while (newPosition < EndOfFilePosition && char.ToUpper(m_SourceCode[newPosition]) == char.ToUpper(token[newPosition - CurrentPosition]))
      {
         newPosition++;
         if (newPosition - CurrentPosition == token.Length)
         {
            CurrentPosition = newPosition;
            return true;
         }
      }

      return false;
   }

   internal bool IsCharacterAt(int position, char c)
   {
      if (position < m_SourceCode.Length)
      {
         return m_SourceCode[position] == c;
      }

      return false;
   }

   internal bool PeekNextCharacter(out char character)
   {
      if (CurrentPosition + 1 < EndOfFilePosition)
      {
         character = m_SourceCode[CurrentPosition + 1];
         return true;
      }

      character = default;
      return false;
   }

   internal void ConsumeLine()
   {
      while (!IsEndOfFile)
      {
         if (TryConsumeNewLineCharacters())
         {
            break;
         }

         ConsumeCharacter();
      }
   }

   internal bool TryConsumeNewLineCharacters()
   {
      // CRLF
      if (CurrentCharacter == '\r' && IsCharacterAt(CurrentPosition + 1, '\n'))
      {
         CurrentPosition += 2;
         return true;
      }

      if (CurrentCharacter is '\n' or '\r')
      {
         bool isPrevCharCR = CurrentCharacter == '\r';
         ConsumeCharacter();

         if (CurrentCharacter is '\n' && isPrevCharCR)
         {
            ConsumeCharacter();
         }

         return true;
      }

      return false;
   }

   internal void ConsumeCharacter()
   {
      CurrentPosition++;
   }

   private void ValidateIsNotEndOfFile(string message)
   {
      if (IsEndOfFile)
      {
         throw new CppIllFormedCodeException(CurrentPosition, message);
      }
   }

   private static KeyValuePair<int, string> CreateHashStringKeyValuePair(string str)
   {
      return new(str.GetHashCode(), str);
   }

   public static int AsciiHexDigitToInt(char c)
   {
      if (c is >= '0' and <= '9')
      {
         return c - '0';
      }

      if (c is >= 'A' and <= 'F')
      {
         return c - 'A' + 10;
      }

      if (c is >= 'a' and <= 'f')
      {
         return c - 'a' + 10;
      }

      return -1;
   }
}
