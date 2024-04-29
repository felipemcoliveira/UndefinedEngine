namespace UndefinedBuildTool;

public class ConfigurationNode(string[] names) : PremakeNode
{
   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendLine("configurations {");

      using (ctx.CreateIndentationScope())
      {
         ctx.AppendJoin(names, CommaWithNewLine, static (ctx, name) =>
         {
            ctx.AppendIndentation();
            ctx.AppendFormat("\"{0}\"", name);
         });
      }

      ctx.AppendLine();
      ctx.AppendIndentation();
      ctx.AppendLine("}");
   }
}
