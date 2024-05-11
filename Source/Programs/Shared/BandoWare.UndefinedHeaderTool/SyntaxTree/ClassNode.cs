using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UCLASS {Name,nq}")]
public class ClassNode(StringView name) : Node
{
   public StringView Name { get; private set; } = name;
}
