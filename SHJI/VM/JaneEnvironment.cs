using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.VM
{
    internal class JaneEnvironment
    {
        public Dictionary<string, IJaneObject> Store { get; set; }

        public Dictionary<string, Dictionary<(ObjectType, ObjectType), Func<IJaneObject, IJaneObject, IJaneObject>>> Operators;

        public JaneEnvironment? Outer { get; }
        public JaneEnvironment()
        {
            Store = new();
            Operators = new();
        }
        public JaneEnvironment(JaneEnvironment outer) : this()
        {
            Outer = outer;
        }

        public IJaneObject Get(string key)
        {
            if (Store.TryGetValue(key, out IJaneObject? obj)) return obj;
            else if (Outer is not null) return Outer.Get(key);
            return IJaneObject.JANE_UNINITIALIZED;
        }

        public IJaneObject Set(string key, IJaneObject value)
        {
            if (Outer is not null && Outer.Has(key) && !Has(key, false))
            {
                Outer.Set(key, value);
                return value;
            }
            Store[key] = value;
            return value;
        }

        public bool Has(string key, bool CheckOuter = true)
        {
            return Store.ContainsKey(key) || CheckOuter && Outer is not null && Outer.Has(key);
        }

        public IJaneObject this[string key] { get => Get(key); set => Set(key, value); }
    }
}
