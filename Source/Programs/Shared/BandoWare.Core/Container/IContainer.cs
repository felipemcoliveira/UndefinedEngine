using System;

namespace BandoWare.Core;
public interface IContainer
{
   T Get<T>();
   T Get<T>(object key);
   T? GetNullable<T>() where T : class;
   T? GetNullable<T>(object key) where T : class;
   object Instantiate(Type type);
   T Instantiate<T>();
   void Set(object key, object value);
   void Set<TKey>(object value);
   bool TryGet<T>(object key, out T value);
}