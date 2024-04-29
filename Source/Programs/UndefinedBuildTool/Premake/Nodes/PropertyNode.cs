namespace UndefinedBuildTool;

public class PropertyNode(string name, string value) : PremakeNode
{
   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendFormatLine("{0} \"{1}\"", name, value);
   }
}
