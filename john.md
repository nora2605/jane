# This is amazing john notatienon

![John Logo](./johnlogo.svg)

This is the Jane Object Hierarchy Notation Standard

## First glance

This is an example for an object that stores a quiz

```john
// Comments are enabled by default (woah!)
// The file can be imported as an object:
// let quiz: obj = JOHN.parse('quiz.john');

quiz {
    sport {
        questions [
            {
                text "Which one is a correct team name in NBA";
                options ["New York Bulls"; "Los Angeles Kings"; "Golden State Warriors"; "Huston Rocket"];
                answer 3
            }
        ]
    }
    maths {
        questions [
            {question "5 + 7 = ?"; options ["10", "11", "12", "13"; answer 2]}
            {question "12 - 8 = ?"; options ["1", "2", "3", "4"; answer 3]}
        ]
        description "maths is cool"
        version v1.0.0.0
    }
}
```

So as you see, John supports the following cool epic awesome features:

+ line comments
+ objects and nesting
+ arrays
+ tuples
+ hashes (array of tuples)
+ basic typing (see below)

Minimized JOHN does not require any newlines, commas or semicolons.

The Seperator can be whatever you want (semicolon, comma, space, all of it acts as a token break)

## Typing

JOHN supports the following types and primitives:

| Type name | Jane equivalent   | JOHN declaration      | javascript equivalent  |
| -         | -                 | -                     | -                     |
| string    | :str              | "string"              | "string"              |
| char      | :chr              | 'c'                   | "c"                   |
| bool  | :bool    | true     | true     |
| byte      | :u8               | 0b11100101 or 219u8   | 219                   |
| sbyte     | :i8               | -117i8                | -117                  |
| short     | :i16              | 12444i16              | 12444                 |
| ushort    | :u16              | 52444u16              | 52444                 |
| int       | :i32              | 1234567               | 1234567               |
| uint      | :u32              | 31111111u32 or 0xffff | 3111111111            |
| long      | :i64              | 123456789098765i64    | 123456789098765       |
| ulong     | :u64              |18446744073709551615u64| 18446744073709551615  |
| float     | :f32              | 15.0 or 15f32         | 15                    |
| double    | :f64              | 15.4f64               | 15.4                  |
| version   | :Jane.vrs  | v1.2.3.4              | `{"major": 1, "minor": 2, "patch": 3, "build": 4}` |
| index     | :Jane.idx         | \*1 or ^1              | 1 or -1               |
| range     | :Jane.rng         | 1..3                  | \[1,2\]               |
| tuple     | :(T1, T2)         | ("123" 123)           | \["123", 123\]        |
| sets      | :Set\<T\>           | {[1 2 3 4 2]}         | \[1 2 3 4\] |

## Future

JOHN wants to be concise and short for all sorts of purposes. Feature Goals:

+ Timestamp
+ Time range
+ extended semantic versioning
+ memory size
+ nodes (xml, html)

## Implementations

None of the Reference Implementations are yet complete.
They do not feature robust parsing and no type safety and no comments.
They are about as usable as JSON.

Reference JOHN parsers:

+ for C#: [Nuget Package](https://www.nuget.org/packages/JOHNCS), [Repository](https://github.com/nora2605/JOHNCS)
+ for JS (parses to native structures): [NPM Package](https://www.npmjs.com/package/johnjs), [Repository](https://github.com/nora2605/johnjs)
+ for Haskell: [Repository](https://github.com/nora2605/john.hs)
+ for Rust: [Crate](https://crates.io/crates/johnrs), [Repository](https://github.com/nora2605/JOHNrs)
