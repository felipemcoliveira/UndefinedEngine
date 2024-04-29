using System.Reflection;

namespace UndefinedCore;

public class BooleanArgumentParser : ArgumentParser
{
   public override object Parse(string arg, MemberInfo targetMember)
   {
      if (arg.Equals("0"))
      {
         return false;
      }

      if (arg.Equals("1"))
      {
         return true;
      }

      return bool.Parse(arg);
   }
}
