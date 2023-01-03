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
- [ ] Statements and State
- [ ] Control Flow
- [ ] Functions
- [ ] Resolving and Binding
- [ ] Classes
- [ ] Inheritance

### Bytecode Virtual Machine

## Deviations / Enchancements
- Reworked interpreter to facilitate testing (see [Interpreter.Tests](https://github.com/h2oboi89/lox/tree/main/Interpreter.Tests))
