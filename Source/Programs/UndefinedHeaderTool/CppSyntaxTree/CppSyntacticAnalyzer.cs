using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using UndefinedCore;

using Range = UndefinedCore.Range;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UFUNCTION (Parameter) {Identifier,nq}")]
public class CppFunctionParameterNode
(
   Range tokensRange,
   CppToken identifierToken,
   int parameterIndex,
   CppLexicalAnalysis lexicalAnalysis
)
: CppSyntaxNode(tokensRange, lexicalAnalysis)
{
   public CppToken IdentifierToken { get; } = identifierToken;
   public int ParameterIndex { get; set; } = parameterIndex;

   public string Identifier
   {
      get
      {
         string identifier = LexicalAnalysis.GetTokenValue(IdentifierToken);
         CppFunctionNode functionNode = (CppFunctionNode)Parent!;
         Debug.Assert(functionNode != null);

         return $"{functionNode.Identifier}::{identifier} [{ParameterIndex}]";
      }
   }
}

[DebuggerDisplay("UFUNCTION {Identifier,nq}")]
public class CppFunctionNode(Range tokensRange, CppToken identifierToken, CppLexicalAnalysis lexicalAnalysis)
   : CppSyntaxNode(tokensRange, lexicalAnalysis)
{
   public CppToken IdentifierToken { get; } = identifierToken;

   public bool IsStatic { get; set; }
   public bool IsConstExpr { get; set; }
   public bool IsVirtual { get; set; }

   public string Identifier
   {
      get
      {
         string identifier = LexicalAnalysis.GetTokenValue(IdentifierToken);
         if (Parent is CppClassNode classNode)
         {
            return $"{classNode.Identifier}::{identifier}";
         }

         return identifier;
      }
   }
}

internal partial class CppSyntacticAnalyzer
{
   private CppTokenType CurrentTokenType => CurrentToken.Type;
   private bool IsEndOfFile => m_Tokens[m_CurrentTokenIndex].Type == CppTokenType.EndOfFile;
   private CppToken CurrentToken => m_Tokens[m_CurrentTokenIndex];
   private ReadOnlySpan<char> CurrentTokenValue => m_SourceCode.AsSpan(CurrentToken.StartPosition, CurrentToken.Length);
   private int CurrentTokenPosition => CurrentToken.StartPosition;

   private static readonly StringHashSet s_FunctionSpecifiers =
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

   private static readonly StringHashSet s_OverloadableOperators =
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
   private int m_CurrentTokenIndex;
   private Stack<char> m_DelimiterStack;

   // copied from lexical analysis for faster access
   private string m_SourceCode;
   private List<CppToken> m_Tokens;

   public CppSyntacticAnalyzer(CppLexicalAnalysis lexicalAnalysis)
   {
      m_LexicalAnalysis = lexicalAnalysis;
      m_CurrentTokenIndex = 0;
      m_SourceCode = lexicalAnalysis.SourceCode;
      m_Tokens = lexicalAnalysis.Tokens;
      m_DelimiterStack = [];
   }

   public CppSyntaxNode Analyze()
   {
      CppSyntaxNode root = new((0, m_LexicalAnalysis.Tokens.Count), m_LexicalAnalysis);

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
      int firstNodeIndex = m_CurrentTokenIndex;
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

      int identifierIndex = m_CurrentTokenIndex;
      if (hasImportExportAttribute)
      {
         string importExportAttribute = m_LexicalAnalysis.GetTokenValue(m_CurrentTokenIndex - 1);
         ExpectFormat(CppTokenType.Identifier, "Expected identifier after \"{0}\" keyword.", importExportAttribute);
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

      CppClassNode classNode = new((firstNodeIndex, m_CurrentTokenIndex), m_Tokens[identifierIndex], m_LexicalAnalysis);
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

         m_CurrentTokenIndex++;
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

   private bool AcceptClassAccessSpecifier(out CppAccessSpecifier accessSpecifier)
   {
      if (!IsEndOfFile)
      {
         accessSpecifier = CppAccessSpecifier.None;
         return false;
      }

      ReadOnlySpan<char> specifier = CurrentTokenValue;
      accessSpecifier = specifier switch
      {
         "public" => CppAccessSpecifier.Public,
         "protected" => CppAccessSpecifier.Protected,
         "private" => CppAccessSpecifier.Private,
         _ => CppAccessSpecifier.None,
      };

      if (accessSpecifier == CppAccessSpecifier.None)
      {
         accessSpecifier = CppAccessSpecifier.None;
         return false;
      }

      ExpectExact(CppTokenType.Symbol, ":", "Expected \":\" after access specifier.");
      return true;
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
      int firstNodeIndex = m_CurrentTokenIndex;
      if (!TryAcceptEngineHeadear("UFUNCTION", out CppEngineHeaderNode? engineHeaderNode))
      {
         node = null;
         return false;
      }

      HashSet<string> functionSpecifiers = [];
      while (!IsEndOfFile)
      {
         if (!AcceptAny(CppTokenType.Keyword, s_FunctionSpecifiers, out string? specifier))
         {
            break;
         }

         functionSpecifiers.Add(specifier);
      }

      SkipUntil("(", "{(<", ">)}");
      int identifierTokenIndex = m_CurrentTokenIndex - 1;
      if (!IsEndOfFile && m_Tokens[identifierTokenIndex].Type != CppTokenType.Identifier)
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected function name.");
      }

      CppToken identifierToken = m_Tokens[identifierTokenIndex];
      Range tokensRange = (firstNodeIndex, m_CurrentTokenIndex);
      CppFunctionNode functionNode = new(tokensRange, identifierToken, m_LexicalAnalysis);

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

   private bool AcceptAny(CppTokenType validTokenTypes, StringHashSet set, [NotNullWhen(true)] out string? acceptedValue)
   {
      if (IsEndOfFile && (CurrentTokenType & validTokenTypes) == 0)
      {
         acceptedValue = null;
         return false;
      }

      ReadOnlySpan<char> value = CurrentTokenValue;
      if (!set.Contains(value, out acceptedValue))
      {
         return false;
      }

      m_CurrentTokenIndex++;
      return true;
   }

   private bool AcceptImportExportClassAttribute()
   {
      Regex importExportAttributeRegex = GetImportExportClassAttributeRegex();
      bool hasImportExportAttribute = false;
      if (!IsEndOfFile && importExportAttributeRegex.IsMatch(CurrentTokenValue))
      {
         hasImportExportAttribute = true;
         m_CurrentTokenIndex++;
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
      int firstNodeIndex = m_CurrentTokenIndex;
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

      int identifierIndex = m_CurrentTokenIndex;
      Expect(CppTokenType.Identifier, "Expected identifier after \"enum\" keyword.");
      CppEnumNode enumNode = new((firstNodeIndex, m_CurrentTokenIndex), m_Tokens[identifierIndex], m_LexicalAnalysis);

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
      int startTokenIndex = m_CurrentTokenIndex;
      bool hasEngineHeader = TryAcceptEngineHeadear("UMETA", out CppEngineHeaderNode? engineHeaderNode);

      int identifierTokenIndex = m_CurrentTokenIndex;
      if (!Accept(CppTokenType.Identifier))
      {
         if (hasEngineHeader)
         {
            throw new CppIllFormedCodeException(CurrentTokenPosition, "Expected identifier after UMETA header.");
         }

         m_CurrentTokenIndex = startTokenIndex;
         node = null;
         return false;
      }

      CppEnumItemNode enumItemNode = new
      (
         (identifierTokenIndex, m_CurrentTokenIndex),
         m_Tokens[identifierTokenIndex],
         m_LexicalAnalysis
      );

      enumItemNode.AddChild(engineHeaderNode);

      if (AcceptExact(CppTokenType.Symbol, "="))
      {
         SkipUntil(",}", "{(<", ">)}");
      }

      node = enumItemNode;
      return true;
   }

   private bool TryAcceptEngineHeadear(string headerName, [NotNullWhen(true)] out CppEngineHeaderNode? engineHeaderNode)
   {
      int startTokenIndex = m_CurrentTokenIndex;
      if (!AcceptExact(CppTokenType.EngineHeader, headerName))
      {
         engineHeaderNode = null;
         return false;
      }

      engineHeaderNode = new(Range.Invalid, m_LexicalAnalysis);
      ExpectExact(CppTokenType.Symbol, "(", $"Expected \"(\" after \"{headerName}\".");

      while (!IsEndOfFile)
      {
         int firstSpecifierTokenIndex = m_CurrentTokenIndex;
         if (AcceptExact(CppTokenType.Identifier, out ReadOnlySpan<char> specifierName))
         {
            Range valueRange = Range.Invalid;
            if (AcceptExact(CppTokenType.Symbol, "="))
            {
               int firstTokenIndex = m_CurrentTokenIndex;
               SkipUntil(",)", "({<", ">})");
               int lastTokenIndex = m_CurrentTokenIndex - 1;

               valueRange = new(firstTokenIndex, lastTokenIndex);
            }

            engineHeaderNode.AddChild(new CppSpecifierNode
            (
               (firstSpecifierTokenIndex, m_CurrentTokenIndex),
               m_Tokens[firstSpecifierTokenIndex],
               valueRange, m_LexicalAnalysis
            ));

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

         throw new CppIllFormedCodeException(CurrentTokenPosition, $"Unexpected token after \"{headerName}\".");
      }

      engineHeaderNode.TokensRange = (startTokenIndex, m_CurrentTokenIndex);
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

         m_CurrentTokenIndex++;
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
            m_CurrentTokenIndex++;
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

         m_CurrentTokenIndex++;
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
            m_CurrentTokenIndex++;
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

         m_CurrentTokenIndex++;
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

         m_CurrentTokenIndex++;
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

         m_CurrentTokenIndex++;
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
         m_CurrentTokenIndex++;
         return true;
      }

      return false;
   }

   private bool AcceptExact(CppTokenType type, out ReadOnlySpan<char> token)
   {
      if ((CurrentTokenType & type) != 0)
      {
         token = CurrentTokenValue;
         m_CurrentTokenIndex++;
         return true;
      }

      token = default;
      return false;
   }

   private bool AcceptExact(CppTokenType type, char c)
   {
      if (!IsEndOfFile && CurrentTokenType == type && CurrentTokenValue[0] == c)
      {
         m_CurrentTokenIndex++;
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
      int index = m_CurrentTokenIndex;
      if (AcceptExact(bothType, firstValue, comparison) && AcceptExact(bothType, secondValue, comparison))
      {
         return true;
      }

      m_CurrentTokenIndex = index;
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
      int index = m_CurrentTokenIndex;
      if (AcceptExact(firtType, firstValue, comparison) && AcceptExact(secondType, secondValue, comparison))
      {
         return true;
      }

      m_CurrentTokenIndex = index;
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
      int index = m_CurrentTokenIndex;
      if (AcceptExact(CppTokenType.Keyword, firstValue) && AcceptExact(CppTokenType.Keyword, secondValue))
      {
         return true;
      }

      m_CurrentTokenIndex = index;
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
         m_CurrentTokenIndex++;
         return true;
      }

      return false;
   }

   private void Expect(CppTokenType type, out ReadOnlySpan<char> token, string message)
   {
      if (!Accept(type, out token))
      {
         throw new CppIllFormedCodeException(CurrentTokenPosition, message);
      }
   }

   private bool Accept(CppTokenType validTypes, out ReadOnlySpan<char> token)
   {
      if ((CurrentTokenType & validTypes) != 0)
      {
         token = CurrentTokenValue;
         m_CurrentTokenIndex++;
         return true;
      }

      token = default;
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
         m_CurrentTokenIndex++;
         return true;
      }

      return false;
   }

   private bool IsCurrentTokenExact(ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal)
   {
      if (MemoryExtensions.CompareTo(CurrentTokenValue, value, comparison) == 0)
      {
         return true;
      }

      return false;
   }

   [GeneratedRegex(@"^[A-Z][A-Z0-9_]*_API$", RegexOptions.Compiled)]
   private static partial Regex GetImportExportClassAttributeRegex();
}
