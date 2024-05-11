namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public abstract class CppLiteralNode : Node
{
}

public class LiteralNode<T>(T value) : CppLiteralNode
{
   public T Value { get; } = value;
}

