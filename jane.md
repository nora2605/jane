# Jane

![Jane Logo](./janelogo.svg)

## Infrastructure

- jane: Language Definition
- john: Jane Object and Heap Notation [See here](./john.md)
- eric: Extensions and Related Integrations CommandLine; eric i package
- shjc: Schleswig Holstein jane compiler
- shji: Schleswig Holstein jane interpreter

## Features and stuff i thought about

- Arrays are Lists and Slices (internal workings are unimportant for end user)
- physical dimensions for mathematical operations (and casting) (velocity as M*S^-1 and Voltage as E*Q^-1 etc.)
- Object oriented ig
- PascalCase for classes and static functions
- snake_case, camelCase or lowercase for variables
- UPPERCASE for internal constants
- return can be omitted at the end of a function
- syringe operator
- preprocessor directives
- Combinators
- Standard Library class for mathematical terms
- Standard Library least squares
- Standard Library equation solver
- Currying
- REPL
- Gleam Expression Blocks
- extensions Blocks
- Swift error handling
- named arguments
- () operator for calling stuff
- spelled out variants for bitwise operations (xor, nxor)
- try -f if you really wanna fuck around and find out
- tuple indexing

## Data Types

Primitives:

- Signed and unsigned integers from 8 to 128 bits
- Float32 and 64
- Strings
- Characters (UTF-16)

Inbuilt:

- Index, Range, other utils
- Enums
- Structs
- Interfaces
- Classes
- Spaces (Not really a type but whatever, same as package/namespace in other languages)
      - Can include (stateless) Functions, Classes, Structs, Enums, and Compile-Time Constants
- Tuples
- Arrays
- Dynamic Type (Basically a typesafe <string, obj> dictionary)

## IO

- The IO space contains methods directly for doing stuff with Files, Streams, all that
- Standard Library contains Console IO in the Tty Class

## Example Programs

### **Hello world something like**

```jane
fn -s Main(args: str[]) -> i32 {
    Jane.Tty.WriteLn("Hello World!")
    0
}
```

### **Short Hello World**

```jane
Tty.WriteLn("Hello World!");
```

### **Inline Array declaration**

```jane
// Uses JOHN
[1 2 3]
["hello" "hallo" "bonjour"]
```

### **Inline Dictionary/Map declaration**

```jane
{"key1": "value1", "key2": "value2", "key3": "value3"}
// As long as type can be inferred
```

### **Inline Object declaration**

```jane
{ident "value" ident2 ["nested stuff", "is also", "supported"]}
```

### **Inline variable declaration**

```jane
// let -i: inline, a normal let returns abyss
if ((let -i foo = long.expensive.function) + 1 > something) {
    Tty.WriteLn(foo); // this avoids evaluation the function again and declares it inline nicely
}
```

### **Closures/Anonymous functions:**

```jane
file use Jane
file class Program

fn -s Main() -> i32 {
    AddHandler("the cool type", (i: i32) => {
        // ... method body
    })
    // This can be shortened using the following syntax sugar to avoid callback hell:
    AddHandler("the cool type") (i: i32) {
        // ... method body
    }
}

// The function type can be specified like this: a function that takes an i32 and returns abyss
// (may not have a return value)
// notice the capitalization
fn -s AddHandler(type: str, handler: Fn(i32) -> abyss) {
    // ...
    handler(34) // Example of how to call such a callback
    // ...
} 
```

### **Currying**

```jane
// gotta need a good way to do this
```

### **When you dereference a non primitive type it will be a reference and not a copy**

```jane
goo = {foo "bar" goo "gaa"};
let goo2 = copy goo; // Makes a shallow copy
goo2.foo = "nya";

print(JOHN.serialize(cum) ~ "\n" ~ JOHN.serialize(penis));

/*
{
    foo "bar"
    goo "gaa"
}
{
    foo "nya"
    goo "gaa"
}
*/
```

### **Use the ref or copy keyword (basically ref = pointer) to pass references/clones instead**

```jane
fn -s Add(a: ref i32, b: i32) {
    a += b
}

fn -s Main() {
    let a = 5, b = 3
    Add(ref a, b)
    a
    // >>> 8
}
```

### **decorators!!!!**

```jane
// The compiler likes decorators for things where it could mix stuff up
// If you have two static mains in your assembly, use:
@EntryPoint
fn -s Main() {

}


```

### **Operator Overloading**

```jane
class Dong {
    // -o is override, -O is operator.
    // == is standard defined for classes to be equal when they share the same memory location
    // for structs it's (shallow) value comparison by default
    fn -oO ==(other: Me) -> bool => me.ding == other.ding
}

// As extension (somewhere, very very far away from the original definition):
ext Dong {
    // literally same thing
}
```

### **Object prototype**

```jane
let a: i32[] = 0..10;

// Used in current assembly only
ext i32[] {
    fn Add5() => me.Map(x => x + 5)
}

a.Add5(); // 5,6,7,8,9,10,11,12,13,14,15

Object prototype (global)

// Used whenever containing class/space is imported in file
ext -g i32[] {
    // ...
}

// If you want to use common overrides, make a file called extensions.jn in the assembly and
// put your extension blocks in there top-level

// (Good practice advice, not specification)
```

### **Interfaces**

```jane
// You can define an interface like this:
interface Greetable : Stringifiable {
    fn greet() => "Hello, ${me}"
}

// Make an existing type conform to an interface by using the extension blocks
ext -g str : Greetable {
    fn -o greet() => "Hello, " ~ me
}

// Now you can use it like this:
"nora".greet()
// >>> Hello, nora

or make a class/other interface derived from it:

class Person : Greetable {
    // No implementation of greet() necessary, but Person needs to be Stringifiable to be used in a format string
    // if necessary, implement a fn -o ToString() method
    fn -o ToString() => me.Name
    // An override implementation would be necessary if greet was abstract (-A switch)
}
```

### **Computed Properties**

```jane
// A class, struct or interface can have computed Properties
class Person {
    // This is a property, with no getter or setter defined, it will act like a field
    let -p Name { }
    // This property is get-only
    let -p FullName { get }
    // This property is computed
    let -p Age { get => (Now - BirthDate).Years; set => BirthDate = Now - new TimeSpan(Years: value) }
}
```

### **Type coalescion and pattern matching**

```jane
// If you have (for example) an interface and want to get a derived type, you can use the
// Type coalescion operator ::
// This also works for any custom or inbuilt explicit/implicit conversion operators
3::f64 // >>> 3.0

let g: Greetable = new Person(Age: 18)

// iso stands for Is Superset Of and checks for Type Conformity
if g iso Person {
    g::Person.Age
}
```

### **Binary and Boolean Operations**

```jane
// The set is complete!
1 & 1 // Binary and
1 | 1 // Binary or
1 xor 1 // Binary xor
1 nxor 1 // Binary nxor
1 nand 1 // Binary nand
1 nor 1 // Binary nor
!1 // Binary negation
x ==& y // Checks for a binary flag x on y (basically)
// (This is operator chaining and works for any 2 operators)
// (Generally: <x left-op right-op y> is equal to <x right-op x left-op y>)
// Meaning x ==& y -> x == x & y

1 && 1 // Boolean and (if the first one is false, the second one does NOT get evaluated)
1 || 1 // Boolean or (same thing)
!1 // Boolean Negation

// No more Boolean operations unless someone shows me a scenario where they are useful
```

### **Arithmetic and special Operators**

```jane
// We got everything that is to be expected of maths, really
1 + 1
1 - 1
1 * 1
1 / 1
1 ^ 1 // this one's nice, it's a power
1 ~ 1 // This is a concatenation operator
1 ? 1 : 1 // This is a ternary operator
```

### **Inline Dictionaries and Sets and funny operators**

```jane
"abc" in "abcd" // true
"abc" in ["abc"] // true
"a" in ["abc"] // false
["abc", "def"] in ["abc", "def", "ghi"] // true
["def", "abc"] in ["abc", "def"] // false
"abc" iso str // true
[1,2,3] in [1,3,2,4] // false
// => need to represent sets
[[1,2,3]] == [[3,1,2]] // true
// => use for double curlies?
{{1: "hi", 2: "luci"}} // [Map<i32, str>]
```
