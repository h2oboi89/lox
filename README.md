# [lox](https://github.com/h2oboi89/lox)

## Summary

Lox interpreter written in C# (.NET 7)

This is my 2nd attempt. First one can be found here: https://github.com/h2oboi89/lox.archive

Based on Bob Nystrom's Lox book
 - website: <a href="http://craftinginterpreters.com/">Crafting Interpreters</a>
 - github:  <a href="https://github.com/munificent/craftinginterpreters">munificent/craftinginterpreters</a>

## Sections Completed

Links are organized as [online book link] : [source code tag link].

NOTE: some sections do not contain a tag link as they only represent a partial section of code that did not lend itself to testing.

### Tree-walk Interpreter

- [x] [Scanning](http://craftinginterpreters.com/scanning.html) : [Scanning](https://github.com/h2oboi89/lox/releases/tag/Scanning)
- [x] [Representing Code](http://craftinginterpreters.com/representing-code.html)
- [x] [Parsing Expressions](http://craftinginterpreters.com/parsing-expressions.html) : [Parsing](https://github.com/h2oboi89/lox/releases/tag/Parsing)
  - this tag fixes some missing code that should have been in Scanning tag.
- [x] [Evaluating Expressions](http://craftinginterpreters.com/evaluating-expressions.html) : [EvaluatingExpressions](https://github.com/h2oboi89/lox/releases/tag/EvaluatingExpressions)
- [x] [Statements and State](http://craftinginterpreters.com/statements-and-state.html) : [Statements](https://github.com/h2oboi89/lox/releases/tag/Statements)
- [x] [Control Flow](http://craftinginterpreters.com/control-flow.html) : [ControlFlow](https://github.com/h2oboi89/lox/releases/tag/ControlFlow)
- [x] [Functions](http://craftinginterpreters.com/functions.html) : [Functions](https://github.com/h2oboi89/lox/releases/tag/Functions)
- [x] [Resolving and Binding](http://craftinginterpreters.com/resolving-and-binding.html) : [ResolveAndBinding](https://github.com/h2oboi89/lox/releases/tag/ResolveAndBinding)
- [x] [Classes](http://craftinginterpreters.com/classes.html) : [Classes](https://github.com/h2oboi89/lox/releases/tag/Classes)
- [ ] [Inheritance](http://craftinginterpreters.com/inheritance.html)

### Bytecode Virtual Machine

## Deviations / Enchancements
- Reworked interpreter to facilitate testing (see [Interpreter.Tests](https://github.com/h2oboi89/lox/tree/main/Interpreter.Tests))
- Superclass format is like C# (`class : superclass`) instead of using `<`

### TODO
- block comments
- ternary expressions
- comma separated expression (var a = 1, var b = 2;)
- detect divide by zero
- REPL autoprints expressions
- add continue and break
- anonymous functions
- report unused variables
- static methods for classes
- public, private, protected fields and methods
- getters and setters
- multiple inheritance
- interfaces
- read user input built-ins
- namespaces and imports?

- enforce class name is upper case
- virtual, abstract, override
- warn user if field overrides method name
- make print a built-in function
- global scope not special (no more multiple declarations)
- allow single line functions (no braces)