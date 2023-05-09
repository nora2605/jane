/*
Language: Jane
Author: Nora2605 <nora.ja2605@gmail.com>
Contributor:
Website: https://janelang.ml/
Category: common
*/

/** @type LanguageFn */
export default function Jane(hljs) {
  const BUILT_IN_KEYWORDS = [
    "bool",
    "u8",
    "chr",
    "double",
    "enum",
    "f32",
    "i32",
    "i64",
    "str",
    "i128",
    "obj",
    "i8",
    "i16",
    "u128",
    "u64",
    "u32",
    "u16",
  ];
  const LITERAL_KEYWORDS = [
    "false",
    "abyss",
    "nil",
    "null",
    "void",
    "hole",
    "true",
  ];
  const NORMAL_KEYWORDS = [
    "file",
    "as",
    "base",
    "break",
    "case",
    "catch",
    "class",
    "const",
    "continue",
    "do",
    "else",
    "finally",
    "for",
    "if",
    "in",
    "interface",
    "namespace",
    "new",
    "cpy",
    "ref",
    "params",
    "return",
    "sizeof",
    "struct",
    "switch",
    "me",
    "throw",
    "try",
    "type",
    "typeof",
    "while",
  ];
  const CONTEXTUAL_KEYWORDS = [
    "alias",
    "async",
    "await",
    "from",
    "get",
    "import",
    "let",
    "partial",
    "remove",
    "select",
    "set",
    "use",
    "value|0",
    "yield",
  ];

  const KEYWORDS = {
    keyword: NORMAL_KEYWORDS.concat(CONTEXTUAL_KEYWORDS),
    built_in: BUILT_IN_KEYWORDS,
    literal: LITERAL_KEYWORDS,
  };
  const TITLE_MODE = hljs.inherit(hljs.TITLE_MODE, {
    begin: "[a-zA-Z](\\.?\\w)*",
  });
  const NUMBER_SUFFIX = "([ui](8|16|32|64|128)|f(32|64))\?";
  const NUMBERS = {
    className: "number",
    variants: [
      { begin: "\\b(0b[01']+)" + NUMBER_SUFFIX },
      {
        begin:
          "(-?)\\b([\\d']+(\\.[\\d']*)?|\\.[\\d']+)(u|U|l|L|ul|UL|f|F|b|B)" +
          NUMBER_SUFFIX,
      },
      {
        begin:
          "(-?)(\\b0[xX][a-fA-F0-9']+|(\\b[\\d']+(\\.[\\d']*)?|\\.[\\d']+)([eE][-+]?[\\d']+)?)" +
          NUMBER_SUFFIX,
      },
    ],
    relevance: 0,
  };
  const VERBATIM_STRING = {
    className: "string",
    begin: '@"',
    end: '"',
    contains: [{ begin: '""' }],
  };
  const SUBST = {
    className: "subst",
    begin: /\$\{/,
    end: /\}/,
    keywords: KEYWORDS,
  };
  const SUBST_NO_LF = hljs.inherit(SUBST, { illegal: /\n/ });
  const INTERPOLATED_STRING = {
    className: "string",
    begin: '"',
    end: '"',
    illegal: /\n/,
    contains: [
      hljs.BACKSLASH_ESCAPE,
      SUBST_NO_LF,
    ],
  };
  const RAW_STRING = {
    className: "string",
    begin: 'raw"',
    end: '"',
    illegal: /\n/,
    contains: [
      hljs.BACKSLASH_ESCAPE,
    ],
  };
  const INTERPOLATED_VERBATIM_STRING = {
    className: "string",
    begin: '@"',
    end: '"',
    contains: [
      { begin: '""' },
      SUBST,
    ],
  };
  const INTERPOLATED_VERBATIM_STRING_NO_LF = hljs.inherit(
    INTERPOLATED_VERBATIM_STRING,
    {
      illegal: /\n/,
      contains: [
        { begin: '""' },
        SUBST_NO_LF,
      ],
    },
  );
  SUBST.contains = [
    RAW_STRING,
    INTERPOLATED_VERBATIM_STRING,
    INTERPOLATED_STRING,
    VERBATIM_STRING,
    hljs.APOS_STRING_MODE,
    hljs.QUOTE_STRING_MODE,
    NUMBERS,
    hljs.C_BLOCK_COMMENT_MODE,
  ];
  SUBST_NO_LF.contains = [
    RAW_STRING,
    INTERPOLATED_VERBATIM_STRING_NO_LF,
    INTERPOLATED_STRING,
    hljs.APOS_STRING_MODE,
    hljs.QUOTE_STRING_MODE,
    NUMBERS,
    hljs.inherit(hljs.C_BLOCK_COMMENT_MODE, { illegal: /\n/ }),
  ];
  const STRING = {
    variants: [
      RAW_STRING,
      INTERPOLATED_VERBATIM_STRING,
      INTERPOLATED_STRING,
      VERBATIM_STRING,
    ],
  };

  const GENERIC_MODIFIER = {
    begin: "<",
    end: ">",
    contains: [
      TITLE_MODE,
    ],
  };
  const AT_IDENTIFIER = {
    // decorator moment
    begin: "@" + hljs.IDENT_RE,
    relevance: 0,
  };

  return {
    name: "Jane",
    aliases: [
      "jn",
      "jane",
    ],
    keywords: KEYWORDS,
    illegal: /::/,
    contains: [
      hljs.COMMENT(
        "///",
        "$",
        {
          returnBegin: true,
          contains: [
            {
              className: "doctag",
              variants: [
                {
                  begin: "///",
                  relevance: 0,
                },
                { begin: "<!--|-->" },
                {
                  begin: "</?",
                  end: ">",
                },
              ],
            },
          ],
        },
      ),
      hljs.C_LINE_COMMENT_MODE,
      hljs.C_BLOCK_COMMENT_MODE,
      {
        className: "meta",
        begin: "#",
        end: "$",
        keywords: {
          keyword:
            "if else elif endif define undef warning error line region endregion pragma checksum",
        },
      },
      STRING,
      NUMBERS,
      {
        beginKeywords: "class interface",
        relevance: 0,
        end: /[{;=]/,
        illegal: /[^\s:,]/,
        contains: [
          TITLE_MODE,
          GENERIC_MODIFIER,
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
        ],
      },
      {
        beginKeywords: "namespace",
        relevance: 0,
        end: /[{;=]/,
        illegal: /[^\s:]/,
        contains: [
          TITLE_MODE,
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
        ],
      },
      {
        // [Attributes("")]
        className: "meta",
        begin: "^\\s*\\[(?=[\\w])",
        excludeBegin: true,
        end: "\\]",
        excludeEnd: true,
        contains: [
          {
            className: "string",
            begin: /"/,
            end: /"/,
          },
        ],
      },
      {
        // Expression keywords prevent 'keyword Name(...)' from being
        // recognized as a function definition
        beginKeywords: "new return throw await else",
        relevance: 0,
      },
      {
        className: "function",
        begin: "fn",
        returnBegin: true,
        end: /[{;=]/,
        excludeEnd: true,
        keywords: KEYWORDS,
        contains: [
          {
            begin: "-",
            contains: [
              hljs.C_LINE_COMMENT_MODE,
            ],
            end: /\s+/,
          },
          {
            begin: hljs.IDENT_RE + "\\s*(<[^=]+>\\s*)?\\(",
            returnBegin: true,
            contains: [
              hljs.TITLE_MODE,
              GENERIC_MODIFIER,
            ],
            relevance: 0,
          },
          { match: /\(\)/ },
          {
            className: "params",
            begin: /\(/,
            end: /\)/,
            excludeBegin: true,
            excludeEnd: true,
            keywords: KEYWORDS,
            relevance: 0,
            contains: [
              STRING,
              NUMBERS,
              hljs.C_BLOCK_COMMENT_MODE,
            ],
          },
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
        ],
      },
      AT_IDENTIFIER,
    ],
  };
}
