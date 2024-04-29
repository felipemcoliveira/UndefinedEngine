﻿using System;

namespace UndefinedCore;

public class TypeUtility
{
   public static Type GetUderlyingType(Type type)
   {
      Type? underlyingType = Nullable.GetUnderlyingType(type);
      return underlyingType ?? type;
   }
}
