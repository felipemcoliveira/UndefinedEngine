using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UENUM {Name,nq}")]
public class CppEnumNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
