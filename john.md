# JOHN - Jane Object Hierarchy Notation

<img src="./johnlogo.svg" width="256" height="256" alt="John Logo" />

The capitalization "JOHN" is preferred in this document but "John" is also acceptable.

This is the Jane Object Hierarchy Notation Standard Document Version 1.

## Syntax

The JOHN Syntax is fairly simple and based on common brace conventions.
A JOHN file may contain either a top-level object or a value of any supported type.

In the following document, angled brackets (`<` and `>`) will be used to denote placeholders. The following terms will be used to denote common symbols:

| Symbol(s) | Name                      |
|:------    | ----:                     |
| `[`       | Left Square Bracket       |
| `]`       | Right Square Bracket      |
| `(`       | Left Parenthesis          |
| `)`       | Right Parenthesis         |
| `{`       | Left Curly Brace          |
| `}`       | Right Curly Brace         |
| `<`       | Left Angled Bracket       |
| `>`       | Right Angled Bracket      |
| `:`       | Colon                     |
| `.`       | Period                    |
| `-`       | Hyphen                    |
| `'`       | Single Quote(-ation mark) |
| `"`       | Double Quote(-ation mark) |
| `@`       | At-Sign                   |
| `#`       | Hashtag                   |
| `\`       | Backslash                 |

Other Symbols are referred to by their Unicode names.

Spaces and newlines in the following code examples are generally replacable with any [Token Break](#token-break).

### Comment

A comment in JOHN can be made using a double-slash (`//`) in a line of text. The rest of this line will be interpreted as a comment and ignored by a parser.

Example:

```john
// This is a single-line comment
<JOHN code> // This is also a single-line comment
```

C-Style multi-line comments are not supported.

#### Implementation note -  Comment

Since comments are neither serialized nor deserialized from/into JOHN, they only stay and appear in custom config files. Do not overwrite these files with serialized data if the comments serve a purpose.

### Object

Objects in JOHN are declared as follows:

```john
{
    <key> <value> <key> <value> <key> <value> <...>
}
```

`<key>` is a string of alphanumerical characters and the underscore. It matches the following Regular Expression: `[_a-zA-Z]+\w*`.
Evident from that, it may not start with a digit.

Certain keywords may not be a key:
`true`, `false` and `abyss`.
If you do want to use them, you can prefix them with an underscore (this is subject of implementation details).

`<value>` is a [JOHN value](#john-value).

An object that is not top-level may be empty.

#### Top Level Object

If at top-level, an object does not need to include the outer curly braces. It may be defined as so:

```john
<key> <value>
<key> <value>
```

#### Implementation note - Object

The object type is encouraged to be deserialized into a language-native object. In type-safe languages where excessive "juggling" may be necessary, a dictionary or map is recommended instead.

### Array

Arrays in JOHN are declared as follows:

```john
[
    <value> <value> <value> <...>
]
```

Where `<value>` is any [JOHN value](#john-value).

#### Implementation note - Array

Arrays should be kept (deeply) type-consistent. JOHN does not and can not check for it itself, the recommended behaviour for inconsistent arrays is a warning during parse.

Tuples should be used for type-various collections instead.

### Tuple

Tuples in JOHN are declared as follows:

```john
(
    <value> <value> <value> <...>
)
```

`<value>` is a [JOHN value](#john-value)

The tuple may be empty, but the empty Tuple is better represented by the [Abyss](#abyss) type.

### Set

Sets in JOHN are declared like this:

```john
{[
    <value> <value> <value> <...>
]}
```

`<value>` is a [JOHN value](#john-value)

The set may be empty

#### Implementation note - Set

A set contains only unique values in no particular order. A JOHN implementation is encouraged to use the languages standard library for a set implementation. This, like the Array, is a type-consistent collection. A type mismatch should cause a Warning in the parser.

### Dictionary

A dictionary in JOHN is declared like this:

```john
{{
    <key> <value> <key> <value> <...>
}}
```

`<key>` and `<value>` are both JOHN values, but `<key>` may not be of any composite type. Allowed types are

* [Integer](#integer)
* [Floating Point](#floating-point-number)
* [Character](#character)
* [String](#string)
* [Boolean](#boolean)
* [Abyss](#abyss) - A default value

#### Implementation note - Dictionary

The dictionary is encouraged to be type-consistency enforced. If type variety is desired, one should use an [Array](#array) of [Tuples](#tuple). Abyss, as a null value, may be combined with any key-type to form a default value on lookup failure, if so supported by the host language.

### JOHN Value

Every syntactical construct except for comments and annotations may be used as a JOHN value. This includes Scalar/Value Types and Objects, Arrays, Tuples.

### Integer

In JOHN, the Integer type is a string of characters matching one of the following Regular Expressions: `-?[0-9_]+(e[0-9]+)?`, `-?0x[0-9a-fA-F]+`, `-?0b[01]+`, `-?0o[01234567]+`.

These represent decimal, hexadecimal, octal and binary numbers.

#### Implementation note - Integer

The type of integer in the host language that these values should be parsed at (if no type hint or scaffolding is supplied) should be the native integer (64 bit on most systems), the Big Integer if it doesn't fit, or the number type if the language does not provide an integral type.

### Floating Point Number

In JOHN a floating point number is represented by a string of characters matching one of the regular expressions `-?(\d+f|\d+e-\d+|\d*\.\d+(e-?[0-9]+)?f?)`, `0x([0-9a-fA-F]{8}){1,2}[rR]`.

Note that this does not include region specific notation: a comma is NOT accepted as a decimal point.

The hexadecimal notation is supposed to be interpreted as a binary representation for the IEEE 754 32- or 64-bit floating point format. It is suffixed with an `r` or `R`, for "real" or "rational", whatever makes more sense in your head. The bytes are in the usual order (big-endian in js dataview).

Examples:

```john
1.0
2.55
3f
-7f
-.3f
.441e-2

0x2a1949dfR
0x407A4B0A3D70A3D7r
```

#### Implementation note - Floating Point numbers

The JOHN Floating point numbers should be parsed into 64-bit (double precision) floats by default, if no type-hint/scaffolding is provided. The hexadecimal notation is always parsed into the corresponding number of bits.

### Index

An index is a type that represents the index of a finite list or array and can be from the front or from the back (top or bottom, start or end). It is declared like this:

```john
*1 // This is the second Element from the start of the array
^0 // This is the last Element of the array
```

It does not support negative integrals.

#### Implementation note - Index

This feature in particular is more or less specific for a few host languages and may be parsed as a positive or negative integer by others.

### Range

A range is a type that represents a start, end and step value for iteration or an interval on the integers or floating point numbers. In JOHN it's declared like this:

```john
1..2 // inclusive integer range from 1 to 2
0..10 // inclusive integer range from 0 to 10
0..9..0.5 // inclusive floating point range from 0 to 9 with step size .5
0..^10..1 // end-exclusive integer range from 0 to 10 (0 to 9)
^0..^8..0.01 // start and end exclusive floating point range from 0 to 8 with step 0.01 (0.01 to 7.99)
```

A range matches the Regular Expression `(\^?-?[0-9]+(\.[0-9]+)?)\.\.(\^?-?[0-9]+(\.[0-9]+)?)(\.\.(-?[0-9]+(\.[0-9]+)?))?`. Note therefore that this type does NOT support all notation formats for floating point numbers and integers in JOHN.

#### Implementation note - Range

This feature is very specific to Jane. Other languages may parse this as one of the following:

* A custom type `JOHNRange`
* A tuple
* A precomputed array (recommended)

### Version

The Version type in JOHN represents Semantic Versioning. It is declared like this:

```john
v1.2
v1.0.3
v12.0
v16
v0.0.0.1
v0.1.0-rc1
v0.2.5-alpha
```

Specifically, it matches the following Regular Expression: `v[0-9]+(\.[0-9]+){0,3}(-\w+)?`.

#### Implementation note - Version

If your host language does not have a version type it may be parsed the following ways:

* Custom type `JOHNVersion`
* Tuple `(int, int?, int?, int?, string?)`
* Object `{major: int, minor?: int, patch?: int, build?: int, suffix?: string}` (recommended)

### String

A string in JOHN is a sequence of characters enclosed by double quotation marks `"`. Double quotation marks as well as special characters are escapable using the backslash character `\`, as well as the backslash itself. The string may not contain an unescpaped control character, including a newline. The string should(!) be UTF-8 encoded and parsed as such if possible.

### Character

A character in JOHN is a single UTF-8 character delimited by two single quotation marks `'`. It supports the same escape sequences as [the JOHN String](#string).

### Boolean

A boolean in JOHN can have one of two values:

* The character sequence `true`
* The character sequence `false`

These are not available as `<key>`s in [Objects](#object).

### Abyss

The `abyss` value, also known as `null`, `nil`, `void` or `()` is the absense of a value. In JOHN it is denoted using the sequence of characters `abyss` or the token `#`.

### Datetime

A datetime in JOHN is an ISO 8601 Date, Time or combined Datetime string. Examples:

```john
18:00:35
1970-01-01
2019-11-15T13:34:22
2024-07-11T01:42:53
2007-08-31T16:47+00:00
2007-12-24T18:21Z
2009-01-01T12:00:00+01:00
2009-06-30T18:30:00+02:00
2010-01-01T12:00:00.001+02:00
```

(Some of these examples were taken from the german Wikipedia page for ISO 8061.)

#### Implementation note - Datetime

Usually languages carry a Datetime library around with them. If they do not, parsing these might be a very difficult task, use a string instead and let the end user invoke a library on it to stay dependency-free.

The JavaScript reference implementation uses `Date.parse` which returns millis after epoch.

Additionally, a timestring carries token breaks. Be careful to handle these.

### Time Interval

A time interval in JOHN conforms to the ISO 8061 time interval definition. It may not be combined with a datetime string. Examples:

```john
P3Y6M4DT12H30M17S
P1D
PT24H
P14W
```

#### Implementation note - Time interval

See essentially [Implementation note - Datetime](#implementation-note---datetime)

The JavaScript reference implementation uses an object containing keys corresponding to every time unit as to be adaptable to different length months and leap years.

### Information Unit

Information units in JOHN offer a way to represent common memory, storage sizes and SI suffixes. Examples:

```john
1MB     // = 1000000 bytes
2MiB    // = 2097152 bytes
23TiB   // = 25288767438848 bytes
1b      // = 1 bit
4Mb     // = 4000000 bits
14Gib   // = 15032385536 bits
```

Due to limitations of integer sizes, the expressions should only parse up until 1EiB (1 Exabibyte or Ebibyte). Higher values give an error.

Supported Suffixes:

* `KB`, `Kb`, `KiB`, `Kib` as 1000 bytes, 1000 bits, 1024 bytes and 1024 bits respectively
* `MB`, `Mb`, `MiB`, `Mib`
* `GB`, `Gb`, `GiB`, `Gib`
* `TB`, `Tb`, `TiB`, `Tib`
* `PB`, `Pb`, `PiB`, `Pib`
* `EB`, `Eb`, `EiB`, `Eib`

#### Implementation note - Information Unit

This value type is to be parsed as a number of bits in a 64-bit integer. If type hinted as a string it may be parsed as the expression itself.

### Nodes

Nodes are Objects of certain type, with Attributes and children. This is the common datatype in XML and HTML. In JOHN, Nodes are structured like this:

```john
| <type> <attribute_key> <attribute_value> <...> | [
    <children...>
]
// or
| <type> <attribute_key> <attribute_value <...> | <content>
```

For readable Markup, it is recommended to use the [Token break characters](#token-break). Here is a comparison of HTML to (opinionated) JOHN:

```html
<!DOCTYPE html>
<html>
    <head>
        <title>Hello World!</title>
    </head>
    <body>
        <h1 id="heading1">Heading 1</h1>
    </body>
</html>
```

```john
doctype "html"
dom |html| [
    |head| [
        |title| "Hello World!"
    ]
    |body| [
        |h1 id="heading1"| "Heading 1"
    ]
]
```

#### Implementation note - Nodes

Nodes should parse as objects with 3 properties: type, attributes and children. In Javascript they parse as `JSX.Element`s.

### Annotations

Annotations in JOHN are non-data directives, that can tell a parser how to interpret data. The following annotations are given by the spec:

* `@as_dict` parses an object as a dictionary or map.
* `@schema()` - see [Schemas](#schemas)
* `@mini` (top-level only) signifies that a file can be parsed with reduced feature set (must be the only annotation)

Other annotations may be freely declared by implementations to handle language specificities.

#### Implementation note

When serializing to JOHN, global annotations should be specified in the Serializer Options. There is no spec for handling Pointer or element-specific annotations, since those features are mainly for the data seeding use case.

### Token Break

Token breaks are characters that cause a token in JOHN to end.
The following characters are token breaks:

* The space
* The newline (CRLF or LF)
* The tab
* The semicolon
* The colon
* The comma
* The equals sign

The following tokens do not require any token breaks around them:

* The left parenthesis
* The right parenthesis
* The left square bracket
* The right square bracket
* The left curly brace
* The right curly brace
* The hashtag
* The middle bar

## Schemas

Schemas in JOHN may be defined as JOHN and included via the `@schema(<filename>)` annotation. In Schemas, string values contain a primitive JOHN Value Type. These are the type strings:

```john
a_string            "string"
a_character         "char"
an_integer          "integer"
a_floating_point    "float"
a_boolean           "bool"
an_index            "index"
a_range             "range"
a_version           "version"
abyssable_string    "string?" // or anything else
a_datetime          "datetime"
a_time_interval     "interval"
an_info_unit        "size"
an_unspecified_val  "any"
```

And objects, arrays, tuples, sets, dictionaries and nodes are type-defined like so:

```john
an_object {
    <key> <type>
    <...>
}
an_array [
    <type> // just one
]
a_tuple (
    <type> <type> <type> <...>
)
a_set {[
    <type> // just one
]}
a_dictionary {{
    <key_type> <value_type> // just one
}}
a_node | <key> <type> <...> | [
    <type> // just one (like array)
]
```

A schema does not allow values to be left out. If they're not required they should be marked as abyssable (see above). For generic use of the object, array, tuple, set, dictionary and node types one may specify `"object"`, `"array"`, `"tuple"`, `"set"`, `"dictionary"` or `"node"` as types.

If multiple types are allowed, a union type can be constructed using `|` inside the type string.

Recursive types may be specified using the Pointer syntax:

```john
@name(node)
value "integer"
next @point(node)
```

## JOHNmini

JOHN defines a subset of itself as JOHNmini, that doesn't support Schemas, Annotations, non-primitive types (indices, versions, timestamps and -intervals), nodes, sets, dictionaries and comments.

JOHNmini is therefore an analogue to JSON, with a more minimal syntax that is more lenient.

## Use cases

1. It serves as a more minimal alternative to JSON, even in its minimal feature set
2. It serves as a data seeder; as to not construct and hardcode complex objects in the programming language manually.
3. It serves as a general purpose notation standard for different datatypes, without necessary link to a programming language.

## Reference Implementation

The Reference Implementation is written in JavaScript/TypeScript due to its friendly nature with dynamic objects.

Its repository is located [here](https://github.com/nora2605/JOHNJS)

Since this is, of course, part of the Jane Project, its standard library will have a more complete implementation later.

## Examples

### A simple person object

```john
first_name "Max"
last_name "Mustermann"
age 28
```
