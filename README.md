# JaneCore

This is a C# implementation of the Jane Language.

SHJI/C stands for Schleswig-Holstein Jane Interpreter/Compiler, which is a pendant to the name of the Haskell Compiler which is also named after a geographical location.

## Folders

- `Core` contains the Jane Language Frontend
- `SHJC` will contain the Compiler to LLVM
- `SHJI` contains the Interpreter, VM and Standard Library (for now?)

## Jane

<img src="./janelogo.svg" width="256" height="256" alt="Jane Logo" />

See [The Jane feature goals](./jane.md) and the [Website](https://jane.luemir.xyz/)

## John

<img src="./johnlogo.svg" width="256" height="256" alt="Jane Logo" />

See [The John feature goals](./john.md)
And a JS parser at [JohnJS](https://github.com/nora2605/johnjs)

## Details

It will eventually be a compiler to JaneVM and act as a complete language frontend; whereas SHJC will then be a compiler to LLVM or .NET IL (or similar) that gets it down to native executables.

## Credit

HUGE Thanks to the [Writing an Interpreter in Go](https://interpreterbook.com/) Book by Thorsten Ball. Most of the code (as of July 2023) is a slightly altered workthrough of that Book.

I encourage everyone who also wants to build their own language to read this book, it gives enough insight that your own changes to the behavior of the Language seem intuitive.
