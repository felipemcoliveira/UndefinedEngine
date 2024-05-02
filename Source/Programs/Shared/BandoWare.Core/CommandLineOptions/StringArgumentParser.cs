using System.Reflection;

namespace BandoWare.Core;

public class StringArgumentParser : ArgumentParser
{
   public override object Parse(string arg, MemberInfo targetMember)
   {
      return arg;
   }
}
