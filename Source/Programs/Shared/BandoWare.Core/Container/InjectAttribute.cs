using System;

namespace BandoWare.Core;

[AttributeUsage
(
    AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field,
    Inherited = false,
    AllowMultiple = false
)]
sealed class InjectAttribute : Attribute
{
   public object? Key { get; set; }

   public InjectAttribute()
   {
      Key = null;
   }

   public InjectAttribute(object key)
   {
      Key = key;
   }
}
