# Lox Grammar

## Top level
```
program        → declaration* EOF ;
```

## Statements
```
declaration     → funDecl
                | varDecl
                | statement ;

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

assignment      → INDENTIFIER "=" assignment
                | logic_or ;

logic_or        → logic_and ( "or" logic_and )* ;

logic_and       → equality ( "and" equality )* ;

equality        → comparison ( ( "!=" | "==" ) comparison )* ;

comparison      → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;

term            → factor ( ( "-" | "+" ) factor )* ;

factor          → unary ( ( "/" | "*" ) unary )* ;

unary           → ( "!" | "-" ) unary 
                | call ;

call            → primary ( "(" arguments? ")" )* ;

arguments       → expression ( "," expression )* ;

primary         → NUMBER 
                | STRING 
                | "true" 
                | "false" 
                | "nil" 
                | "(" expression ")" 
                | IDENTIFIER ;
```