namespace UndefinedBuildTool;

public abstract class PremakeFilterNode : PremakeNode
{
   public abstract string GetFilterString();

   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendFormatLine("filter \"{0}\"", GetFilterString());

      using (ctx.CreateIndentationScope())
      {
         foreach (PremakeNode child in Children)
            child.Write(ctx);
      }
   }
}
