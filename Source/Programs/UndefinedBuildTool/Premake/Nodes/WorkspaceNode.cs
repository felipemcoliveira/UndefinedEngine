namespace UndefinedBuildTool;

public class WorkspaceNode(string name) : PremakeNode
{
   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendFormatLine("workspace \"{0}\"", name);

      using (ctx.CreateIndentationScope())
      {
         foreach (PremakeNode child in Children)
            child.Write(ctx);
      }
   }
}
