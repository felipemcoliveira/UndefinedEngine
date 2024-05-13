using BandoWare.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal partial class SyntaxParser
{
   private Token CurrentToken => m_Tokens[m_Position];
   private TokenType CurrentTokenType => CurrentToken.Type;
   private bool IsEndOfFile => CurrentToken.Type == TokenType.EndOfFile;
   private StringView CurrentTokenValue => CurrentToken.Text;
   private int CurrentTokenTextPosition => CurrentToken.TextPosition;

   private static readonly HashSet<StringView> s_FunctionSpecifiers =
   [
      // C++ specifiers
      "inline",
      "virtual",
      "explicit",
      "friend",
      "static",
      "const",
      "volatile",
      "mutable",
      "extern",
      "register",
      "thread_local",
      "constexpr",

      // Engine specifiers 
      "FORCEINLINE",
      "CONSTEXPR"
   ];

   private static readonly HashSet<StringView> s_OverloadableOperators =
   [
      "new",
      "delete",
      "new []",
      "delete []",
      "+",
      "-",
      "*",
      "/",
      "%",
      "^",
      "&",
      "|",
      "~",
      "!",
      "=",
      "<",
      ">",
      "+=",
      "-=",
      "*=",
      "/=",
      "%=",
      "^=",
      "&=",
      "|=",
      "<<",
      ">>",
      ">>=",
      "<<=",
      "==",
      "!=",
      "<=",
      ">=",
      "&&",
      "||",
      "++",
      "--",
      ",",
      "->*",
      "->",
      "()",
      "[]"
   ];

   private int m_Position;
   private Stack<char> m_DelimiterStack;
   private string m_SourceFileText;
   private List<Token> m_Tokens;
   private List<int> m_HeaderMacroTokenIndices;
   private SourceFileTextPositionMap m_TextPositionMap;
   private string m_SourceFilePath;

   public SyntaxParser(TokenizeResult tokenizeResult, SourceFileTextPositionMap sourceFileTextPositionMap)
   {
      m_Position = 0;
      m_DelimiterStack = [];
      m_HeaderMacroTokenIndices = tokenizeResult.HeaderMacroTokenIndices;
      m_SourceFilePath = tokenizeResult.SourceFilePath;
      m_SourceFileText = tokenizeResult.SourceFileText;
      m_Tokens = tokenizeResult.Tokens;
      m_TextPositionMap = sourceFileTextPositionMap;
   }

   public RootNode Parse()
   {
      RootNode root = new();

      SkipUntilNextEngineHeader();
      while (!IsEndOfFile)
      {
         if (AcceptDeclaration(out Node? node))
         {
            root.AddChild(node);
            continue;
         }

         SkipUntilNextEngineHeader();
      }

      root.SetTokenRange(0, m_Tokens.Count - 1);
      return root;
   }

   public bool AcceptDeclaration(out Node? node)
   {
      if (IsEndOfFile || CurrentTokenType != TokenType.HeaderMacro)
      {
         node = null;
         return false;
      }

      return AcceptClassDeclaration(out node) || AcceptEnumDeclaration(out node);
   }

   private bool AcceptClassDeclaration(out Node? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UCLASS", out HeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      if (!AcceptKeyword("class"))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected \"class\" keyword.");
      }

      bool hasImportExportAttribute = AcceptImportExportClassAttribute();

      StringView className = CurrentTokenValue;
      if (hasImportExportAttribute)
      {
         ExpectFormat(TokenType.Identifier, "Expected identifier after \"{0}\" keyword.", m_Tokens[m_Position - 1].Text);
      }
      else
      {
         Expect(TokenType.Identifier, "Expected identifier after \"class\" keyword.");
      }

      // base class
      if (AcceptExact(TokenType.Symbol, ":"))
      {
         // TODO: parse base classes
         SkipUntil("{;", "{(<", ">)}");
      }

      // forward declaration
      if (AcceptExact(TokenType.Symbol, ";"))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "UCLASS should be used only with class definition.");
      }

      ClassNode classNode = new(className);
      classNode.AddChild(engineHeaderNode);

      ExpectExact(TokenType.Symbol, "{", "Expected \"{\" after class declaration");
      ExpectGeneratedBodyMacro();

      int curlyBraceCount = 0;
      while (!IsEndOfFile)
      {
         if (AcceptClassMemberDeclaration(out Node? classMemberNode))
         {
            classNode.AddChild(classMemberNode);
            continue;
         }

         if (TrySkipDelimiters('{', '}'))
         {
            continue;
         }

         if (AcceptExact(TokenType.Symbol, "}"))
         {
            curlyBraceCount--;
            if (curlyBraceCount == 0)
            {
               ExpectExact(TokenType.Symbol, ";", "Expected \";\" after class declaration");
               break;
            }
         }

         m_Position++;
      }

      classNode.SetTokenRange(firstNodeIndex, m_Position - 1);
      node = classNode;
      return true;
   }

   private void ExpectGeneratedBodyMacro()
   {
      if (!AcceptExact(TokenType.Identifier, "GENERATED_BODY")
         || !AcceptExact(TokenType.Symbol, "(")
         || !AcceptExact(TokenType.Symbol, ")"))
      {
         throw CreateIllFormedCodeException(m_Tokens[m_Position - 1].TextPosition, "Expected GENERATED_BODY(). " +
            "It should be the first thing in the class body, even before access specifiers).");
      }
   }

   private bool AcceptClassMemberDeclaration(out Node? node)
   {
      if (IsEndOfFile || CurrentTokenType != TokenType.HeaderMacro)
      {
         node = null;
         return false;
      }

      return AcceptFunctionDeclaration(out node);
   }

   private bool AcceptFunctionDeclaration(out Node? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UFUNCTION", out HeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      HashSet<StringView> functionSpecifiers = [];
      while (!IsEndOfFile)
      {
         if (!AcceptAny(TokenType.Keyword, s_FunctionSpecifiers, out StringView specifier))
         {
            break;
         }

         functionSpecifiers.Add(specifier);
      }


      SkipUntil("(", "{(<", ">)}");

      Token functionNameToken = m_Tokens[m_Position - 1];

      if (functionNameToken.Type != TokenType.Identifier)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected function name.");
      }

      int identifierTokenIndex = m_Position - 1;
      if (!IsEndOfFile && m_Tokens[identifierTokenIndex].Type != TokenType.Identifier)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected function name.");
      }

      FunctionNode functionNode = new(functionNameToken.Text);

      functionNode.AddChild(engineHeaderNode);
      functionNode.IsVirtual = functionSpecifiers.Contains("virtual");
      functionNode.IsStatic = functionSpecifiers.Contains("static");
      functionNode.IsConstExpr = functionSpecifiers.Contains("constexpr");

      foreach (FunctionParameterNode parameter in ConsumeFunctionParameters())
      {
         functionNode.AddChild(parameter);
      }

      SkipUntil("{;", "{(<", ">)}");

      if (!TrySkipDelimiters('{', '}') && !AcceptExact(TokenType.Symbol, ";"))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected \";\" or function body after function declaration.");
      }

      node = functionNode;
      return true;
   }

   private IEnumerable<FunctionParameterNode> ConsumeFunctionParameters()
   {
      ExpectExact(TokenType.Symbol, "(", "Expected \"(\" after function name.");

      if (AcceptKeyword("void"))
      {
         ExpectExact(TokenType.Symbol, ")", "Expected \")\" after parameter list.");
         yield break;
      }

      int paremeterIndex = 0;
      while (!IsEndOfFile)
      {
         if (AcceptExact(TokenType.Symbol, ")"))
         {
            yield break;
         }

         if (AcceptExact(TokenType.Symbol, ","))
         {
            continue;
         }

         yield return ConsumeFunctionParameter(paremeterIndex++);
      }

      throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected \")\" after parameter list.");
   }

   private FunctionParameterNode ConsumeFunctionParameter(int parameterIndex)
   {
      // this is gonna be a very naive solution: we're just gonna skip
      // until get where we expect the parameter identifier.
      SkipUntil("=,)", "{(<", ">)}");
      m_Position--;

      Expect(TokenType.Identifier, out StringView parameterName, "Expected parameter identifier.");

      // skip until we get to the next parameter or the closing parenthesis
      SkipUntil(",)", "{(<", ">)}");

      return new FunctionParameterNode(parameterName, parameterIndex);
   }

   private bool AcceptAny(TokenType validTokenTypes, HashSet<StringView> set, out StringView acceptedValue)
   {
      if (IsEndOfFile || (CurrentTokenType & validTokenTypes) == 0 || !set.Contains(CurrentTokenValue))
      {
         acceptedValue = StringView.Empty;
         return false;
      }

      acceptedValue = CurrentTokenValue;
      m_Position++;
      return true;
   }

   private bool AcceptImportExportClassAttribute()
   {
      Regex importExportAttributeRegex = GetImportExportClassAttributeRegex();
      bool hasImportExportAttribute = false;
      if (!IsEndOfFile && importExportAttributeRegex.IsMatch(CurrentTokenValue))
      {
         hasImportExportAttribute = true;
         m_Position++;
      }

      return hasImportExportAttribute;
   }

   private bool AcceptExportImportClassAttribute()
   {
      if (!IsEndOfFile || CurrentToken.Type != TokenType.Identifier)
      {
         return false;
      }

      ReadOnlySpan<char> identifier = CurrentTokenValue;

      return false;
   }

   private bool AcceptEnumDeclaration(out Node? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UENUM", out HeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      if (!AcceptKeyword("enum"))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected \"enum\" keyword.");
      }

      _ = AcceptKeyword("class") || AcceptKeyword("struct");

      Expect(TokenType.Identifier, out StringView identifier, "Expected identifier after \"enum\" keyword.");
      EnumNode enumNode = new(identifier);

      // type identifier
      if (AcceptExact(TokenType.Symbol, ":"))
      {
         SkipUntil("{;", "{(<", ">)}");
      }

      // forward declaration
      if (AcceptExact(TokenType.Symbol, ";"))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "UENUM should be used only with enum definition.");
      }

      enumNode.AddChild(engineHeaderNode);

      ExpectExact(TokenType.Symbol, "{", "Expected \"{\" after enum declaration.");

      while (!IsEndOfFile)
      {
         if (TryAcceptEnumItem(out Node? enumItemNode))
         {
            enumNode.AddChild(enumItemNode);
            continue;
         }

         if (AcceptExact(TokenType.Symbol, ","))
         {
            continue;
         }

         if (AcceptExact(TokenType.Symbol, "}"))
         {
            break;
         }

         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Unexpected token.");
      }

      ExpectExact(TokenType.Symbol, ";", "Expected \";\" after enum declaration.");

      node = enumNode;
      return true;
   }

   private bool TryAcceptEnumItem(out Node? node)
   {
      int startTokenIndex = m_Position;
      bool hasEngineHeader = TryAcceptEngineHeadear("UMETA", out HeaderNode? engineHeaderNode);

      if (!Accept(TokenType.Identifier, out StringView identifier))
      {
         if (hasEngineHeader)
         {
            throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected identifier after UMETA header.");
         }

         m_Position = startTokenIndex;
         node = null;
         return false;
      }

      EnumItemNode enumItemNode = new(identifier);
      enumItemNode.AddChild(engineHeaderNode);

      if (AcceptExact(TokenType.Symbol, "="))
      {
         SkipUntil(",}", "{(<", ">)}");
      }

      node = enumItemNode;
      return true;
   }

   private bool TryAcceptEngineHeadear
   (
      string headerName,
      [NotNullWhen(true)] out HeaderNode? headerNode
   )
   {
      if (!AcceptExact(TokenType.HeaderMacro, headerName))
      {
         headerNode = null;
         return false;
      }

      headerNode = new(headerName);
      ExpectExact(TokenType.Symbol, "(", $"Expected \"(\" after \"{headerName}\".");

      while (!IsEndOfFile)
      {
         if (AcceptHeaderSpecifier(out HeaderSpecifierNode? specifierNode))
         {
            headerNode.AddChild(specifierNode);
            continue;
         }

         if (AcceptExact(TokenType.Symbol, ","))
         {
            continue;
         }

         if (AcceptExact(TokenType.Symbol, ")"))
         {
            break;
         }

         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Unexpected character \"{CurrentTokenValue}\".");
      }

      return true;
   }

   private bool AcceptHeaderSpecifier(out HeaderSpecifierNode? specifierNode)
   {
      if (IsEndOfFile || !Accept(TokenType.Identifier, out StringView specifierName))
      {
         specifierNode = null;
         return false;
      }

      specifierNode = new HeaderSpecifierNode(specifierName);
      if (!AcceptExact(TokenType.Symbol, "="))
      {
         return true;
      }

      if (!AcceptLiteral(out CppLiteralNode? literalNode))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected literal after \"=\".");
      }

      specifierNode.AddChild(literalNode);
      return true;
   }

   /// <remarks>
   /// This method only support a subset of literals, which are boolean, integer
   /// and string literals. And even the supported literals have their own
   /// </remarks>
   private bool AcceptLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      return AcceptBooleanLiteral(out node) || AcceptIntegerLiteral(out node) || AcceptStringLiteral(out node);
   }


   /// <summary>
   /// Accepts a boolean literal.
   /// </summary>
   private bool AcceptBooleanLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      if (!Accept(TokenType.BooleanLiteral, out StringView booleanLiteral))
      {
         node = null;
         return false;
      }

      bool isTrue = booleanLiteral == "true";

      node = new LiteralNode<bool>(isTrue);
      return false;
   }

   /// <summary>
   /// This method uses <see cref="long.TryParse"/> to evaluate the integer
   /// literal. This means that the integer literal is limited to what C# can
   /// parse.
   /// </summary>
   private bool AcceptIntegerLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      if (!Accept(TokenType.NumericLiteral, out StringView integerLiteral))
      {
         node = null;
         return false;
      }

      if (!long.TryParse(integerLiteral.Span, out long value))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition,
            "Invalid integer literal. Integer literals are limited to what C# can parse.");
      }

      node = new LiteralNode<long>(value);
      return true;
   }

   /// <summary>
   /// This method uses <see cref="Regex.Unescape"/> to evaluate the string.
   /// This means that the string literal is to what this method can unescape.
   /// </summary>
   private bool AcceptStringLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      if (!Accept(TokenType.StringLiteral, out StringView stringLiteral))
      {
         node = null;
         return false;
      }

      if (stringLiteral.Length < 2 || stringLiteral[0] != '"' || stringLiteral[^1] != '"')
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition,
            "Invalid string literal. Don't use prefixes or suffixes.");
      }


      string str = Regex.Unescape(stringLiteral);
      node = new LiteralNode<string>(str);
      return true;
   }

   private void AcceptUntilCloseDelimiter(char openDelimiter, char closeDelimiter)
   {
      int count = 1;
      while (!IsEndOfFile)
      {
         if (AcceptExact(TokenType.Symbol, openDelimiter))
         {
            count++;
         }

         if (AcceptExact(TokenType.Symbol, closeDelimiter))
         {
            count--;
            if (count == 0)
            {
               return;
            }
         }

         m_Position++;
      }

      if (IsEndOfFile)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Expected \"{closeDelimiter}\".");
      }
   }

   private void SkipUntilNextEngineHeader()
   {
      ReadOnlySpan<char> openDelimiters = "{[(";
      ReadOnlySpan<char> closeDelimiters = "}])";

      while (!IsEndOfFile)
      {
         if (m_DelimiterStack.Count == 0 && CurrentToken.Type == TokenType.HeaderMacro)
         {
            return;
         }

         ReadOnlySpan<char> currentTokenValue = CurrentTokenValue;
         if (currentTokenValue.Length != 1)
         {
            m_Position++;
            continue;
         }

         bool isOpenDelimiter = false;
         for (int i = 0; i < openDelimiters.Length; i++)
         {
            if (openDelimiters[i] == currentTokenValue[0])
            {
               isOpenDelimiter = true;
               m_DelimiterStack.Push(closeDelimiters[i]);
            }
         }

         if (!isOpenDelimiter && m_DelimiterStack.TryPeek(out char delimiter) && currentTokenValue[0] == delimiter)
         {
            m_DelimiterStack.Pop();
         }

         m_Position++;
      }

      if (m_DelimiterStack.Count > 0)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
      }
   }

   private void SkipUntil(ReadOnlySpan<char> stopCharacters, ReadOnlySpan<char> openDelimiters, ReadOnlySpan<char> closeDelimiters)
   {
      if (openDelimiters.Length != closeDelimiters.Length)
      {
         throw new ArgumentException("Open and close delimiters must have the same length.", nameof(closeDelimiters));
      }

      while (!IsEndOfFile)
      {
         ReadOnlySpan<char> currentTokenValue = CurrentTokenValue;
         if (currentTokenValue.Length != 1)
         {
            m_Position++;
            continue;
         }

         if (m_DelimiterStack.Count == 0 && stopCharacters.Contains(currentTokenValue[0]))
         {
            return;
         }

         bool isOpenDelimiter = false;
         for (int i = 0; i < openDelimiters.Length; i++)
         {
            if (openDelimiters[i] == currentTokenValue[0])
            {
               isOpenDelimiter = true;
               m_DelimiterStack.Push(closeDelimiters[^-i]);
            }
         }

         if (!isOpenDelimiter && m_DelimiterStack.TryPeek(out char delimiter) && currentTokenValue[0] == delimiter)
         {
            m_DelimiterStack.Pop();
         }

         m_Position++;
      }

      if (m_DelimiterStack.Count > 0)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
      }
   }

   private void SkipDelimiters(ReadOnlySpan<char> openDelimiters, ReadOnlySpan<char> closeDelimiters)
   {
      if (openDelimiters.Length != closeDelimiters.Length)
      {
         throw new ArgumentException("Open and close delimiters must have the same length.", nameof(closeDelimiters));
      }

      while (!IsEndOfFile)
      {
         bool isOpenDelimiter = false;
         for (int i = 0; i < openDelimiters.Length; i++)
         {
            if (AcceptExact(TokenType.Symbol, openDelimiters[i]))
            {
               isOpenDelimiter = true;
               m_DelimiterStack.Push(closeDelimiters[i]);
               break;
            }
         }

         if (!isOpenDelimiter && AcceptExact(TokenType.Symbol, m_DelimiterStack.Peek()))
         {
            m_DelimiterStack.Pop();
         }

         m_Position++;
      }

      if (m_DelimiterStack.Count > 0)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
      }
   }

   private void SkipDelimiters(char openChar = '{', char closeChar = '}')
   {
      if (!TrySkipDelimiters(openChar, closeChar))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, "Expected \"{\".");
      }
   }

   private bool TrySkipDelimiters(char openChar = '{', char closeChar = '}')
   {
      if (!AcceptExact(TokenType.Symbol, openChar))
      {
         return false;
      }

      int scopeCount = 1;
      while (!IsEndOfFile && scopeCount > 0)
      {
         if (AcceptExact(TokenType.Symbol, openChar))
         {
            scopeCount++;
            continue;
         }

         if (AcceptExact(TokenType.Symbol, closeChar))
         {
            scopeCount--;
            continue;
         }

         m_Position++;
      }

      if (scopeCount != 0)
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, $"Expected \"{closeChar}\".");
      }

      return true;
   }

   private void ExpectExact(TokenType type, ReadOnlySpan<char> value, string? message = null)
   {
      if (!AcceptExact(type, value))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, message ?? "Unexpected token.");
      }
   }

   private bool AcceptAny(TokenType type)
   {
      if ((CurrentTokenType & type) != 0)
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private bool AcceptExact(TokenType type, out ReadOnlySpan<char> token)
   {
      if ((CurrentTokenType & type) != 0)
      {
         token = CurrentTokenValue;
         m_Position++;
         return true;
      }

      token = default;
      return false;
   }

   private bool AcceptExact(TokenType type, char c)
   {
      if (!IsEndOfFile && CurrentTokenType == type && CurrentTokenValue[0] == c)
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private bool AcceptExact
   (
      TokenType bothType,
      ReadOnlySpan<char> firstValue,
      ReadOnlySpan<char> secondValue,
      StringComparison comparison = StringComparison.Ordinal
   )
   {
      int index = m_Position;
      if (AcceptExact(bothType, firstValue, comparison) && AcceptExact(bothType, secondValue, comparison))
      {
         return true;
      }

      m_Position = index;
      return false;
   }

   private bool AcceptExact
   (
      TokenType firtType,
      ReadOnlySpan<char> firstValue,
      TokenType secondType,
      ReadOnlySpan<char> secondValue,
      StringComparison comparison = StringComparison.Ordinal
   )
   {
      int index = m_Position;
      if (AcceptExact(firtType, firstValue, comparison) && AcceptExact(secondType, secondValue, comparison))
      {
         return true;
      }

      m_Position = index;
      return false;
   }

   private void ExpectKeyword(ReadOnlySpan<char> value, string message)
   {
      if (!AcceptKeyword(value))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, message ?? $"Expected \"{value}\" keyword.");
      }
   }

   private bool AcceptKeywordSequence(ReadOnlySpan<char> firstValue, ReadOnlySpan<char> secondValue)
   {
      int index = m_Position;
      if (AcceptExact(TokenType.Keyword, firstValue) && AcceptExact(TokenType.Keyword, secondValue))
      {
         return true;
      }

      m_Position = index;
      return false;
   }

   private bool AcceptKeyword(ReadOnlySpan<char> value)
   {
      return AcceptExact(TokenType.Keyword, value);
   }

   private bool AcceptEngineHeader(ReadOnlySpan<char> header)
   {
      return AcceptExact(TokenType.HeaderMacro, header);
   }

   private bool Peek(TokenType type)
   {
      if (!IsEndOfFile && (CurrentTokenType & type) != 0)
      {
         return true;
      }

      return false;
   }

   private bool PeekExact(TokenType type, ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      if (!IsEndOfFile && CurrentTokenType == type && IsCurrentTokenExact(value, comparison))
      {
         return true;
      }

      return false;
   }

   private bool AcceptExact(TokenType type, ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      if (!IsEndOfFile && CurrentTokenType == type && IsCurrentTokenExact(value, comparison))
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private void Expect(TokenType type, out StringView tokenValue, string message)
   {
      if (!Accept(type, out tokenValue))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, message);
      }
   }

   private bool Accept(TokenType validTypes, out StringView value)
   {
      if ((CurrentTokenType & validTypes) != 0)
      {
         value = CurrentTokenValue;
         m_Position++;
         return true;
      }

      value = default;
      return false;
   }

   private void ExpectFormat(TokenType type, string format, string arg0)
   {
      if (!Accept(type))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, string.Format(format, arg0));
      }
   }

   private void Expect(TokenType type, string message)
   {
      if (!Accept(type))
      {
         throw CreateIllFormedCodeException(CurrentTokenTextPosition, message);
      }
   }

   private bool Accept(TokenType type)
   {
      if ((CurrentTokenType & type) != 0)
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private bool IsCurrentTokenExact(ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      return CurrentTokenValue.Equals(value, comparison);
   }

   [GeneratedRegex(@"^[A-Z][A-Z0-9_]*_API$", RegexOptions.Compiled)]
   private static partial Regex GetImportExportClassAttributeRegex();

   private IllFormedCodeException CreateIllFormedCodeException(int position, string message)
   {
      return new IllFormedCodeException
      (
         position,
         SourceFileTextPositionType.TextPosition,
         m_TextPositionMap,
         m_SourceFilePath,
         m_SourceFileText,
         message
      );
   }
}
