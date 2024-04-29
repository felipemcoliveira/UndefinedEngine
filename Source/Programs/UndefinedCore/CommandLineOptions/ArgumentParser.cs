using System;
using System.Reflection;

namespace UndefinedCore;

public abstract class ArgumentParser
{
   public abstract object Parse(string arg, MemberInfo targetMember);

   public virtual void Validate(string arg, MemberInfo targetMember)
   {

   }

   public static Type GetMemberTypeWithoutNullable(MemberInfo member)
   {
      if (member is PropertyInfo property)
      {
         return GetNullableType(property.PropertyType);
      }
      else if (member is FieldInfo field)
      {
         return GetNullableType(field.FieldType);
      }
      else
      {
         throw new InvalidOperationException("Member is not a property or field.");
      }
   }

   private static Type GetNullableType(Type type)
   {
      Type? underlyingType = Nullable.GetUnderlyingType(type);
      if (underlyingType != null)
      {
         return underlyingType;
      }

      return type;
   }
}
