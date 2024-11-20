namespace SHJI.VM
{
    public class Frame(JaneFunction fn, int basePointer)
    {
        public JaneFunction Function = fn;
        public int InstructionPointer = -1;
        public int BasePointer = basePointer;
        public byte[] Instructions { get => Function.Value; }
    }
}
