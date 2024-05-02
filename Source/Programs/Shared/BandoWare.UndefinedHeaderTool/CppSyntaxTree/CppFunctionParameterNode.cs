using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UFUNCTION (Parameter) {Name,nq}")]
public class CppFunctionParameterNode(StringView name, int parameterIndex) : CppSyntaxNode
{
   public StringView Name { get; } = name;
   public int ParameterIndex { get; set; } = parameterIndex;
}
