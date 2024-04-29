#pragma once

#define TEXT(x) L##x

#define PREPROCESSOR_TO_STRINGW(x) TEXT(PREPROCESSOR_TO_STRING_INNER(x))
#define PREPROCESSOR_TO_STRING(x) PREPROCESSOR_TO_STRING_INNER(x)
#define PREPROCESSOR_TO_STRING_INNER(x) #x

#define PREPROCESSOR_JOIN(x, y) PREPROCESSOR_JOIN_INNER(x, y)
#define PREPROCESSOR_JOIN_INNER(x, y) x##y

#if defined(__clang__)
#define PRAGMA(Token) _Pragma(Token)
#else
#define PRAGMA(Token) __pragma(Token)
#endif

#define PUSH_MACRO(Name) PRAGMA(push_macro(PREPROCESSOR_TO_STRING(Name)))
#define POP_MACRO(Name) PRAGMA(pop_macro(PREPROCESSOR_TO_STRING(Name)))
#define WARNING(Code) PRAGMA(warning(Code))