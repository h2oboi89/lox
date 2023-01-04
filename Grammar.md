# Lox Grammar

## Top level
```
program        → declaration* EOF ;
```

## Statements
```
declaration    → varDecl
               | statement ;

varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;               

statement      → exprStmt
               | printStmt 
               | block ;

block          → "{" declaration* "}" ;

exprStmt       → expression ";" ;

printStmt      → "print" expression ";" ;
```

## Expressions
```
expression     → assignment ;

assignment     → INDENTIFIER "=" assignment
               | equality ;

equality       → comparison ( ( "!=" | "==" ) comparison )* ;

comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;

term           → factor ( ( "-" | "+" ) factor )* ;

factor         → unary ( ( "/" | "*" ) unary )* ;

unary          → ( "!" | "-" ) unary 
               | primary ;

primary        → NUMBER 
               | STRING 
               | "true" 
               | "false" 
               | "nil" 
               | "(" expression ")" 
               | IDENTIFIER ;
```