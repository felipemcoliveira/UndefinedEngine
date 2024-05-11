using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UFUNCTION (Parameter) {Name,nq}")]
public class FunctionParameterNode(StringView name, int parameterIndex) : Node
{
   public StringView Name { get; } = name;
   public int ParameterIndex { get; set; } = parameterIndex;
}
