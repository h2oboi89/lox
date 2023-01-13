# Lox Grammar

## Top level
```
program        → declaration* EOF ;
```

## Statements
```
declaration     → classDecl
                | funDecl
                | varDecl
                | statement ;

classDecl       → "class" IDENTIFIER ( ":" IDENTIFIER )? "{" function* "}" ;

funDecl         → "fun" function ;

function        → IDENTIFIER "(" paramters? ")" block ;

parameters      → IDENTIFIER ( "," IDENTIFIER )* ;

varDecl         → "var" IDENTIFIER ( "=" expression )? ";" ;               

statement       → exprStmt
                | ifStmt
                | forStmt
                | printStmt
                | returnStmt
                | whileStmt
                | block ;

exprStmt        → expression ";" ;

ifStmt          → "if" "(" expression ")" statement ( "else" statement )? ;

forStmt         → "for" "(" ( varDecl | exprStmt | ";" expression? ";" expression? ")" statement ;

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