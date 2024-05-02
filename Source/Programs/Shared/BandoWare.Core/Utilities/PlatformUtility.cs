using System;

namespace BandoWare.Core;

public static class PlatformUtility
{
   public static Platform GetHostPlatform()
   {
      return Environment.OSVersion.Platform switch
      {
         PlatformID.Win32NT => Platform.Windows,
         PlatformID.Unix => Platform.Linux,
         PlatformID.MacOSX => Platform.MacOS,
         _ => throw new NotSupportedException("Unsupported platform.")
      };
   }
}
