using System;

namespace UndefinedBuildTool;

public readonly struct PremakeIndentationScope : IDisposable
{
   private readonly PremakeFileGenerationContext m_Context;

   public PremakeIndentationScope(PremakeFileGenerationContext ctx)
   {
      m_Context = ctx;
      m_Context.IncreaseIndentation();
   }

   public void Dispose()
   {
      m_Context.DecreaseIndentation();
   }
}
