using System.Reflection;

namespace UndefinedCore;

public class StringArgumentParser : ArgumentParser
{
   public override object Parse(string arg, MemberInfo targetMember)
   {
      return arg;
   }
}
