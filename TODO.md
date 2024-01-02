# TODO

The following constitutes a non-exhaustive list of features that still have to be done (mostly for my own convenience):

## Standard Library

* Gaussian Integers
* Rational Numbers (Lossless rational computation my beloved)
* Complex Numbers
* Math AST
* Int to arbitrary base

## General

This Project will be split into 3 parts:
Language Frontend and Core Library (JaneCore) (Lexer, Parser, AST, VM)
Interpreter/REPL (SHJI)
Compiler (SHJC)

* Operators:
	* Switcheroo operator: switches a bool from true to false or false to true (postfix !!)
	* Modulo
	* Binary arithmetic
	* Boolean arithmetic
	* Pufferfish
	* Type coercion
	* Range Operator
* Type Parsing:
	* Number suffixes
* WHAT THE FUCK IS A FUNCTION TYPE EXPRESSION
* Generics
* currying operator?
* generator (yield)
* type aliasing
* https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns


## Parser

* Parse Lambda expressions
* Parse Accessor Expressions
* Parse Object Literals and Tuples (Regarding Tuples: make "," the tuple operator in general contexts, in argument lists require ())
* Parse Control flow (for, loop, while)
* Parse Preprocessor Directives
* Parse #/Maths functions
* Make distinction between ParseIdentifier, ParseIdentifierWithOptionalTypeSignature and ParseType (for generics and other type expressions (abyssables, tuple types and array types, inline object types, ...) that are more complicated than just "str" or something)


## Interpreter

* A proper type-system (I have to think about a way I can evaluate operators without hardcoding every number type)
	* Also Rewrite the ObjectType enum to be a class to support generics, inbuilts, all that
		* That also means there will be no "Jane..." structs anymore, just JaneObject with a specific type that is linked to the standard library
		* Maybe the primitives have to stay, i don't know
* External Functions (file use directives)
* Object Orientation (basically)
* Type enforcement for Arrays
* Type inference for objects
* Custom Operator Overloading
* Extension Blocks
* (a vision that was granted to me by Alan Turing in a dream) INFIX OPERATOR TYPE INFER ALGORITHM:
	* Pseudocode
```
	InferTypes(BinaryOperator, Type1, Type2?, depth=0) {
		OperatorDict = OperatorImplementations[BinaryOperator]
		if (TryGet OperatorDict[(Type1, Type2?)])
			return (OperatorDict[(Type1, Type2?)].Function, (OperatorDict[(Type1, Type2,...)].OutType, Type1, Type2, ...))
		foreach Type get ImplicitTypeConversions
			foreach CombinationOf ImplicitTypeConversions
				InferTypes(depth = depth + 1) foreach Combination
		return Combination.Sort(by depth).Fst()
	}
```