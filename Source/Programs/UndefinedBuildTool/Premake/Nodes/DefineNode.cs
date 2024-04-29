using System.Collections.Generic;

namespace UndefinedBuildTool;

public class DefineNode : PremakeNode
{
   private readonly Dictionary<string, object> m_Defines = [];

   public DefineNode(IEnumerable<(string Name, object Value)> defines)
   {
      foreach ((string name, object value) in defines)
         m_Defines.Add(name, value);
   }

   public void Add(string key, bool value)
   {
      m_Defines.Add(key, value);
   }

   public override void Write(PremakeFileGenerationContext ctx)
   {
      ctx.AppendIndentation();
      ctx.AppendLine("defines {");

      using (ctx.CreateIndentationScope())
      {
         ctx.AppendJoin(m_Defines, CommaWithNewLine, static (ctx, define) =>
         {
            ctx.AppendIndentation();
            ctx.AppendFormat("\"{0}=%{1}\"", define.Key, define.Value);
         });
      }

      ctx.AppendLine();
      ctx.AppendIndentation();
      ctx.AppendLine("}");
   }
}
