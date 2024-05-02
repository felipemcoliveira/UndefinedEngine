using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{Name,nq}")]
public class CppSpecifierNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
