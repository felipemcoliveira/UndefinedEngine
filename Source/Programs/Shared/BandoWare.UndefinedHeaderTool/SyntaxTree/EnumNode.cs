using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UENUM {Name,nq}")]
public class EnumNode(StringView name) : Node
{
   public StringView Name { get; private set; } = name;
}
