using Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DLCQuestipelago.Extensions
{
    public static class ReflectionExtensions
    {
        public static TFieldType GetPrivateFieldValue<TClassType, TFieldType>(this TClassType instance, string fieldName)
        {
            var field = typeof(TClassType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = (TFieldType)field.GetValue(instance);
            return fieldValue;
        }
    }
}
