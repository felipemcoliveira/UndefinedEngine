using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UCLASS {Name,nq}")]
public class CppClassNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
