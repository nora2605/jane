# This is amazing john notatienon

This is the Jane Object and Heap Notation Standard

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
+ heaps (just there because of the name tbh, see below)
+ typing (see below)

Minimized JOHN does not require any newlines.

## Typing

JOHN supports the following types and primitives:

| Type name | Jane equivalent   | JOHN declaration      | javacript equivalent  |
| -         | -                 | -                     | -                     |
| string    | :str              | "string"              | "string"              |
| char      | :chr              | 'c'                   | "c"                   |
| byte      | :u8               | 0b11100101 or 219b    | 219                   |
| sbyte     | :i8               | 0b10001011s or -117bs | -117                  |
| short     | :i16              | 12444i16              | 12444                 |
| ushort    | :u16              | 52444u16              | 52444                 |
| int       | :i32              | 1234567               | 1234567               |
| uint      | :u32              | 31111111u32 or 0xffff | 3111111111            |
| long      | :i64              | 123456789098765i64    | 123456789098765       |
| ulong     | :u64              |18446744073709551615u64| 18446744073709551615  |
| float     | :f32              | 15.0 or 15f32         | 15                    |
| double    | :f64              | 15.4f64               | 15.4                  |
| version   | :Jane.Tuples.vrs  | v1.2.3.4              | `{"major": 1, "minor": 2, "patch": 3, "build": 4}` |
| index     | :Jane.idx         | *1 or ^1              | 1 or -1               |
| range     | :Jane.rng         | 1..3                  | \[1,2,3\]             |
| tuple     | :(T1, T2)         | ("123" 123)           | \["123", 123\]        |
| link list | :Jane.LList       | "123" -> "234"        | `{"value": 123, "next": &{"value": "234". "next": null}}` |
| circ llist| :Jane.CircularLList| -> "123" -> "234"    | `{"value": 123, "next": &{"value": "234", "next": &this}}` |
