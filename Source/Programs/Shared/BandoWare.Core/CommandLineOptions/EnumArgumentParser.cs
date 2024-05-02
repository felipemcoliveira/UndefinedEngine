using System;
using System.Reflection;

namespace BandoWare.Core;

public class EnumArgumentParser : ArgumentParser
{
   public override object Parse(string arg, MemberInfo targetMember)
   {
      Type targetType = GetMemberTypeWithoutNullable(targetMember);
      return Enum.Parse(targetType, arg, true);
   }
}
