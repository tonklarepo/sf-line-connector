using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dashboard.Services.Helpers
{
    public class EntityHelper
    {
        public static T Map<T, TU>(T target, TU source)
        {
            // get property list of the target object.
            // this is a reflection extension which simply gets properties (CanWrite = true).
            var tprops = target.GetType().GetProperties();

            tprops.Where(o => o.CanWrite).ToList().ForEach(prop =>
            {
                // check whether source object has the the property
                var sp = source.GetType().GetProperty(prop.Name);
                if (sp != null)
                {
                    // if yes, copy the value to the matching property
                    var value = sp.GetValue(source, null);
                    target.GetType().GetProperty(prop.Name).SetValue(target, value, null);
                }
            });

            return target;
        }
        
        public static T MapWithoutJsonIgnore<T, TU>(T target, TU source)
        {
            // get property list of the target object.
            // this is a reflection extension which simply gets properties (CanWrite = true).
            var tprops = target.GetType().GetProperties();

            tprops.Where(o => o.CanWrite).ToList().ForEach(prop =>
            {
                // check whether source object has the the property
                var sp = source.GetType().GetProperty(prop.Name);
                if (sp != null)
                {
                    // if yes, copy the value to the matching property
                    var value = sp.GetValue(source, null);
                    if (sp.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).FirstOrDefault() == null)
                    {
                        target.GetType().GetProperty(prop.Name).SetValue(target, value, null);
                    }
                }
            });

            return target;
        }
    }
}
