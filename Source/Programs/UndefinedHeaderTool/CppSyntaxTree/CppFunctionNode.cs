using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UFUNCTION {Identifier,nq}")]
public class CppFunctionNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; } = name;
   public bool IsStatic { get; set; }
   public bool IsConstExpr { get; set; }
   public bool IsVirtual { get; set; }
}
