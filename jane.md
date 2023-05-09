# Jane

![Jane Logo](./janelogo.svg)

## Infrastructure

- jane: Language Definition
- john: Jane Object and Heap Notation [See here](./john.md)
- eric: Extensions and Related Integrations Commandline; eric i pooper
- shjc: shitty jane compiler
- shji: shitty jane REPL

## Featur and stuf i thought abuot

- define pragma
- physical dimensions for mathematical operations (and casting) (velocity as M*S^-1 and Voltage as E*Q^-1 etc.)
- Object oriented ig
- PascalCase for classes and static functions
- snake_case, camelCase or lowercase for variables
- UPPERCASE for internal constants
- return can be omitted at the end of a function
- syringe operator
- preprocessor directives | something like: `@(Expand(i, 0, 4))s[i] = d[i];` gets preprocessed to `s[0] = d[0]; s[1] = d[1]; s[2] = d[2]; s[3] = d[3]; s[4] = d[4];` // This is just what a good compiler does ik.....
- Combinators
- Standard Library class for mathematical terms
- Standard Library least squares
- Standard Library equation solver
- REPL
- Gleam Exression Blocks
- extensions Blocks
- Swift error handling
- named arguments

## Data Types

## IO

## Example Programs

### **Hello world something like**

```jane
fn -s Main(args: str[]) {
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
cum.PushRange(["hi" "fooof" "i ate a cookie"]);
```

### **Inline Object declaration**

```jane
db.AddEntry("pooper", {pooper "cum" semen ["semen 1", "semen 2", "semen 3"]})
```

### **Inline variable declaration**

```jane
// let -i: inline, a normal let returns abyss
if ((let -i pooper = long.expensive.function) ~ "penis") {
    Tty.WriteLn(pooper); // this avoids evaluation the function again and declares it inline nicely
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
fn -s AddHandler(type: str, handler: fn(i32) -> abyss) {
    // ...
    handler(34) // Example of how to call such a callback
    // ...
} 
```

### **Stripping parameters to new function**

```jane
gotta need a good way to do this
```

### **When you dereference a non primitive type it will be a reference and not a copy**

```jane
penis = {pooper "shit" semen ["sperm1" "sperm2"]};
let cum = cpy penis;
cum.pooper = "nya";

print(JOHN.serialize(cum) ~ "\n" ~ JOHN.serialize(penis));

/*
{
    pooper "nya"
    semen ["sperm1" "sperm2"]
}
{
    pooper "shit";
    semen ["sperm1" "sperm2"]
}
*/
```

### **Use the ref or cpy keyword (basically ref = pointer) to pass references/clones instead**

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
    // for structs it's value comparison by default
    fn -oO ==(other: Me) -> bool => me.penis == other.penis
}

// As extension (somewhere, very very far away from the original definition):
ext Dong {
    // literally same thing
}
```

### **Object prototype**

```jane
let a: i32[] = Range(0, 10);

// Used in current assembly only
ext i32[] {
    fn Add5() => me.Map(x => x + 5)
}

a.Add5(); // 5,6,7,8,9,10,11,12,13,14

Object prototype (global)

// Used whenever containing class/namespace is imported in file
ext -g i32[] {
    // ...
}

// If you want to use common overrides, make a file called extensions.jn in the assembly and
// put your extension blocks in there top-level
```

### **Interfaces**

```jane
// You can define an interface like this:
interface Penisable {
    fn makePenis() => "8==${me}==D"
}

// Make an existing type conform to an interface by using the extension blocks
ext -g str : Penisable {
    fn -o makePenis() => "8==" ~ me ~ "==D"
}

// Now you can use it like this:
"lool".makePenis()
// >>> 8==lool==D

or make a class/other interface derived from it:

class Penis : Penisable, Stringifiable {
    // No implementation of makePenis() necessary, but Penis needs to be Stringifiable to be used in a format string
    fn -o ToString() => "=".repeat(me.length)
    // An override implementation would be necessary if makePenis was abstract (-A switch)
}
```
