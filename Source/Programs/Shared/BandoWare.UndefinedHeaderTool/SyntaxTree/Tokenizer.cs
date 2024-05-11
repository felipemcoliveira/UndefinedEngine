using BandoWare.Core;
using System;
using System.Collections.Generic;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal class Tokenizer
{
   private int EndOfFilePosition => m_SourceFileContent.Length;
   private bool IsEndOfFile => Position >= EndOfFilePosition;
   private char CurrentCharacter => m_SourceFileContent[Position];

   private int Position { get; set; }

   private static readonly Dictionary<StringView, TokenType> s_SpecialIdentifierTokenTypes;
   private static readonly HashSet<char> s_SymbolCharacters;
   private static readonly HashSet<StringView> s_AllSymbols;

   private readonly SourceFile m_SourceFile;
   private readonly string m_SourceFileContent;
   private readonly ReadOnlyMemory<char> m_SourceFileContentMemory;

   public Tokenizer(SourceFile sourceFile)
   {
      m_SourceFile = sourceFile;
      m_SourceFileContent = sourceFile.Content;
      m_SourceFileContentMemory = sourceFile.Content.AsMemory();
      Position = 0;
   }

   static Tokenizer()
   {
      s_SpecialIdentifierTokenTypes = new()
      {
         { "alignas", TokenType.Keyword },
         { "alignof", TokenType.Keyword },
         { "and", TokenType.Keyword },
         { "and_eq", TokenType.Keyword },
         { "asm", TokenType.Keyword },
         { "atomic_cancel", TokenType.Keyword },
         { "atomic_commit", TokenType.Keyword },
         { "atomic_noexcept", TokenType.Keyword },
         { "auto", TokenType.Keyword },
         { "bitand", TokenType.Keyword },
         { "bitor", TokenType.Keyword },
         { "bool", TokenType.Keyword },
         { "break", TokenType.Keyword },
         { "case", TokenType.Keyword },
         { "catch", TokenType.Keyword },
         { "char", TokenType.Keyword },
         { "char8_t", TokenType.Keyword },
         { "char16_t", TokenType.Keyword },
         { "char32_t", TokenType.Keyword },
         { "class", TokenType.Keyword },
         { "compl", TokenType.Keyword },
         { "concept", TokenType.Keyword },
         { "const", TokenType.Keyword },
         { "consteval", TokenType.Keyword },
         { "constexpr", TokenType.Keyword },
         { "constinit", TokenType.Keyword },
         { "const_cast", TokenType.Keyword },
         { "continue", TokenType.Keyword },
         { "co_await", TokenType.Keyword },
         { "co_return", TokenType.Keyword },
         { "co_yield", TokenType.Keyword },
         { "decltype", TokenType.Keyword },
         { "default", TokenType.Keyword },
         { "delete", TokenType.Keyword },
         { "do", TokenType.Keyword },
         { "double", TokenType.Keyword },
         { "dynamic_cast", TokenType.Keyword },
         { "else", TokenType.Keyword },
         { "enum", TokenType.Keyword },
         { "explicit", TokenType.Keyword },
         { "export", TokenType.Keyword },
         { "extern", TokenType.Keyword },
         { "float", TokenType.Keyword },
         { "for", TokenType.Keyword },
         { "friend", TokenType.Keyword },
         { "goto", TokenType.Keyword },
         { "if", TokenType.Keyword },
         { "inline", TokenType.Keyword },
         { "int", TokenType.Keyword },
         { "long", TokenType.Keyword },
         { "mutable", TokenType.Keyword },
         { "namespace", TokenType.Keyword },
         { "new", TokenType.Keyword },
         { "noexcept", TokenType.Keyword },
         { "not", TokenType.Keyword },
         { "not_eq", TokenType.Keyword },
         { "operator", TokenType.Keyword },
         { "or", TokenType.Keyword },
         { "or_eq", TokenType.Keyword },
         { "private", TokenType.Keyword },
         { "protected", TokenType.Keyword },
         { "public", TokenType.Keyword },
         { "reflexpr", TokenType.Keyword },
         { "register", TokenType.Keyword },
         { "reinterpret_cast", TokenType.Keyword },
         { "requires", TokenType.Keyword },
         { "return", TokenType.Keyword },
         { "short", TokenType.Keyword },
         { "signed", TokenType.Keyword },
         { "sizeof", TokenType.Keyword },
         { "static", TokenType.Keyword },
         { "static_assert", TokenType.Keyword },
         { "static_cast", TokenType.Keyword },
         { "struct", TokenType.Keyword },
         { "switch", TokenType.Keyword },
         { "synchronized", TokenType.Keyword },
         { "template", TokenType.Keyword },
         { "this", TokenType.Keyword },
         { "thread_local", TokenType.Keyword },
         { "throw", TokenType.Keyword },
         { "try", TokenType.Keyword },
         { "typedef", TokenType.Keyword },
         { "typeid", TokenType.Keyword },
         { "typename", TokenType.Keyword },
         { "union", TokenType.Keyword },
         { "unsigned", TokenType.Keyword },
         { "using", TokenType.Keyword },
         { "virtual", TokenType.Keyword },
         { "void", TokenType.Keyword },
         { "volatile", TokenType.Keyword },
         { "wchar_t", TokenType.Keyword },
         { "while", TokenType.Keyword },
         { "xor", TokenType.Keyword },
         { "xor_eq", TokenType.Keyword },
         { "true", TokenType.BooleanLiteral },
         { "false", TokenType.BooleanLiteral },
         { "nullptr", TokenType.PointerLiteral },
         { "UCLASS", TokenType.EngineHeader },
         { "UENUM", TokenType.EngineHeader },
         { "USTRUCT", TokenType.EngineHeader },
         { "UMETHOD", TokenType.EngineHeader },
         { "UMETA", TokenType.EngineHeader },
         { "UPROPERTY", TokenType.EngineHeader },
         { "UFUNCTION", TokenType.EngineHeader }
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

   public TokenizeResult Tokenize()
   {
      List<Token> tokens = new(20 / m_SourceFileContent.Length);
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
               AddToken(tokens, TokenType.Symbol, operatorStart, operatorLength);
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
               AddToken(tokens, TokenType.Symbol, operatorStart, operatorLength);
               break;
            }

            case '.':
            {
               int start = Position;
               if (TryConsumeNumericLiteral())
               {
                  AddToken(tokens, TokenType.Symbol, start, Position - start);
                  break;
               }

               // it will always return true since the dot is a symbol
               TryConsumeSymbol(out int operatorStart, out int operatorLength);
               AddToken(tokens, TokenType.Symbol, start, operatorLength);
               break;
            }

            case '"':
            {
               int start = Position;
               ConsumeStringLiteral(false);
               AddToken(tokens, TokenType.StringLiteral, start, Position - start);
               break;
            }

            case >= '0' and <= '9':
            {
               int start = Position;
               if (TryConsumeNumericLiteral())
               {
                  AddToken(tokens, TokenType.NumericLiteral, start, Position - start);
                  break;
               }

               throw CreateIllFormedCodeException(start, "Unexpected character.");
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
                  AddToken(tokens, TokenType.StringLiteral, start, Position - start);
                  break;
               }

               if (TryConsume('\''))
               {
                  ConsumeCharLiteral();
                  AddToken(tokens, TokenType.CharacterLiteral, start, Position - start);
                  break;
               }

               TokenType type = GetIdentifierTokenType(identifier);
               if (type == TokenType.EngineHeader)
               {
                  engineHeaderTokenIndices.Add(tokens.Count);
               }

               AddToken(tokens, type, start, Position - start);
               break;
            }

            default:
            {
               throw CreateIllFormedCodeException(Position, "Unexpected character.");
            }
         }
      }

      AddToken(tokens, TokenType.EndOfFile, EndOfFilePosition, 0);
      return new(tokens, engineHeaderTokenIndices, m_SourceFile);
   }

   private void AddToken(List<Token> tokens, TokenType type, int position, int length)
   {
      tokens.Add(new
      (
         type: type,
         textPosition: position,
         text: m_SourceFileContentMemory.Slice(position, length)));
   }

   private bool TryConsumeNumericLiteralDigits(int numericBase)
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
         throw CreateIllFormedCodeException(Position, "Invalid digit base in number literal.");
      }

      return Position != start;
   }

   private void ConsumeCharLiteral()
   {
      if (!TryConsume('\''))
      {
         throw CreateIllFormedCodeException(Position, "Invalid character literal.");
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
                  throw CreateIllFormedCodeException(Position, "Unterminated character literal.");
               }

               break;
            }
            default:
            {
               throw CreateIllFormedCodeException(Position, "Invalid escape sequence.");
            }
         }

         ConsumeCharacter();
         if (!TryConsume('\''))
         {
            throw CreateIllFormedCodeException(Position, "Unterminated character literal.");
         }
      }
   }

   private void ConsumeStringLiteral(bool isRawLiteralString)
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
   /// <param name="start">TextPosition of the first quote.</param>
   /// <exception cref="IllFormedCodeException">Thrown when the syntax of the
   ///    raw string literal does not conform to C++ standards. This could be
   ///    due to an incorrect position (missing opening quote), improper delimiter
   ///    usage, or if the raw string literal is not properly terminated before
   ///    the end of the file is reached.</exception>
   /// <remarks>
   /// This method should be used after the literal prefix "R" has been
   /// consumed.
   /// </remarks>
   private void ConsumeRawLiteralString()
   {
      if (!TryConsume('"'))
      {
         throw CreateIllFormedCodeException(Position, "Invalid raw string literal.");
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
         throw CreateIllFormedCodeException(Position, "Invalid raw string literal.");
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

      throw CreateIllFormedCodeException(Position, "Unterminated string literal.");
   }

   private void ConsumeStringLiteral()
   {
      if (!TryConsume('"'))
      {
         throw CreateIllFormedCodeException(Position, "Invalid string literal.");
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

   private bool TryConsumeSymbol(out int start, out int length)
   {
      start = Position;
      length = Math.Min(3, EndOfFilePosition - Position);

      while (length > 0)
      {
         ReadOnlySpan<char> symbol = m_SourceFileContent.AsSpan().Slice(start, length);
         if (s_AllSymbols.Contains(symbol))
         {
            Position += length;
            return true;
         }

         length--;
      }

      return false;
   }

   private bool TryConsumeDigit(int numericBase)
   {
      int digit = AsciiHexDigitToInt(CurrentCharacter);
      if (digit == -1 || digit >= numericBase)
      {
         return false;
      }

      ConsumeCharacter();
      return true;
   }

   private void ConsumeWhitespaces()
   {
      while (!IsEndOfFile && char.IsWhiteSpace(CurrentCharacter))
      {
         ConsumeCharacter();
      }
   }

   private static TokenType GetIdentifierTokenType(StringView identifier)
   {
      if (s_SpecialIdentifierTokenTypes.TryGetValue(identifier, out TokenType specialTokenType))
      {
         return specialTokenType;
      }

      return TokenType.Identifier;
   }

   private bool TryConsumeNumericLiteral()
   {
      return TryConsumeHexadecimalLiteral()
         || TryConsumeBinaryLiteral()
         || TryConsumeOctalLiteral()
         || TryConsumeDecimalLiteral();
   }

   private bool TryConsumeDecimalLiteral()
   {
      int start = Position;
      bool isFloating = false;
      if (!TryConsumeDigit(10) && !(isFloating = TryConsume('.') && TryConsumeDigit(10)))
      {
         Position = start;
         return false;
      }

      ConsumeDigits(10);

      if (!isFloating && TryConsume('.'))
      {
         ConsumeDigits(10);
      }

      TryConsumeExponent();

      // user-define literal operator identifier
      TryConsumeIdentifier(out _);
      return true;
   }

   private bool TryConsumeOctalLiteral()
   {
      int start = Position;
      if (!TryConsume('0') || !TryConsumeDigit(8))
      {
         Position = start;
         return false;
      }

      ConsumeDigits(8);
      if (!IsEndOfFile && AsciiHexDigitToInt(CurrentCharacter) >= 8)
      {
         throw CreateIllFormedCodeException(Position, "Invalid digit in octal constant.");
      }

      if (TryConsume('.'))
      {
         throw CreateIllFormedCodeException(Position, "Invalid prefix \"0\" for floating constant.");
      }

      TryConsumeExponent();

      // user-define literal operator identifier
      TryConsumeIdentifier(out _);
      return true;
   }

   private bool TryConsumeBinaryLiteral()
   {
      int start = Position;
      if (!TryConsumeIgnoreCase("0b") && !TryConsumeDigit(2))
      {
         Position = start;
         return false;
      }

      ConsumeDigits(2);

      if (!IsEndOfFile && AsciiHexDigitToInt(CurrentCharacter) >= 2)
      {
         throw CreateIllFormedCodeException(Position, "Invalid digit in binary constant.");
      }

      if (TryConsume('.') || TryConsume('e'))
      {
         throw CreateIllFormedCodeException(Position, "Invalid prefix \"0b\" for floating constant.");
      }

      // user-define literal operator identifier
      TryConsumeIdentifier(out _);
      return true;
   }

   private bool TryConsumeHexadecimalLiteral()
   {
      int start = Position;
      bool isFloating = false;
      if (!TryConsumeIgnoreCase("0x") || !TryConsumeDigit(16) && !(isFloating = TryConsume('.') && TryConsumeDigit(16)))
      {
         Position = start;
         return false;
      }

      ConsumeDigits(16);

      if (!isFloating && TryConsume('.'))
      {
         isFloating = true;
         ConsumeDigits(16);
      }

      if (isFloating)
      {
         if (!TryConsumeIgnoreCase('p'))
         {
            throw CreateIllFormedCodeException(Position, "Hexadecimal floating constants require an exponent.");
         }

         TryConsumeSingleNumericSign();
         ConsumeDigits(10);
      }

      // user-define literal operator identifier
      TryConsumeIdentifier(out _);
      return true;
   }

   private bool TryConsumeExponent()
   {
      if (!TryConsumeIgnoreCase('e'))
      {
         return false;
      }

      TryConsumeSingleNumericSign();

      if (!TryConsumeDigit(10))
      {
         throw CreateIllFormedCodeException(Position, "Exponent has no digits.");
      }

      ConsumeDigits(10);
      return true;
   }

   private void ConsumeDigits(int numericBase)
   {
      while (TryConsumeDigit(numericBase))
      {
      }
   }

   private bool TryConsumeIdentifier(out ReadOnlySpan<char> identifier)
   {
      bool value = TryConsumeIdentifier(out int start, out int length);
      identifier = m_SourceFileContent.AsSpan().Slice(start, length);
      return value;
   }

   private bool TryConsumeIdentifier(out int start, out int length)
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

   private bool TryConsumeComment()
   {
      return TryConsumeSingleLineComment() || TryConsumeMultilineComment();
   }

   private bool TryConsumeSingleLineComment()
   {
      if (!TryConsume("//"))
      {
         return false;
      }

      ConsumeLine();
      return true;
   }

   private bool TryConsumeMultilineComment()
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

      throw CreateIllFormedCodeException(Position, "Unterminated comment");
   }

   private bool TryConsumeSingleNumericSign()
   {
      while (CurrentCharacter is '+' or '-')
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   private bool TryConsume(char c)
   {
      if (!IsEndOfFile && CurrentCharacter == c)
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   private bool TryConsumeIgnoreCase(char c)
   {
      if (!IsEndOfFile && char.ToUpper(CurrentCharacter) == char.ToUpper(c))
      {
         ConsumeCharacter();
         return true;
      }

      return false;
   }

   private bool TryConsume(char c, int count)
   {
      int newPosition = Position;
      while (newPosition < EndOfFilePosition && m_SourceFileContent[newPosition] == c)
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

   private bool TryConsume(ReadOnlySpan<char> token)
   {
      int newPosition = Position;
      while (newPosition < EndOfFilePosition && m_SourceFileContent[newPosition] == token[newPosition - Position])
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

   private bool TryConsumeIgnoreCase(ReadOnlySpan<char> token)
   {
      int newPosition = Position;
      while (newPosition < EndOfFilePosition
         && char.ToUpper(m_SourceFileContent[newPosition]) == char.ToUpper(token[newPosition - Position]))
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

   private bool IsCharacterAt(int position, char c)
   {
      if (position < m_SourceFileContent.Length)
      {
         return m_SourceFileContent[position] == c;
      }

      return false;
   }

   private void ConsumeLine()
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

   private bool TryConsumeNewLineCharacters()
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

   private void ConsumeCharacter()
   {
      Position++;
   }

   private void ValidateIsNotEndOfFile(string message)
   {
      if (IsEndOfFile)
      {
         throw CreateIllFormedCodeException(Position, message);
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

   private static IllFormedCodeException CreateIllFormedCodeException(int contentPosition, string message)
   {
      SourceFileTextPosition position = new(SourceFileTextPositionType.TextPosition, contentPosition);
      return new IllFormedCodeException(position, message);
   }
}
