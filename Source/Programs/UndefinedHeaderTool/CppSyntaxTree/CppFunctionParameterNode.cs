using System.Diagnostics;
using UndefinedCore;

namespace UndefinedHeader.SyntaxTree;

[DebuggerDisplay("UFUNCTION (Parameter) {Identifier,nq}")]
public class CppFunctionParameterNode(StringView identifier, int parameterIndex) : CppSyntaxNode
{
   public StringView Identifier { get; } = identifier;
   public int ParameterIndex { get; set; } = parameterIndex;
}
