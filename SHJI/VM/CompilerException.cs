using System.Runtime.Serialization;

namespace SHJI.VM
{
    [Serializable]
    internal class CompilerException : Exception
    {
        public CompilerException()
        {
        }

        public CompilerException(string? message) : base(message)
        {
        }
    }
}