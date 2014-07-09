namespace Promorphosis.Common.Helpers
{
   /// <summary>
   /// Generics based dynamic object class to provide patching functionality.
   /// 
   /// Ref: http://blogs.msdn.com/b/csharpfaq/archive/2009/10/19/dynamic-in-c-4-0-creating-wrappers-with-dynamicobject.aspx
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class DeltaPatcher<T> : DynamicObject
   {
      // Has the cached list of properties that can be patched.
      private static Dictionary<string, PropertyInfo> m_patchableProperties = null;

      // Save property values set on this instnace. We will use these to apply
      // patch to target base item.
      Dictionary<string, object> m_setValues = new Dictionary<string, object>();

      /// <summary>
      /// Constructor
      /// </summary>
      public DeltaPatcher()
      {
         // cache patchable properties for later use, we don't have to
         // look for them in every instance of T
         if (m_patchableProperties == null)
         {
            m_patchableProperties = new Dictionary<string, PropertyInfo>();

            // get public settable properties of T
            var props = typeof(T).GetProperties(); 
            foreach (var prop in props)
            {
                if (prop.CanWrite)
                {
                    // Skip not patchable properties
                    var attr = prop.GetCustomAttribute(typeof(NotPatchableAttribute));
                    if (attr == null)
                    {
                       m_patchableProperties.Add(prop.Name, prop);
                    }
                }
            }
         }         
      }

      /// <summary>
      /// Called when a dynamic property is being set for this instance.
      /// 
      /// We intercept this and remember that set was called. This information
      /// is later used to patch a target base item.
      /// </summary>
      /// <param name="binder"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
         // Converting the property name to lowercase
         // so that property names become case-insensitive.
         string name = binder.Name;
         if (binder.IgnoreCase)
         {
            name = name.ToLower();
         }

         if (value != null)
         {
            // ensure appropriately typed value is saved by us
            if (m_patchableProperties.ContainsKey(name))
            {
               PropertyInfo prop = m_patchableProperties[name];

               TypeConverter typeConverter = TypeDescriptor.GetConverter(prop.PropertyType);
               value = typeConverter.ConvertFromString(value.ToString());
            }
         }
      
         // Remember that set property was called
         m_setValues.Add(name, value);

         return true;
      }
    
      /// <summary>
      /// Called when a dynamic property is being read from this object.
      /// </summary>
      /// <param name="binder"></param>
      /// <param name="result"></param>
      /// <returns></returns>
      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
         // Converting the property name to lowercase
         // so that property names become case-insensitive.
         string name = binder.Name;
         if (binder.IgnoreCase)
         {
            name = name.ToLower();
         }

         // If the property name is found in a dictionary,
         // set the result parameter to the property value and return true.
         // Otherwise, return false.
         return m_setValues.TryGetValue(name, out result);
      }

      /// <summary>
      /// Patches the target base item with the property values that were set on this dynamic object.
      /// </summary>
      /// <param name="baseItem"></param>
      public virtual void Patch(T baseItem)
      {
         // apply values that were set, to base item
         foreach (string key in m_setValues.Keys)
         {
            object result = m_setValues[key];

            var prop = m_patchableProperties[key];
            prop.SetValue(baseItem, result);
         }
      }     
   }

   /// <summary>
   /// Property attribute to mark a particular property as non-patchable.
   /// </summary>
   public class NotPatchableAttribute : Attribute 
   { 
   }
}
