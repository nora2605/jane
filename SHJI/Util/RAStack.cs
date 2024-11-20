using System.Collections;

namespace SHJI.Util
{
    public class RAStack<T>(IEnumerable<T> collection) : IEnumerable<T>
    {
        private readonly T[] stack = collection.ToArray();
        public int StackPointer { 
            get => sp;
            set {
                if (sp >= stack.Length)
                    throw new ArgumentOutOfRangeException("StackPointer", "Stack Overflow");
                sp = value;
            }
        }
        private int sp;

        public RAStack() : this(new T[1<<16]) { }

        public T Pop()
        {
            return stack[--StackPointer];
        }

        public T Peek() => stack[StackPointer-1];

        public void Push(T value)
        {
            stack[StackPointer++] = value;
        }

        public T this[int index]
        {
            get { return stack[index]; }
            set { stack[index] = value; }
        }


        public IEnumerator<T> GetEnumerator() => sp == 0 ? Enumerable.Empty<T>().GetEnumerator() : stack[..(sp-1)].Select(x => x).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}