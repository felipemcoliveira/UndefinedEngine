using System;

namespace UndefinedBuildTool;

public class SystemFilterNode(Platform platform) : PremakeFilterNode
{
   public override string GetFilterString()
   {
      return platform switch
      {
         Platform.Windows => "system:windows",
         Platform.MacOS => "system:macosx",
         Platform.Linux => "system:linux",
         _ => throw new ArgumentOutOfRangeException()
      };
   }
}
