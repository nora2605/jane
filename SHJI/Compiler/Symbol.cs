namespace SHJI.Compiler
{
    public class SymbolTable
    {
        public Dictionary<string, Symbol> Store { get; private set; } = [];

        public Symbol? Define(string name)
        {
            var s = new Symbol(name, "", Store.Count);
            if (Store.ContainsKey(name)) return null;
            Store.Add(name, s);
            return s;
        }

        public Symbol? Resolve(string name)
        {
            if (Store.TryGetValue(name, out var s)) return s;
            else return null;
        }
    }

    public readonly struct Symbol(string name, string scope, int index)
    {
        public string Name { get; } = name;
        public int Index { get; } = index;
        public string Scope { get; } = scope;
    }
}