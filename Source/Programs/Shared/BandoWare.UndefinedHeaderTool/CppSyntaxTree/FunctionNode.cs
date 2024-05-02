using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UFUNCTION {Identifier,nq}")]
public class FunctionNode(StringView name) : SyntaxNode
{
   public StringView Name { get; } = name;
   public bool IsStatic { get; set; }
   public bool IsConstExpr { get; set; }
   public bool IsVirtual { get; set; }
}
