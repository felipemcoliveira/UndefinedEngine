namespace UndefinedBuildTool;

public class ProjectNode(string name) : PremakeNode
{
   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendFormatLine("project \"{0}\"", name);

      using (ctx.CreateIndentationScope())
      {
         foreach (PremakeNode child in Children)
            child.Write(ctx);
      }
   }
}
