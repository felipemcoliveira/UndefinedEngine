using System;
using System.Collections.Generic;
using System.Reflection;

namespace BandoWare.Core;

/// <summary>
/// DI Container.
/// </summary>
public class Container
{
   readonly Dictionary<object, object> m_Values = [];

   public Container()
   {
      Set<Container>(this);
   }

   public void Set<TKey>(object value)
   {
      Set(typeof(TKey), value);
   }

   public void Set(object key, object value)
   {
      ArgumentNullException.ThrowIfNull(key);

      if (m_Values.ContainsKey(key))
         throw new ArgumentException($"Key {key} already exists in the container.");

      m_Values[key] = value;
   }

   public T Get<T>()
   {
      return Get<T>(typeof(T));
   }

   public T Get<T>(object key)
   {
      ArgumentNullException.ThrowIfNull(key);

      if (!m_Values.TryGetValue(key, out object? value))
         throw new ArgumentException($"Key {key} does not exist in the container.");

      if (value is not T)
         throw new InvalidCastException($"Value for key {key} is not of type {typeof(T)}.");

      return (T)value;
   }

   public T? GetNullable<T>() where T : class
   {
      return GetNullable<T>(typeof(T));
   }

   public T? GetNullable<T>(object key) where T : class
   {
      ArgumentNullException.ThrowIfNull(key);

      if (!m_Values.TryGetValue(key, out object? value))
         return null;

      if (value is not T)
         throw new InvalidCastException($"Value for key {key} is not of type {typeof(T)}.");

      return value as T;
   }

   public bool TryGet<T>(object key, out T value)
   {
      ArgumentNullException.ThrowIfNull(key);

      if (m_Values.TryGetValue(key, out object? untypedValue))
      {
         if (untypedValue is T typedValue)
         {
            value = typedValue;
            return true;
         }
      }

      value = default!;
      return false;
   }

   public T Instantiate<T>()
   {
      return (T)Instantiate(typeof(T));
   }

   public object Instantiate(Type type)
   {
      if (type.IsAbstract || type.IsInterface)
         throw new ArgumentException($"Type {type} is abstract or an interface and cannot be created.");

      ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

      if (constructors.Length == 0)
         throw new ArgumentException($"Type {type} does not have any public constructors.");

      if (constructors.Length > 1)
         throw new ArgumentException($"Type {type} has more than one public constructor.");

      return Construct(constructors[0]);
   }

   private object Construct(ConstructorInfo ctor)
   {
      ParameterInfo[] parameters = ctor.GetParameters();
      if (parameters.Length == 0)
         return ctor.Invoke(null);

      object[] parameterValues = new object[parameters.Length];
      for (int i = 0; i < parameters.Length; i++)
      {
         ParameterInfo parameter = parameters[i];
         object key = parameter.ParameterType;

         InjectAttribute? injectAttribute = parameter.GetCustomAttribute<InjectAttribute>();
         if (injectAttribute != null)
         {
            if (injectAttribute.Key == null)
            {
               throw new ArgumentException($"Key for parameter {parameter.Name} is null.");
            }

            key = injectAttribute.Key;
         }

         if (!m_Values.TryGetValue(key, out object? value))
         {
            if (Nullable.GetUnderlyingType(parameter.ParameterType) != null)
            {
               continue;
            }

            throw new ArgumentException($"No value found for parameter {parameter.Name} (Key {key}).");
         }

         parameterValues[i] = value;
      }

      return InjectFieldAndProperties(ctor.Invoke(parameterValues));
   }



   private object InjectFieldAndProperties(object instance)
   {
      Type type = instance.GetType();
      FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      foreach (FieldInfo field in fields)
      {
         InjectAttribute? injectAttribute = field.GetCustomAttribute<InjectAttribute>();
         if (injectAttribute == null)
            continue;

         Type? underlyingNullableType = Nullable.GetUnderlyingType(field.FieldType);
         bool isNullable = underlyingNullableType != null;
         Type? fieldType = isNullable ? underlyingNullableType : field.FieldType;
         object key = injectAttribute.Key ?? fieldType!;

         if (!m_Values.TryGetValue(key, out object? value))
         {
            if (isNullable)
               continue;

            throw new ArgumentException($"No value found for field {field.Name} (Key {key}).");
         }

         field.SetValue(instance, value);
      }

      PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      foreach (PropertyInfo property in properties)
      {
         InjectAttribute? injectAttribute = property.GetCustomAttribute<InjectAttribute>();
         if (injectAttribute == null)
            continue;

         Type? underlyingNullableType = Nullable.GetUnderlyingType(property.PropertyType);
         bool isNullable = underlyingNullableType != null;
         Type? propertyType = isNullable ? underlyingNullableType : property.PropertyType;
         object key = injectAttribute.Key ?? propertyType!;

         if (!m_Values.TryGetValue(key, out object? value))
         {
            if (isNullable)
               continue;

            throw new ArgumentException($"No value found for property {property.Name} (Key {key}).");
         }

         property.SetValue(instance, value);
      }

      return instance;
   }
}
