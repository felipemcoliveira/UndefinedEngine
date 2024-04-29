using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UndefinedCore;

public enum Architecture
{
   x86,
   x64,
   ARM,
   ARM64
}

public enum Configuration
{
   Debug,
   Release,
   Distribution
}

public abstract class Compiler
{
   private readonly Dictionary<string, string> m_Definitions = [];
   private readonly List<string> m_IncludePaths = [];
   private readonly List<string> m_SourceFiles = [];

   public abstract Task Compile(TargetInfo buildTarget);

   public virtual void AddDefinition(string name, string value)
   {
      if (!IsValidDefinition(name))
      {
         throw new ArgumentException($"Invalid definition name: {name}", nameof(name));
      }

      m_Definitions[name] = value;
   }

   public void AddDefinition(string name)
   {
      AddDefinition(name, "1");
   }

   public virtual void AddIncludePath(string path)
   {
      m_IncludePaths.Add(path);
   }

   public virtual void AddSourceFile(string filePath)
   {
      m_SourceFiles.Add(filePath);
   }

   protected static bool IsValidDefinition(string definition)
   {
      if (string.IsNullOrEmpty(definition))
      {
         return false;
      }

      if (!char.IsLetter(definition[0]) && definition[0] != '_')
      {
         return false;
      }

      for (int i = 1; i < definition.Length; i++)
      {
         if (!char.IsLetterOrDigit(definition[i]) && definition[i] != '_' && definition[i] != '=')
         {
            return false;
         }

         if (definition[i] == '=')
         {
            break;
         }
      }

      return true;
   }
}

//public class MSVCCompiler : Compiler
//{
//   public Version VisualStudioVersion { get; set; }
//   public Version CompilerVersion { get; set; }

//   public override Task Compile(TargetInfo buildTarget)
//   {

//   }
//}

public class TargetInfo
{
   public Architecture Architecture { get; }
   public Configuration Configuration { get; }
   public Platform Platform { get; }
   public string ProjectPath { get; }

   public TargetInfo(Architecture architecture, Configuration configuration, Platform platform, string projectPath)
   {
      Architecture = architecture;
      Configuration = configuration;
      Platform = platform;
      ProjectPath = projectPath;
   }
}

public abstract class Module
{
   public string Name { get; }
   public Guid Guid { get; }
   public string Path { get; }

   public List<Module> Dependencies { get; }

   public void OnCompile(Compiler compiler)
   {

   }
}

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
