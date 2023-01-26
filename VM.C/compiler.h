#ifndef clox_compiler_h
#define clox_compiler_h

#include "object.h"
#include "vm.h"

ObjectFunction* compile(const char* source);
void markCompilerRoots();

#endif // !clox_compiler_h
