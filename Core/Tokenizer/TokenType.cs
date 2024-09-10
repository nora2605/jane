namespace Jane.Core
{
    public enum TokenType
    {
        // General
        ILLEGAL,                    // anything that isn't the stuff below :/
        EOF,                        // \0 or EOF
        EOL,                        // \n (CRLF supported)
        IDENTIFIER,                 // /^[_a-zA-Z][_a-zA-Z0-9]*$/
        LPAREN,                     // (
        RPAREN,                     // )
        LSQB,                       // [
        RSQB,                       // ]
        LCRB,                       // {
        RCRB,                       // }
        SEMICOLON,                  // ;
        ASSIGN,                     // =
        DISCARD,                    // _
        // Numeric
        NUMERIC,                    // e.g. 1234, 0x00, 0b00, 0o00, 0.123, .123, 2f
        // Constants
        ABYSS,                      // abyss
        TRUE,                       // true
        FALSE,                      // false
        // Literals
        DOUBLE_QUOTE,               // '
        SINGLE_QUOTE,               // "
        STRING_CONTENT,             // sequence of UTF-8 characters, C style escapes
        CHAR_CONTENT,               // any UTF-8 character, C style escapes
        STRING_INTERPOLATION_START, // ${
        FLAGS,                      // /-[a-zA-Z]+/, the modifiers for certain keywords; spelt out variants are lexed as identifiers
        // Keywords
        // - Declarations
        CLASS,                      // class {}
        STRUCT,                     // struct {}
        TRAIT,                      // trait {}
        EXT,                        // ext <type> {}
        TYPE,                       // type <type> = <type algebra>
        FILE,                       // file use|class|realm
        USE,                        // see ^
        REALM,                      // realm <name> {}
        FN,                         // fn <flags> <name>(<args>) <[-> <return type>]?> {}
        FN_TYPE,                    // ->
        LET,                        // let <flags> <pattern> <[{ get: <getter>, <[set: <setter>]?> }]?> = <expr>
        NEW,                        // new <type?>(<args>)
        ENUM,                       // enum <flags?> <name> {}
        // - Control Flow
        RETURN,                     // ret <expr?>
        MATCH,                      // match <expr> { [<pattern> => <statement>]* | _ => statement }
        IF,                         // if <expr> {} [else <statement>]? | if (<expr>) <statement> ...
        ELSE,                       // see ^
        WHILE,                      // while [-a]? <expr> {}
        LOOP,                       // loop <expr?> {}
        FOR,                        // for <name> in <expr> | for <name (value)>, <name (index)> in <expr>
        BREAK,                      // break
        CONTINUE,                   // continue
        // Operators
        // - Mathematical
        PLUS,                       // +
        MINUS,                      // -
        MUL,                        // * ----- also Index from front (as prefix)
        DIV,                        // /
        REMAINDER,                  // %
        POWER,                      // ^ ----- also Index from behind (as prefix)
        INCREMENT,                  // ++
        DECREMENT,                  // --
        EQUAL,                      // ==
        NOT_EQUAL,                  // !=
        GT,                         // >
        LT,                         // <
        GTE,                        // >=
        LTE,                        // <=
        LEFT_SHIFT,                 // <<
        RIGHT_SHIFT,                // >>
        COMMA,                      // , ----- for tuples, argument lists
        L_AND,                      // &&
        L_OR,                       // ||
        L_XOR,                      // xor
        B_AND,                      // & ----- also intersection
        B_OR,                       // | ----- also union
        B_XOR,                      // ^^
        B_NEG,                      // ~~
        IN,                         // in
        // - General/Typed
        CONCAT,                     // ~
        RANGE,                      // ..
        CURRY,                      // $
        LAMBDA,                     // =>
        INTERRO,                    // ? ----- optional type annotation, abyss check, ternary 'if then'
        ACCESSOR,                   // .
        ABYSS_ACCESSOR,             // ?.
        ABYSS_COALESC,              // ??
        EXCLAM,                     // ! ----- logical negation and unwrap of optional values/results
        COERCE,                     // ::
        COLON,                      // : ----- Ternary 'else' or type annotation
    }
}
