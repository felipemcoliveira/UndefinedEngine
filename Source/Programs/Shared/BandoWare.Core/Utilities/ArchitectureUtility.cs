using System;
using System.Runtime.InteropServices;

namespace BandoWare.Core;

public static class ArchitectureUtility
{
   public static Architecture GetHostArchitecture()
   {
      return RuntimeInformation.ProcessArchitecture switch
      {
         System.Runtime.InteropServices.Architecture.X86 => Architecture.x86,
         System.Runtime.InteropServices.Architecture.X64 => Architecture.x64,
         System.Runtime.InteropServices.Architecture.Arm => Architecture.ARM,
         System.Runtime.InteropServices.Architecture.Arm64 => Architecture.ARM64,
         _ => throw new NotSupportedException("Unsupported architecture.")
      };
   }
}
