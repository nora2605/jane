# Jane

![Jane Logo](./janelogo.svg)

## Infrastructure

- jane: Language Definition
- john: Jane Object Hierarchy Notation [See here](./john.md)
- shjc: Schleswig Holstein jane compiler with LLVM backend
- shji: Schleswig Holstein jane interpreter with JaneVM backend
- shjvm: Internal VM to run jane bytecode
- `jane`: All-in-one command line inspired by bun and go

## Drafts

- Arrays are Lists and Slices (internal workings are unimportant for end user)
- physical dimensions for mathematical operations (and casting) (velocity as M*S^-1 and Voltage as E*Q^-1 etc.)
- Object oriented ig
- PascalCase for classes and static functions
- snake_case, camelCase or lowercase for variables
- UPPERCASE for internal constants
- return can be omitted at the end of a function
- preprocessor directives
- Combinators
- Standard Library class for mathematical terms
- Standard Library least squares
- Standard Library equation solver
- Blocks are Expressions/implicitly called lambdas
- named arguments
- () operator for calling stuff
- spelled out variants for bitwise operations (xor, nxor)
- tuple indexing like in rust ig

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
- Realms (Not really a type but whatever, same as package/namespace in other languages)
      - Can include (stateless) Functions, Classes, Structs, Enums, and Compile-Time Constants
- Tuples
- Arrays
- Dynamic Type (Basically a typesafe <string, obj> dictionary)

## IO

- The IO realm contains methods directly for doing stuff with Files, Streams, all that
- Standard Library contains Console IO in the Tty Class

## Example Programs

**Note**: These Examples are conceptual/hypothetical and do not represent the current state of the language.

### **Hello world but i really like OOP**

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

**Note**: Thinking about aliasing Tty.WriteLn to println for most contexts.

### **Implicit Return**

```jane
// When a function has a return type, the last expressionStatement (if not a return) will be used as return value.

// This is not possible if no return type is given in the function definition, that type can only be inferred when a return is present in the code

fn add(a: i32, b: i32) {
    a + b // Does nothing
}
```

### **Inline Array declaration**

```jane
// Uses JOHN
[1 2 3]
["hello" "hallo" "bonjour"]
```

### **Inline Dictionary/Map declaration**

```jane
{{"key1": "value1", "key2": "value2", "key3": "value3"}}
```

### **Inline Object declaration**

```jane
{ident "value" ident2 ["nested stuff", "is also", "supported"]}
```

### **Sets! (who doesn't love mathy stuff)**

```jane
{[1, 2, 3, 4, 5, 6, 2, 3, 4, 1, 2, 2, 2]}
// It's the same as {[1, 2, 3, 4, 5, 6]} 
```

### **Inline variable declaration**

```jane
if ((let foo = long.expensive.function) + 1 > something) {
    Tty.WriteLn(foo); // this avoids evaluation the function again and declares it inline nicely
}
```

### **Closures/Anonymous functions:**

```jane
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
fn add(a: i32, b: i32) -> i32 {
    a + b
}
/* With a haskell like syntax, currying is performed with the
 * Curry operator $
 * Like in Haskell, without lambda workarounds, this shorthand
 * Only works for the first arguments
 */
let add2: Fn(i32) -> i32 = add$2

add2(3) // >>> 5
```

### **When you dereference a non primitive type it will be a reference and not a copy**

```jane
goo = {foo "bar" goo "gaa"};
let goo2 = copy goo; // Makes a shallow copy
goo2.foo = "nya";

print(JOHN.serialize(goo) ~ "\n" ~ JOHN.serialize(goo2));

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

**Note**: This might be overridable using -oO.

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

// Other decorators will include:
// Debug Symbols
// Cache Line optimizations
// Loop Unwrap
// Compile-Time macros
// ...

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

### **The Constructor**

```jane
// Instead of repeating the type name the constructor
// Is defined using a special function flag -C

fn -C() {
    // Inside the Constructor, the Object might not have a valid state
    // Only stateless functions are permitted to run until all fields that need to have been initialized.
    // These typically include only non-primitives without a default
    // that haven't been initialized in top-level
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
{[1,2,3]} == {[3,1,2]} // true
// => use for double curlies?
{{1: "hi", 2: "luci"}} // [Map<i32, str>]
```

### **Error handling**

```jane
fn -ps readFile(path: str) -> str {
    let stream = open(path, "r")
    // Error: Result<Stream, Error> does not have the method readToEnd()
    stream.readToEnd()
}

fn -ps readFile(path: str) -> str {
    // Error: Bubbling up not allowed in non-throwing function
    let stream = try open(path, "r")
    stream.readToEnd()
}

// Do this instead (-t = --throws)
fn -ps -t readFile(path: str) -> str {
    let stream = try open(path, "r")
    stream.readToEnd()
}

// If you're sure, you can force an unwrap of a throwing function
fn -ps readFile(path: str) -> str {
    let stream = open(path, "r")! // danger zone!
    stream.readToEnd()
}

// If you're unsure, you can catch with different flags
fn -pst readFile(path: str) -> str {
    // print error to stderr and bubble up
    let stream = try -p open(path, "r")
    
    // discard error and treat as abyssable
    let stream = try -d open(path, "r")
}

// Try/Catch Blocks
fn -ps readFile(path: str) -> str {
    // when error handling, you might want to be inside a block where multiple things can fail:
    try {
        let stream = open(path, "r")
        let result = UTF8.fromBytes(stream)
        return result
    } catch (FileNotFound) {
        Tty.WriteLn("didn't find file :(")
    } catch (InvalidUTF8) {
        Tty.WriteLn("that seems to be some binary ahh")
    } finally {
        return "";
    }
}

// You can use the same modifiers here, and also just panic on catch:
fn -ps readFile(path: str) -> str {
    // -f forces an unwrap when there is no return value (like in a block)
    try -f {
        ...
    }
}
```

### **Bake IO**

```jane
// If you want to embed a file at compile time, whether it be a config, audio, image or script file, you can use the IO.embed function
// By using the constant switch on the let, you enable the variable to be defined at compile time
// In interpreted mode, the file will be read JIT

let -c sfx: Buffer = embed("./sfx1.wav")

// The difference between embed and read is that embed is marked as stateless to the compiler while read is not, because at runtime the filesystem is obviously mutable
```

### **Unwrap Abyssables**

```jane
let foo: i32? = 20
if something() {
    foo = abyss
}

// if you're sure something() didn't happen and the compiler isn't
// you can force to unwrap the optional and panic if it's abyss
// use a postfix ! for that

let bar: i32 = foo!
```
