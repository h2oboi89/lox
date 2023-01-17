# Lox Grammar

## Top level
```
program        → declaration* EOF ;
```

## Declaration
```
declaration     → classDecl
                | funDecl
                | varDecl
                | statement ;

classDecl       → "class" IDENTIFIER ( ":" IDENTIFIER )? "{" function* "}" ;

funDecl         → "fun" function ;

function        → IDENTIFIER "(" parameters? ")" block ;

parameters      → IDENTIFIER ( "," IDENTIFIER )* ;

varDecl         → "var" IDENTIFIER ( "=" expression )? ";" ;
```

## Statements
```
statement       → exprStmt
                | ifStmt
                | forStmt
                | printStmt
                | returnStmt
                | whileStmt
                | block ;

exprStmt        → expression ";" ;

forStmt         → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;

ifStmt          → "if" "(" expression ")" statement ( "else" statement )? ;

printStmt       → "print" expression ";" ;

returnStmt      → "return" expression? ";" ;

whileStmt       → "while" "(" expression ")" statement ;

block           → "{" declaration* "}" ;
```

## Expressions
```
expression      → assignment ;

assignment      → ( call "." )? INDENTIFIER "=" assignment
                | logic_or ;

logic_or        → logic_and ( "or" logic_and )* ;

logic_and       → equality ( "and" equality )* ;

equality        → comparison ( ( "!=" | "==" ) comparison )* ;

comparison      → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;

term            → factor ( ( "-" | "+" ) factor )* ;

factor          → unary ( ( "/" | "*" ) unary )* ;

unary           → ( "!" | "-" ) unary 
                | call ;

call            → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;

arguments       → expression ( "," expression )* ;

primary         → "true" 
                | "false" 
                | "nil" 
                | "this"
                | NUMBER 
                | STRING 
                | IDENTIFIER
                | "(" expression ")"
                | "super" "." IDENTIFIER ;
```

## Lexical Grammar
```
NUMBER          → DIGIT+ ( "." DIGIT+ )? ;
STRING          → "\"" <any char except "\"">* "\"" ;
IDENTIFIER      → ALPHA ( ALPHA | DIGIT )* ;
ALPHA           → "a" ... "z"
                | "A" ... "Z"
                | "_" ;
DIGIT           → "0" ... "9" ;
```