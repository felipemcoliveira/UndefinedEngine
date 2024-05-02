using System;
using System.Collections.Generic;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

internal class CppLexicalAnalyzer
{
   public int EndOfFilePosition => m_SourceCode.Length;
   public bool IsEndOfFile => Position >= EndOfFilePosition;
   public char CurrentCharacter => m_SourceCode[Position];

   internal int Position { get; set; }

   private static readonly Dictionary<StringView, CppTokenType> s_SpecialIdentifierTokenTypes;
   private static readonly HashSet<char> s_SymbolCharacters;
   private static readonly HashSet<StringView> s_AllSymbols;

   private readonly string m_SourceCode;
   private readonly ReadOnlyMemory<char> m_SourceCodeMemory;

   public CppLexicalAnalyzer(string sourceCode)
   {
      m_SourceCode = sourceCode;
      m_SourceCodeMemory = sourceCode.AsMemory();
      Position = 0;
   }

   static CppLexicalAnalyzer()
   {
      s_SpecialIdentifierTokenTypes = new()
      {
         { "alignas", CppTokenType.Keyword },
         { "alignof", CppTokenType.Keyword },
         { "and", CppTokenType.Keyword },
         { "and_eq", CppTokenType.Keyword },
         { "asm", CppTokenType.Keyword },
         { "atomic_cancel", CppTokenType.Keyword },
         { "atomic_commit", CppTokenType.Keyword },
         { "atomic_noexcept", CppTokenType.Keyword },
         { "auto", CppTokenType.Keyword },
         { "bitand", CppTokenType.Keyword },
         { "bitor", CppTokenType.Keyword },
         { "bool", CppTokenType.Keyword },
         { "break", CppTokenType.Keyword },
         { "case", CppTokenType.Keyword },
         { "catch", CppTokenType.Keyword },
         { "char", CppTokenType.Keyword },
         { "char8_t", CppTokenType.Keyword },
         { "char16_t", CppTokenType.Keyword },
         { "char32_t", CppTokenType.Keyword },
         { "class", CppTokenType.Keyword },
         { "compl", CppTokenType.Keyword },
         { "concept", CppTokenType.Keyword },
         { "const", CppTokenType.Keyword },
         { "consteval", CppTokenType.Keyword },
         { "constexpr", CppTokenType.Keyword },
         { "constinit", CppTokenType.Keyword },
         { "const_cast", CppTokenType.Keyword },
         { "continue", CppTokenType.Keyword },
         { "co_await", CppTokenType.Keyword },
         { "co_return", CppTokenType.Keyword },
         { "co_yield", CppTokenType.Keyword },
         { "decltype", CppTokenType.Keyword },
         { "default", CppTokenType.Keyword },
         { "delete", CppTokenType.Keyword },
         { "do", CppTokenType.Keyword },
         { "double", CppTokenType.Keyword },
         { "dynamic_cast", CppTokenType.Keyword },
         { "else", CppTokenType.Keyword },
         { "enum", CppTokenType.Keyword },
         { "explicit", CppTokenType.Keyword },
         { "export", CppTokenType.Keyword },
         { "extern", CppTokenType.Keyword },
         { "float", CppTokenType.Keyword },
         { "for", CppTokenType.Keyword },
         { "friend", CppTokenType.Keyword },
         { "goto", CppTokenType.Keyword },
         { "if", CppTokenType.Keyword },
         { "inline", CppTokenType.Keyword },
         { "int", CppTokenType.Keyword },
         { "long", CppTokenType.Keyword },
         { "mutable", CppTokenType.Keyword },
         { "namespace", CppTokenType.Keyword },
         { "new", CppTokenType.Keyword },
         { "noexcept", CppTokenType.Keyword },
         { "not", CppTokenType.Keyword },
         { "not_eq", CppTokenType.Keyword },
         { "operator", CppTokenType.Keyword },
         { "or", CppTokenType.Keyword },
         { "or_eq", CppTokenType.Keyword },
         { "private", CppTokenType.Keyword },
         { "protected", CppTokenType.Keyword },
         { "public", CppTokenType.Keyword },
         { "reflexpr", CppTokenType.Keyword },
         { "register", CppTokenType.Keyword },
         { "reinterpret_cast", CppTokenType.Keyword },
         { "requires", CppTokenType.Keyword },
         { "return", CppTokenType.Keyword },
         { "short", CppTokenType.Keyword },
         { "signed", CppTokenType.Keyword },
         { "sizeof", CppTokenType.Keyword },
         { "static", CppTokenType.Keyword },
         { "static_assert", CppTokenType.Keyword },
         { "static_cast", CppTokenType.Keyword },
         { "struct", CppTokenType.Keyword },
         { "switch", CppTokenType.Keyword },
         { "synchronized", CppTokenType.Keyword },
         { "template", CppTokenType.Keyword },
         { "this", CppTokenType.Keyword },
         { "thread_local", CppTokenType.Keyword },
         { "throw", CppTokenType.Keyword },
         { "try", CppTokenType.Keyword },
         { "typedef", CppTokenType.Keyword },
         { "typeid", CppTokenType.Keyword },
         { "typename", CppTokenType.Keyword },
         { "union", CppTokenType.Keyword },
         { "unsigned", CppTokenType.Keyword },
         { "using", CppTokenType.Keyword },
         { "virtual", CppTokenType.Keyword },
         { "void", CppTokenType.Keyword },
         { "volatile", CppTokenType.Keyword },
         { "wchar_t", CppTokenType.Keyword },
         { "while", CppTokenType.Keyword },
         { "xor", CppTokenType.Keyword },
         { "xor_eq", CppTokenType.Keyword },
         { "true", CppTokenType.BooleanLiteral },
         { "false", CppTokenType.BooleanLiteral },
         { "nullptr", CppTokenType.PointerLiteral },
         { "UCLASS", CppTokenType.EngineHeader },
         { "UENUM", CppTokenType.EngineHeader },
         { "USTRUCT", CppTokenType.EngineHeader },
         { "UMETHOD", CppTokenType.EngineHeader },
         { "UMETA", CppTokenType.EngineHeader },
         { "UPROPERTY", CppTokenType.EngineHeader },
         { "UFUNCTION", CppTokenType.EngineHeader }
      };

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

      s_SymbolCharacters = [];
      foreach (StringView symbol in s_AllSymbols)
      {
         foreach (char c in symbol)
         {
            s_SymbolCharacters.Add(c);
         }
      }
   }

   public CppLexicalAnalysis Analyze()
   {
      List<CppToken> tokens = new(20 / m_SourceCode.Length);
      List<int> engineHeaderTokenIndices = new(5);

      Position = 0;

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
               AddToken(tokens, CppTokenType.Symbol, operatorStart, operatorLength);
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
               AddToken(tokens, CppTokenType.Symbol, operatorStart, operatorLength);
               break;
            }

            case '.':
            {
               int start = Position;
               if (TryConsumeNumberLiteral())
               {
                  AddToken(tokens, CppTokenType.Symbol, start, Position - start);
                  break;
               }

               // it will always return true since the dot is a symbol
               TryConsumeSymbol(out int operatorStart, out int operatorLength);
               AddToken(tokens, CppTokenType.Symbol, start, operatorLength);
               break;
            }

            case '"':
            {
               int start = Position;
               ConsumeStringLiteral(false);
               AddToken(tokens, CppTokenType.StringLiteral, start, Position - start);
               break;
            }

            case >= '0' and <= '9':
            {
               int start = Position;
               if (TryConsumeNumberLiteral())
               {
                  AddToken(tokens, CppTokenType.NumericLiteral, start, Position - start);
                  break;
               }

               throw new CppIllFormedCodeException(Position, "Unexpected character.");
            }

            case '_':
            case >= 'a' and <= 'z':
            case >= 'A' and <= 'Z':
            {
               int start = Position;
               TryConsumeIdentifier(out ReadOnlySpan<char> identifier);

               if (TryConsume('"'))
               {
                  ConsumeStringLiteral(identifier[^-1] == 'R');
                  AddToken(tokens, CppTokenType.StringLiteral, start, Position - start);
                  break;
               }

               if (TryConsume('\''))
               {
                  ConsumeCharLiteral();
                  AddToken(tokens, CppTokenType.CharacterLiteral, start, Position - start);
                  break;
               }

               CppTokenType type = GetIdentifierTokenType(identifier);
               if (type == CppTokenType.EngineHeader)
               {
                  engineHeaderTokenIndices.Add(tokens.Count);
               }

               AddToken(tokens, type, start, Position - start);
               break;
            }

            default:
            {
               throw new CppIllFormedCodeException(Position, "Unexpected character.");
            }
         }
      }

      AddToken(tokens, CppTokenType.EndOfFile, EndOfFilePosition, 0);
      return new(tokens, engineHeaderTokenIndices, m_SourceCode);
   }

   private void AddToken(List<CppToken> tokens, CppTokenType type, int start, int length)
   {
      tokens.Add(new
      (
         type: type,
         startPosition: start,
         contentView: m_SourceCodeMemory.Slice(start, length),
         index: tokens.Count
      ));
   }

   internal bool TryConsumeNumberLiteral()
   {
      return CppNumericLiteralLexicalAnalysis.TryConsume(this);
   }

   internal bool TryConsumeNumericLiteralDigits(int numericBase)
   {
      int start = Position;
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
         throw new CppIllFormedCodeException(Position, "Invalid digit base in number literal.");
      }

      return Position != start;
   }

   internal void ConsumeCharLiteral()
   {
      if (!TryConsume('\''))
      {
         throw new CppIllFormedCodeException(Position, "Invalid character literal.");
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
                  throw new CppIllFormedCodeException(Position, "Unterminated character literal.");
               }

               break;
            }
            default:
            {
               throw new CppIllFormedCodeException(Position, "Invalid escape sequence.");
            }
         }

         ConsumeCharacter();
         if (!TryConsume('\''))
         {
            throw new CppIllFormedCodeException(Position, "Unterminated character literal.");
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
         throw new CppIllFormedCodeException(Position, "Invalid raw string literal.");
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

      TryConsumeIdentifier(out ReadOnlySpan<char> delimiter);
      if (!TryConsume('('))
      {
         throw new CppIllFormedCodeException(Position, "Invalid raw string literal.");
      }

      while (!IsEndOfFile)
      {
         int closeDelimiterStart = Position;
         if (TryConsume(')') && TryConsume(delimiter) && TryConsume('"', quoteCount))
         {
            return;
         }

         Position = closeDelimiterStart;
         ConsumeCharacter();
      }

      throw new CppIllFormedCodeException(Position, "Unterminated string literal.");
   }

   internal void ConsumeStringLiteral()
   {
      if (!TryConsume('"'))
      {
         throw new CppIllFormedCodeException(Position, "Invalid string literal.");
      }

      while (!IsEndOfFile)
      {
         if (CurrentCharacter == '"' && !IsCharacterAt(Position - 1, '\\'))
         {
            ConsumeCharacter();
            return;
         }

         ConsumeCharacter();
      }
   }

   internal bool TryConsumeSymbol(out int start, out int length)
   {
      start = Position;
      length = Math.Min(3, EndOfFilePosition - Position);

      while (length > 0)
      {
         ReadOnlySpan<char> symbol = m_SourceCode.AsSpan().Slice(start, length);
         if (s_AllSymbols.Contains(symbol))
         {
            Position += length;
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

   internal static CppTokenType GetIdentifierTokenType(StringView identifier)
   {
      if (s_SpecialIdentifierTokenTypes.TryGetValue(identifier, out CppTokenType specialTokenType))
      {
         return specialTokenType;
      }

      return CppTokenType.Identifier;
   }

   internal bool TryConsumeIdentifier(out ReadOnlySpan<char> identifier)
   {
      bool value = TryConsumeIdentifier(out int start, out int length);
      identifier = m_SourceCode.AsSpan().Slice(start, length);
      return value;
   }

   internal bool TryConsumeIdentifier(out int start, out int length)
   {
      start = Position;
      length = 0;
      if (IsEndOfFile || CurrentCharacter != '_' && !char.IsLetter(CurrentCharacter))
      {
         return false;
      }

      while (!IsEndOfFile && (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_'))
      {
         ConsumeCharacter();
      }

      length = Position - start;
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

      int openCommendPosition = Position;
      while (!IsEndOfFile)
      {
         if (TryConsume("*/"))
         {
            return true;
         }

         ConsumeCharacter();
      }

      throw new CppIllFormedCodeException(Position, "Unterminated comment");
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
      int newPosition = Position;
      while (newPosition < EndOfFilePosition && m_SourceCode[newPosition] == c)
      {
         newPosition++;
         if (newPosition - Position == count)
         {
            Position = newPosition;
            return true;
         }
      }

      return false;
   }

   internal bool TryConsume(ReadOnlySpan<char> token)
   {
      int newPosition = Position;
      while (newPosition < EndOfFilePosition && m_SourceCode[newPosition] == token[newPosition - Position])
      {
         newPosition++;
         if (newPosition - Position == token.Length)
         {
            Position = newPosition;
            return true;
         }
      }

      return false;
   }

   internal bool TryConsumeIgnoreCase(ReadOnlySpan<char> token)
   {
      int newPosition = Position;
      while (newPosition < EndOfFilePosition && char.ToUpper(m_SourceCode[newPosition]) == char.ToUpper(token[newPosition - Position]))
      {
         newPosition++;
         if (newPosition - Position == token.Length)
         {
            Position = newPosition;
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
      if (Position + 1 < EndOfFilePosition)
      {
         character = m_SourceCode[Position + 1];
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
      if (CurrentCharacter == '\r' && IsCharacterAt(Position + 1, '\n'))
      {
         Position += 2;
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
      Position++;
   }

   private void ValidateIsNotEndOfFile(string message)
   {
      if (IsEndOfFile)
      {
         throw new CppIllFormedCodeException(Position, message);
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
