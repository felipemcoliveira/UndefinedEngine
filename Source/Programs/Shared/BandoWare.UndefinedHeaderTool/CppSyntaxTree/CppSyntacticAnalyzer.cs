using BandoWare.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

internal partial class CppSyntacticAnalyzer
{
   private CppToken CurrentToken => m_Tokens[m_Position];
   private CppTokenType CurrentTokenType => CurrentToken.Type;
   private bool IsEndOfFile => CurrentToken.Type == CppTokenType.EndOfFile;
   private StringView CurrentTokenValue => CurrentToken.ValueView;
   private int CurrentTokenPosition => CurrentToken.StartPosition;

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

   private readonly CppLexicalAnalysis m_LexicalAnalysis;
   private int m_Position;
   private Stack<char> m_DelimiterStack;

   // copied from lexical analysis for faster access
   private string m_SourceCode;
   private List<CppToken> m_Tokens;

   public CppSyntacticAnalyzer(CppLexicalAnalysis lexicalAnalysis)
   {
      m_LexicalAnalysis = lexicalAnalysis;
      m_Position = 0;
      m_SourceCode = lexicalAnalysis.SourceCode;
      m_Tokens = lexicalAnalysis.Tokens;
      m_DelimiterStack = [];
   }

   public CppSyntaxNode Analyze()
   {
      CppSyntaxNode root = new();

      SkipUntilNextEngineHeader();
      while (!IsEndOfFile)
      {
         if (AcceptDeclaration(out CppSyntaxNode? node))
         {
            root.AddChild(node);
            continue;
         }

         SkipUntilNextEngineHeader();
      }

      return root;
   }

   public bool AcceptDeclaration(out CppSyntaxNode? node)
   {
      if (IsEndOfFile || CurrentTokenType != CppTokenType.EngineHeader)
      {
         node = null;
         return false;
      }

      return AcceptClassDeclaration(out node) || AcceptEnumDeclaration(out node);
   }

   private bool AcceptClassDeclaration(out CppSyntaxNode? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UCLASS", out CppEngineHeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      if (!AcceptKeyword("class"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected \"class\" keyword.");
      }

      bool hasImportExportAttribute = AcceptImportExportClassAttribute();

      StringView classIdentifier = CurrentTokenValue;
      if (hasImportExportAttribute)
      {
         ExpectFormat(CppTokenType.Identifier, "Expected identifier after \"{0}\" keyword.", m_Tokens[m_Position - 1].ValueView);
      }
      else
      {
         Expect(CppTokenType.Identifier, "Expected identifier after \"class\" keyword.");
      }

      // base class
      if (AcceptExact(CppTokenType.Symbol, ":"))
      {
         // TODO: parse base classes
         SkipUntil("{;", "{(<", ">)}");
      }

      // forward declaration
      if (AcceptExact(CppTokenType.Symbol, ";"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "UCLASS should be used only with class definition.");
      }

      CppClassNode classNode = new(CurrentTokenValue);
      classNode.AddChild(engineHeaderNode);

      ExpectExact(CppTokenType.Symbol, "{", "Expected \"{\" after class declaration");
      ExpectGeneratedBodyMacro();

      int curlyBraceCount = 0;
      while (!IsEndOfFile)
      {
         if (AcceptClassMemberDeclaration(out CppSyntaxNode? classMemberNode))
         {
            classNode.AddChild(classMemberNode);
            continue;
         }

         if (TrySkipDelimiters('{', '}'))
         {
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, "}"))
         {
            curlyBraceCount--;
            if (curlyBraceCount == 0)
            {
               ExpectExact(CppTokenType.Symbol, ";", "Expected \";\" after class declaration");
               break;
            }
         }

         m_Position++;
      }

      node = classNode;
      return true;
   }

   private void ExpectGeneratedBodyMacro()
   {
      if (!AcceptExact(CppTokenType.Identifier, "GENERATED_BODY")
         || !AcceptExact(CppTokenType.Symbol, "(")
         || !AcceptExact(CppTokenType.Symbol, ")"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected GENERATED_BODY(). " +
            "It should be the first thing in the class body, even before access specifiers).");
      }
   }

   private bool AcceptClassMemberDeclaration(out CppSyntaxNode? node)
   {
      if (IsEndOfFile || CurrentTokenType != CppTokenType.EngineHeader)
      {
         node = null;
         return false;
      }

      return AcceptFunctionDeclaration(out node);
   }

   private bool AcceptFunctionDeclaration(out CppSyntaxNode? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UFUNCTION", out CppEngineHeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      HashSet<StringView> functionSpecifiers = [];
      while (!IsEndOfFile)
      {
         if (!AcceptAny(CppTokenType.Keyword, s_FunctionSpecifiers, out StringView specifier))
         {
            break;
         }

         functionSpecifiers.Add(specifier);
      }

      SkipUntil("(", "{(<", ">)}");
      int identifierTokenIndex = m_Position - 1;
      if (!IsEndOfFile && m_Tokens[identifierTokenIndex].Type != CppTokenType.Identifier)
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected function name.");
      }

      StringView functionIdentifier = CurrentTokenValue;
      CppFunctionNode functionNode = new(functionIdentifier);

      functionNode.AddChild(engineHeaderNode);
      functionNode.IsVirtual = functionSpecifiers.Contains("virtual");
      functionNode.IsStatic = functionSpecifiers.Contains("static");
      functionNode.IsConstExpr = functionSpecifiers.Contains("constexpr");

      ExpectExact(CppTokenType.Symbol, "(", "Expected \"(\" after function name.");

      foreach (CppFunctionParameterNode parameter in ConsumeFunctionParameters())
      {
         functionNode.AddChild(parameter);
      }

      ExpectExact(CppTokenType.Symbol, ")", "Expected \")\" after parameters list.");

      SkipUntil("{;", "{(<", ">)}");

      if (!TrySkipDelimiters('{', '}') && !AcceptExact(CppTokenType.Symbol, ";"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected \";\" or function body after function declaration.");
      }

      node = functionNode;
      return true;
   }

   private IEnumerable<CppFunctionParameterNode> ConsumeFunctionParameters()
   {
      if (AcceptKeyword("void"))
      {
         yield break;
      }

      while (!IsEndOfFile)
      {

      }
   }

   private bool AcceptAny(CppTokenType validTokenTypes, HashSet<StringView> set, out StringView acceptedValue)
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
      if (!IsEndOfFile || CurrentToken.Type != CppTokenType.Identifier)
      {
         return false;
      }

      ReadOnlySpan<char> identifier = CurrentTokenValue;

      return false;
   }

   private bool AcceptEnumDeclaration(out CppSyntaxNode? node)
   {
      int firstNodeIndex = m_Position;
      if (!TryAcceptEngineHeadear("UENUM", out CppEngineHeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      if (!AcceptKeyword("enum"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected \"enum\" keyword.");
      }

      _ = AcceptKeyword("class") || AcceptKeyword("struct");

      Expect(CppTokenType.Identifier, out StringView identifier, "Expected identifier after \"enum\" keyword.");
      CppEnumNode enumNode = new(identifier);

      // type identifier
      if (AcceptExact(CppTokenType.Symbol, ":"))
      {
         SkipUntil("{;", "{(<", ">)}");
      }

      // forward declaration
      if (AcceptExact(CppTokenType.Symbol, ";"))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "UENUM should be used only with enum definition.");
      }

      enumNode.AddChild(engineHeaderNode);

      ExpectExact(CppTokenType.Symbol, "{", "Expected \"{\" after enum declaration.");

      while (!IsEndOfFile)
      {
         if (TryAcceptEnumItem(out CppSyntaxNode? enumItemNode))
         {
            enumNode.AddChild(enumItemNode);
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, ","))
         {
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, "}"))
         {
            break;
         }

         throw new CppIllFormedCodeException(CurrentTokenPosition, "Unexpected token.");
      }

      ExpectExact(CppTokenType.Symbol, ";", "Expected \";\" after enum declaration.");

      node = enumNode;
      return true;
   }

   private bool TryAcceptEnumItem(out CppSyntaxNode? node)
   {
      int startTokenIndex = m_Position;
      bool hasEngineHeader = TryAcceptEngineHeadear("UMETA", out CppEngineHeaderNode? engineHeaderNode);

      if (!Accept(CppTokenType.Identifier, out StringView identifier))
      {
         if (hasEngineHeader)
         {
            throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected identifier after UMETA header.");
         }

         m_Position = startTokenIndex;
         node = null;
         return false;
      }

      CppEnumItemNode enumItemNode = new(identifier);
      enumItemNode.AddChild(engineHeaderNode);

      if (AcceptExact(CppTokenType.Symbol, "="))
      {
         SkipUntil(",}", "{(<", ">)}");
      }

      node = enumItemNode;
      return true;
   }

   private bool TryAcceptEngineHeadear
   (
      string headerName,
      [NotNullWhen(true)] out CppEngineHeaderNode? headerNode
   )
   {
      if (!AcceptExact(CppTokenType.EngineHeader, headerName))
      {
         headerNode = null;
         return false;
      }

      headerNode = new(headerName);
      ExpectExact(CppTokenType.Symbol, "(", $"Expected \"(\" after \"{headerName}\".");

      while (!IsEndOfFile)
      {
         if (AcceptHeaderSpecifier(out CppHeaderSpecifierNode? specifierNode))
         {
            headerNode.AddChild(specifierNode);
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, ","))
         {
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, ")"))
         {
            break;
         }

         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Unexpected character \"{CurrentTokenValue}\".");
      }

      return true;
   }

   private bool AcceptHeaderSpecifier(out CppHeaderSpecifierNode? specifierNode)
   {
      if (IsEndOfFile || Accept(CppTokenType.Identifier, out StringView specifierName))
      {
         specifierNode = null;
         return false;
      }

      specifierNode = new CppHeaderSpecifierNode(specifierName);
      if (!AcceptExact(CppTokenType.Symbol, "="))
      {
         return true;
      }

      if (!AcceptLiteral(out CppLiteralNode? literalNode))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected literal after \"=\".");
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
      if (!Accept(CppTokenType.BooleanLiteral, out StringView booleanLiteral))
      {
         node = null;
         return false;
      }

      bool isTrue = booleanLiteral == "true";

      node = new CppLiteralNode<bool>(isTrue);
      return false;
   }

   /// <summary>
   /// This method uses <see cref="long.TryParse"/> to evaluate the integer
   /// literal. This means that the integer literal is limited to what C# can
   /// parse.
   /// </summary>
   private bool AcceptIntegerLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      if (!Accept(CppTokenType.NumericLiteral, out StringView integerLiteral))
      {
         node = null;
         return false;
      }

      if (!long.TryParse(integerLiteral.Span, out long value))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition,
            "Invalid integer literal. Integer literals are limited to what C# can parse.");
      }

      node = new CppLiteralNode<long>(value);
      return true;
   }

   /// <summary>
   /// This method uses <see cref="Regex.Unescape"/> to evaluate the string.
   /// This means that the string literal is to what this method can unescape.
   /// </summary>
   private bool AcceptStringLiteral([NotNullWhen(true)] out CppLiteralNode? node)
   {
      if (!Accept(CppTokenType.StringLiteral, out StringView stringLiteral))
      {
         node = null;
         return false;
      }

      if (stringLiteral.Length < 2 || stringLiteral[0] != '"' || stringLiteral[^1] != '"')
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition,
            "Invalid string literal. Don't use prefixes or suffixes.");
      }


      string str = Regex.Unescape(stringLiteral);
      node = new CppLiteralNode<string>(str);
      return true;
   }

   private void AcceptUntilCloseDelimiter(char openDelimiter, char closeDelimiter)
   {
      int count = 1;
      while (!IsEndOfFile)
      {
         if (AcceptExact(CppTokenType.Symbol, openDelimiter))
         {
            count++;
         }

         if (AcceptExact(CppTokenType.Symbol, closeDelimiter))
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
         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Expected \"{closeDelimiter}\".");
      }
   }

   private void SkipUntilNextEngineHeader()
   {
      ReadOnlySpan<char> openDelimiters = "{[(";
      ReadOnlySpan<char> closeDelimiters = "}])";

      while (!IsEndOfFile)
      {
         if (m_DelimiterStack.Count == 0 && CurrentToken.Type == CppTokenType.EngineHeader)
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
         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
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
         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
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
            if (AcceptExact(CppTokenType.Symbol, openDelimiters[i]))
            {
               isOpenDelimiter = true;
               m_DelimiterStack.Push(closeDelimiters[i]);
               break;
            }
         }

         if (!isOpenDelimiter && AcceptExact(CppTokenType.Symbol, m_DelimiterStack.Peek()))
         {
            m_DelimiterStack.Pop();
         }

         m_Position++;
      }

      if (m_DelimiterStack.Count > 0)
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Expected \"{m_DelimiterStack.Peek()}\".");
      }
   }

   private void SkipDelimiters(char openChar = '{', char closeChar = '}')
   {
      if (!TrySkipDelimiters(openChar, closeChar))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected \"{\".");
      }
   }

   private bool TrySkipDelimiters(char openChar = '{', char closeChar = '}')
   {
      if (!AcceptExact(CppTokenType.Symbol, openChar))
      {
         return false;
      }

      int scopeCount = 1;
      while (!IsEndOfFile && scopeCount > 0)
      {
         if (AcceptExact(CppTokenType.Symbol, openChar))
         {
            scopeCount++;
            continue;
         }

         if (AcceptExact(CppTokenType.Symbol, closeChar))
         {
            scopeCount--;
            continue;
         }

         m_Position++;
      }

      if (scopeCount != 0)
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Expected \"{closeChar}\".");
      }

      return true;
   }

   private void ExpectExact(CppTokenType type, ReadOnlySpan<char> value, string? message = null)
   {
      if (!AcceptExact(type, value))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, message ?? "Unexpected token.");
      }
   }

   private bool AcceptAny(CppTokenType type)
   {
      if ((CurrentTokenType & type) != 0)
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private bool AcceptExact(CppTokenType type, out ReadOnlySpan<char> token)
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

   private bool AcceptExact(CppTokenType type, char c)
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
      CppTokenType bothType,
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
      CppTokenType firtType,
      ReadOnlySpan<char> firstValue,
      CppTokenType secondType,
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
         throw new CppIllFormedCodeException(CurrentTokenPosition, message ?? $"Expected \"{value}\" keyword.");
      }
   }

   private bool AcceptKeywordSequence(ReadOnlySpan<char> firstValue, ReadOnlySpan<char> secondValue)
   {
      int index = m_Position;
      if (AcceptExact(CppTokenType.Keyword, firstValue) && AcceptExact(CppTokenType.Keyword, secondValue))
      {
         return true;
      }

      m_Position = index;
      return false;
   }

   private bool AcceptKeyword(ReadOnlySpan<char> value)
   {
      return AcceptExact(CppTokenType.Keyword, value);
   }

   private bool AcceptEngineHeader(ReadOnlySpan<char> header)
   {
      return AcceptExact(CppTokenType.EngineHeader, header);
   }

   private bool Peek(CppTokenType type)
   {
      if (!IsEndOfFile && (CurrentTokenType & type) != 0)
      {
         return true;
      }

      return false;
   }

   private bool PeekExact(CppTokenType type, ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      if (!IsEndOfFile && CurrentTokenType == type && IsCurrentTokenExact(value, comparison))
      {
         return true;
      }

      return false;
   }

   private bool AcceptExact(CppTokenType type, ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      if (!IsEndOfFile && CurrentTokenType == type && IsCurrentTokenExact(value, comparison))
      {
         m_Position++;
         return true;
      }

      return false;
   }

   private void Expect(CppTokenType type, out StringView tokenValue, string message)
   {
      if (!Accept(type, out tokenValue))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, message);
      }
   }

   private bool Accept(CppTokenType validTypes, out StringView value)
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

   private void ExpectFormat(CppTokenType type, string format, string arg0)
   {
      if (!Accept(type))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, string.Format(format, arg0));
      }
   }

   private void Expect(CppTokenType type, string message)
   {
      if (!Accept(type))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, message);
      }
   }

   private bool Accept(CppTokenType type)
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
}
