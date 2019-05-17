using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFCore.BulkExtensions
{
    internal class ObjectReaderEx : ObjectReader // Overridden to fix ShadowProperties in FastMember library
    {
        private readonly DbContext context;
        private readonly Dictionary<string, ValueConverter> convertibleProperties;
        private readonly FieldInfo current;
        private readonly string[] members;
        private readonly HashSet<string> shadowProperties;

        public ObjectReaderEx(Type type, IEnumerable source, HashSet<string> shadowProperties,
            Dictionary<string, ValueConverter> convertibleProperties, DbContext context, params string[] members) :
            base(type, source, members)
        {
            this.shadowProperties = shadowProperties;
            this.convertibleProperties = convertibleProperties;
            this.context = context;
            this.members = members;
            current = typeof(ObjectReader).GetField("current", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override object this[string name]
        {
            get
            {
                if (shadowProperties.Contains(name))
                {
                    var current = this.current.GetValue(this);
                    return context.Entry(current).Property(name).CurrentValue;
                }

                if (convertibleProperties.TryGetValue(name, out var converter))
                {
                    var current = this.current.GetValue(this);
                    var currentValue = context.Entry(current).Property(name).CurrentValue;
                    return converter.ConvertToProvider(currentValue);
                }

                return base[name];
            }
        }

        public override object this[int i]
        {
            get
            {
                var name = members[i];
                return this[name];
            }
        }

        public static ObjectReader Create<T>(IEnumerable<T> source, HashSet<string> shadowProperties,
            Dictionary<string, ValueConverter> convertibleProperties, DbContext context, params string[] members)
        {
            var hasShadowProp = shadowProperties.Count > 0;
            var hasConvertibleProperties = convertibleProperties.Keys.Count > 0;
            return hasShadowProp || hasConvertibleProperties
                ? new ObjectReaderEx(typeof(T), source, shadowProperties, convertibleProperties, context, members)
                : Create(source, members);
        }
    }
}