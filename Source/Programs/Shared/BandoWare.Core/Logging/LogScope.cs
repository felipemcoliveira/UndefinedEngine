namespace BandoWare.Core;

public struct LogScope(string name)
{
   public string Name { get; set; } = name;

   public static implicit operator LogScope(string name)
   {
      return new LogScope(name);
   }
}
