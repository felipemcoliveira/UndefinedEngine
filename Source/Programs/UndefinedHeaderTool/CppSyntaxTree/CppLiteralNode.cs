namespace UndefinedHeader.SyntaxTree;

public abstract class CppLiteralNode : CppSyntaxNode
{
}

public class CppLiteralNode<T>(T value) : CppLiteralNode
{
   public T Value { get; } = value;
}

