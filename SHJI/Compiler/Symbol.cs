namespace SHJI.Compiler
{
    public class SymbolTable(SymbolTable? parent = null)
    {
        public Dictionary<string, Symbol> Store { get; private set; } = [];
        public SymbolTable? Outer { get; } = parent;

        public Symbol? Define(string name)
        {
            var s = new Symbol(name, Store.Count, Outer == null ? "global" : "local");
            if (Store.ContainsKey(name)) return null;
            Store.Add(name, s);
            return s;
        }

        public Symbol? Resolve(string name)
        {
            if (Store.TryGetValue(name, out var s)) return s;
            else return Outer?.Resolve(name);
        }

        public SymbolTable MakeNested()
        {
            return new SymbolTable(this);
        }
    }

    public readonly struct Symbol(string name, int index, string scope)
    {
        public string Name { get; } = name;
        public int Index { get; } = index;
        public string Scope { get; } = scope;
    }
}