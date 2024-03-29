#include <stdarg.h>
#include <stdio.h>
#include <string.h>
#include <time.h>

#include "common.h"
#include "compiler.h"
#include "debug.h"
#include "memory.h"
#include "object.h"
#include "vm.h"

VM vm;

static Value clockNative(int argCount, Value* args) {
    return NUMBER_VALUE((double)clock() / CLOCKS_PER_SEC);
}

static void resetStack() {
    vm.stackTop = vm.stack;
    vm.frameCount = 0;
    vm.openUpValues = NULL;
}

static void runtimeError(const char* format, ...) {
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    fputs("\n", stderr);

    for (int i = vm.frameCount - 1; i >= 0; i--) {
        CallFrame* frame = &vm.frames[i];
        ObjectFunction* function = frame->closure->function;
        size_t instruction = frame->ip - function->chunk.code - 1;

        fprintf(stderr, "[line %d] in ", function->chunk.lines[instruction]);
        if (function->name == NULL) {
            fprintf(stderr, "script\n");
        }
        else {
            fprintf(stderr, "%s()\n", function->name->chars);
        }
    }

    resetStack();
}

static void defineNative(const char* name, NativeFn function) {
    push(OBJECT_VALUE(copyString(name, (int)strlen(name))));
    push(OBJECT_VALUE(newNative(function)));
    tableSet(&vm.globals, AS_STRING(vm.stack[0]), vm.stack[1]);
    pop();
    pop();
}

void initVM() {
    resetStack();
    vm.objects = NULL;

    vm.bytesAllocated = 0;
    vm.nextGC = 1024 * 1024;
    vm.grayCount = 0;
    vm.grayCapacity = 0;
    vm.grayStack = NULL;
    initTable(&vm.globals);
    initTable(&vm.strings);

    vm.initString = NULL;
    vm.initString = copyString("init", 4);

    defineNative("clock", clockNative);
};

void freeVM() {
    freeTable(&vm.globals);
    freeTable(&vm.strings);
    vm.initString = NULL;
    freeObjects();
};

void push(Value value) {
    *vm.stackTop = value;
    vm.stackTop++;
}

Value pop() {
    vm.stackTop--;
    return *vm.stackTop;
}

static Value peek(int distance) {
    return vm.stackTop[-1 - distance];
}

static bool call(ObjectClosure* closure, int argCount) {
    if (argCount != closure->function->arity) {
        runtimeError("Expected %d arguments but got %d", closure->function->arity, argCount);
        return false;
    }

    if (vm.frameCount == FRAMES_MAX) {
        runtimeError("Stack overflow.");
        return false;
    }

    CallFrame* frame = &vm.frames[vm.frameCount++];
    frame->closure = closure;
    frame->ip = closure->function->chunk.code;
    frame->slots = vm.stackTop - argCount - 1;
    return true;
}

static bool callValue(Value callee, int argCount) {
    if (IS_OBJECT(callee)) {
        switch (OBJECT_TYPE(callee))
        {
        case OBJECT_BOUND_METHOD: {
            ObjectBoundMethod* boundMethod = AS_BOUND_METHOD(callee);
            vm.stackTop[-argCount - 1] = boundMethod->receiver;
            return call(boundMethod->method, argCount);
        }
        case OBJECT_CLOSURE:
            return call(AS_CLOSURE(callee), argCount);
        case OBJECT_CLASS: {
            ObjectClass* loxClass = AS_CLASS(callee);
            vm.stackTop[-argCount - 1] = OBJECT_VALUE(newInstance(loxClass));
            Value initializer;
            if (tableGet(&loxClass->methods, vm.initString, &initializer)) {
                return call(AS_CLOSURE(initializer), argCount);
            }
            else if (argCount != 0) {
                runtimeError("Expected 0 arguments but got %d", argCount);
                return false;
            }

            return true;
        }
        case OBJECT_NATIVE: {
            NativeFn native = AS_NATIVE(callee);
            Value result = native(argCount, vm.stackTop - argCount);
            vm.stackTop -= argCount + 1;
            push(result);
            return true;
        }
        default:
            break;
        }
    }
    runtimeError("Can only call functions and classes.");
    return false;
}

static bool invokeFromClass(ObjectClass* loxClass, ObjectString* name, int argCount) {
    Value method;
    if (!tableGet(&loxClass->methods, name, &method)) {
        runtimeError("Undefined property '%s'.", name->chars);
        return false;
    }
    return call(AS_CLOSURE(method), argCount);
}

static bool invoke(ObjectString* name, int argCount) {
    Value receiver = peek(argCount);

    if (!IS_INSTANCE(receiver)) {
        runtimeError("Only instances have methods.");
        return false;
    }

    ObjectInstance* instance = AS_INSTANCE(receiver);

    Value value;
    if (tableGet(&instance->fields, name, &value)) {
        vm.stackTop[-argCount - 1] = value;
        return callValue(value, argCount);
    }

    return invokeFromClass(instance->loxClass, name, argCount);
}

static bool bindMethod(ObjectClass* loxClass, ObjectString* name) {
    Value method;
    if (!tableGet(&loxClass->methods, name, &method)) {
        runtimeError("Undefined property '%s'.", name->chars);
        return false;
    }

    ObjectBoundMethod* boundMethod = newBoundMethod(peek(0), AS_CLOSURE(method));

    pop();
    push(OBJECT_VALUE(boundMethod));
    return true;
}

static ObjectUpValue* captureUpValue(Value* local) {
    ObjectUpValue* previousUpValue = NULL;
    ObjectUpValue* upValue = vm.openUpValues;
    while (upValue != NULL && upValue->location > local) {
        previousUpValue = upValue;
        upValue = upValue->next;
    }

    if (upValue != NULL && upValue->location == local) {
        return upValue;
    }

    ObjectUpValue* createdUpValue = newUpValue(local);
    createdUpValue->next = upValue;

    if (previousUpValue == NULL) {
        vm.openUpValues = createdUpValue;
    }
    else {
        previousUpValue->next = createdUpValue;
    }
    return createdUpValue;
}

static void closeUpValues(Value* last) {
    while (vm.openUpValues != NULL && vm.openUpValues->location >= last) {
        ObjectUpValue* upValue = vm.openUpValues;
        upValue->closed = *upValue->location;
        upValue->location = &upValue->closed;
        vm.openUpValues = upValue->next;
    }
}

static void defineMethod(ObjectString* name) {
    Value method = peek(0);
    ObjectClass* loxClass = AS_CLASS(peek(1));
    tableSet(&loxClass->methods, name, method);
    pop();
}

static bool isFalsey(Value value) {
    return IS_NIL(value) || (IS_BOOL(value) && !AS_BOOL(value));
}

static void concatenate() {
    ObjectString* b = AS_STRING(peek(0));
    ObjectString* a = AS_STRING(peek(1));

    int length = a->length + b->length;
    char* chars = ALLOCATE(char, length + 1);
    memcpy(chars, a->chars, a->length);
    memcpy(chars + a->length, b->chars, b->length);
    chars[length] = '\0';

    ObjectString* result = takeString(chars, length);
    pop();
    pop();
    push(OBJECT_VALUE(result));
}

static InterpretResult run() {
    CallFrame* frame = &vm.frames[vm.frameCount - 1];

#define READ_BYTE() (*frame->ip++)

#define READ_SHORT() (frame->ip += 2, (uint16_t)((frame->ip[-2] << 8) | frame->ip[-1]))

#define READ_CONSTANT() (frame->closure->function->chunk.constants.values[READ_BYTE()])

#define READ_STRING() AS_STRING(READ_CONSTANT())

#define BINARY_OP(valueType, op) \
    do { \
        if (!IS_NUMBER(peek(0)) || !IS_NUMBER(peek(1))) { \
            runtimeError("Operands must be numbers."); \
            return INTERPRET_RUNTIME_ERROR; \
        } \
        double b = AS_NUMBER(pop()); \
        double a = AS_NUMBER(pop()); \
        push(valueType(a op b)); \
    } while (false)

#ifdef DEBUG_TRACE_EXECUTION
    printf("\nEXECUTION START\n");
#endif

    for (;;) {
#ifdef DEBUG_TRACE_EXECUTION
        printf("          ");
        for (Value* slot = vm.stack; slot < vm.stackTop; slot++) {
            printf("[");
            printValue(*slot);
            printf("]");
        }
        printf("\n");

        disassembleInstruction(
            &frame->closure->function->chunk,
            (int)(frame->ip - frame->closure->function->chunk.code)
        );
#endif // !DEBUG_TRACE_EXECUTION

        uint8_t instruction = READ_BYTE();

        switch (instruction)
        {
        case OP_CONSTANT: {
            Value constant = READ_CONSTANT();
            push(constant);
            break;
        }
        case OP_NIL: push(NIL_VALUE); break;
        case OP_TRUE: push(BOOL_VALUE(true)); break;
        case OP_FALSE: push(BOOL_VALUE(false)); break;

        case OP_POP: pop(); break;

        case OP_GET_LOCAL: {
            uint8_t slot = READ_BYTE();
            push(frame->slots[slot]);
            break;
        }
        case OP_SET_LOCAL: {
            uint8_t slot = READ_BYTE();
            frame->slots[slot] = peek(0);
            break;
        }
        case OP_GET_GLOBAL: {
            ObjectString* name = READ_STRING();
            Value value;
            if (!tableGet(&vm.globals, name, &value)) {
                runtimeError("Undefined variable '%s'.", name->chars);
                return INTERPRET_RUNTIME_ERROR;
            }
            push(value);
            break;
        }
        case OP_DEFINE_GLOBAL: {
            ObjectString* name = READ_STRING();
            tableSet(&vm.globals, name, peek(0));
            pop();
            break;
        }
        case OP_SET_GLOBAL: {
            ObjectString* name = READ_STRING();
            if (tableSet(&vm.globals, name, peek(0))) {
                tableDelete(&vm.globals, name);
                runtimeError("Undefined variable '%s'.", name->chars);
                return INTERPRET_RUNTIME_ERROR;
            }
            break;
        }
        case OP_GET_UPVALUE: {
            uint8_t slot = READ_BYTE();
            push(*frame->closure->upValues[slot]->location);
            break;
        }
        case OP_SET_UPVALUE: {
            uint8_t slot = READ_BYTE();
            *frame->closure->upValues[slot]->location = peek(0);
            break;
        }
        case OP_GET_PROPERTY: {
            if (!IS_INSTANCE(peek(0))) {
                runtimeError("Only instances have properties.");
                return INTERPRET_RUNTIME_ERROR;
            }

            ObjectInstance* instance = AS_INSTANCE(peek(0));
            ObjectString* name = READ_STRING();

            Value value;
            if (tableGet(&instance->fields, name, &value)) {
                pop(); // instance
                push(value);
                break;
            }

            if (!bindMethod(instance->loxClass, name)) {
                return INTERPRET_RUNTIME_ERROR;
            }

            break;
        }
        case OP_SET_PROPERTY: {
            if (!IS_INSTANCE(peek(1))) {
                runtimeError("Only instances have fields.");
                return INTERPRET_RUNTIME_ERROR;
            }

            ObjectInstance* instance = AS_INSTANCE(peek(1));
            tableSet(&instance->fields, READ_STRING(), peek(0));
            Value value = pop();
            pop(); // instance
            push(value);
            break;
        }
        case OP_GET_SUPER: {
            ObjectString* name = READ_STRING();
            ObjectClass* superClass = AS_CLASS(pop());

            if (!bindMethod(superClass, name)) {
                return INTERPRET_RUNTIME_ERROR;
            }
            break;
        }

        case OP_EQUAL: {
            Value b = pop();
            Value a = pop();
            push(BOOL_VALUE(valuesEqual(a, b)));
            break;
        }
        case OP_GREATER:    BINARY_OP(BOOL_VALUE, > ); break;
        case OP_LESS:       BINARY_OP(BOOL_VALUE, < ); break;
        case OP_ADD: {
            if (IS_STRING(peek(0)) && IS_STRING(peek(1))) {
                concatenate();
            }
            else if (IS_NUMBER(peek(0)) && IS_NUMBER(peek(1))) {
                double b = AS_NUMBER(pop());
                double a = AS_NUMBER(pop());
                push(NUMBER_VALUE(a + b));
            }
            else {
                runtimeError("Operands must be two numbers or two strings.");
                return INTERPRET_RUNTIME_ERROR;
            }
            break;
        }
        case OP_SUBTRACT:	BINARY_OP(NUMBER_VALUE, -); break;
        case OP_MULTIPLY:	BINARY_OP(NUMBER_VALUE, *); break;
        case OP_DIVIDE:		BINARY_OP(NUMBER_VALUE, / ); break;

        case OP_NOT:
            push(BOOL_VALUE(isFalsey(pop())));
            break;
        case OP_NEGATE:
            if (!IS_NUMBER(peek(0))) {
                runtimeError("Operand must be a number.");
                return INTERPRET_RUNTIME_ERROR;
            }
            push(NUMBER_VALUE(-AS_NUMBER(pop())));
            break;

        case OP_PRINT: {
            printValue(pop());
            printf("\n");
            break;
        }

        case OP_JUMP: {
            uint16_t offset = READ_SHORT();
            frame->ip += offset;
            break;
        }
        case OP_JUMP_IF_FALSE: {
            uint16_t offset = READ_SHORT();
            if (isFalsey(peek(0))) frame->ip += offset;
            break;
        }
        case OP_LOOP: {
            uint16_t offset = READ_SHORT();
            frame->ip -= offset;
            break;
        }

        case OP_CALL: {
            int argCount = READ_BYTE();
            if (!callValue(peek(argCount), argCount)) {
                return INTERPRET_RUNTIME_ERROR;
            }
            frame = &vm.frames[vm.frameCount - 1];
            break;
        }
        case OP_INVOKE: {
            ObjectString* method = READ_STRING();
            int argCount = READ_BYTE();
            if (!invoke(method, argCount)) {
                return INTERPRET_RUNTIME_ERROR;
            }
            frame = &vm.frames[vm.frameCount - 1];
            break;
        }
        case OP_SUPER_INVOKE: {
            ObjectString* method = READ_STRING();
            int argCount = READ_BYTE();
            ObjectClass* superClass = AS_CLASS(pop());
            if (!invokeFromClass(superClass, method, argCount)) {
                return INTERPRET_RUNTIME_ERROR;
            }
            frame = &vm.frames[vm.frameCount - 1];
            break;
        }

        case OP_CLOSURE: {
            ObjectFunction* function = AS_FUNCTION(READ_CONSTANT());
            ObjectClosure* closure = newClosure(function);
            push(OBJECT_VALUE(closure));
            for (int i = 0; i < closure->upValueCount; i++) {
                uint8_t isLocal = READ_BYTE();
                uint8_t index = READ_BYTE();

                if (isLocal) {
                    closure->upValues[i] = captureUpValue(frame->slots + index);
                }
                else {
                    closure->upValues[i] = frame->closure->upValues[index];
                }
            }
            break;
        }
        case OP_CLOSE_UPVALUE: {
            closeUpValues(vm.stackTop - 1);
            pop();
            break;
        }

        case OP_RETURN: {
            Value result = pop();
            closeUpValues(frame->slots);
            vm.frameCount--;
            if (vm.frameCount == 0) {
                pop();
                return INTERPRET_OK;
            }

            vm.stackTop = frame->slots;
            push(result);
            frame = &vm.frames[vm.frameCount - 1];
            break;
        }
        case OP_CLASS: {
            push(OBJECT_VALUE(newClass(READ_STRING())));
            break;
        }
        case OP_INHERIT: {
            Value superClass = peek(1);
            if (!IS_CLASS(superClass)) {
                runtimeError("Superclass must be a class.");
                return INTERPRET_RUNTIME_ERROR;
            }

            ObjectClass* subClass = AS_CLASS(peek(0));
            tableAddAll(&AS_CLASS(superClass)->methods, &subClass->methods);
            pop();
            break;
        }
        case OP_METHOD: {
            defineMethod(READ_STRING());
            break;
        }
        }
    }

#undef READ_BYTE
#undef READ_SHORT
#undef READ_CONSTANT
#undef READ_STRING
#undef BINARY_OP
}

InterpretResult interpret(const char* source) {
    ObjectFunction* function = compile(source);
    if (function == NULL) return INTERPRET_COMPILE_ERROR;

    push(OBJECT_VALUE(function));
    ObjectClosure* closure = newClosure(function);
    pop();
    push(OBJECT_VALUE(closure));
    call(closure, 0);

    return run();
};