#include <stdio.h>
#include <string.h>

#include "memory.h"
#include "object.h"
#include "table.h"
#include "value.h"
#include "vm.h"

#define ALLOCATE_OBJECT(type, objectType) \
    (type*)allocateObject(sizeof(type), objectType)

static Object* allocateObject(size_t size, ObjectType type) {
    Object* object = (Object*)reallocate(NULL, 0, size);
    object->type = type;
    object->isMarked = false;

    object->next = vm.objects;
    vm.objects = object;

#ifdef DEBUG_LOG_GC
    printf("%p allocate %zu for %d\n", (void*)object, size, type);
#endif

    return object;
}

ObjectBoundMethod* newBoundMethod(Value receiver, ObjectClosure* method) {
    ObjectBoundMethod* boundMethod = ALLOCATE_OBJECT(ObjectBoundMethod, OBJECT_BOUND_METHOD);
    boundMethod->receiver = receiver;
    boundMethod->method = method;
    return boundMethod;
}

ObjectClass* newClass(ObjectString* name) {
    ObjectClass* loxClass = ALLOCATE_OBJECT(ObjectClass, OBJECT_CLASS);
    loxClass->name = name;
    initTable(&loxClass->methods);
    return loxClass;
}

ObjectClosure* newClosure(ObjectFunction* function) {
    ObjectUpValue** upValues = ALLOCATE(ObjectUpValue*, function->upValueCount);
    for (int i = 0; i < function->upValueCount; i++) {
        upValues[i] = NULL;
    }

    ObjectClosure* closure = ALLOCATE_OBJECT(ObjectClosure, OBJECT_CLOSURE);
    closure->function = function;
    closure->upValues = upValues;
    closure->upValueCount = function->upValueCount;
    return closure;
}

ObjectFunction* newFunction() {
    ObjectFunction* function = ALLOCATE_OBJECT(ObjectFunction, OBJECT_FUNCTION);
    function->arity = 0;
    function->upValueCount = 0;
    function->name = NULL;
    initChunk(&function->chunk);
    return function;
}

ObjectInstance* newInstance(ObjectClass* loxClass) {
    ObjectInstance* instance = ALLOCATE_OBJECT(ObjectInstance, OBJECT_INSTANCE);
    instance->loxClass = loxClass;
    initTable(&instance->fields);
    return instance;
}

ObjectNative* newNative(NativeFn function) {
    ObjectNative* native = ALLOCATE_OBJECT(ObjectNative, OBJECT_NATIVE);
    native->function = function;
    return native;
}

static ObjectString* allocateString(char* chars, int length, uint32_t hash) {
    ObjectString* string = ALLOCATE_OBJECT(ObjectString, OBJECT_STRING);
    string->length = length;
    string->chars = chars;
    string->hash = hash;

    push(OBJECT_VALUE(string));
    tableSet(&vm.strings, string, NIL_VALUE);
    pop();

    return string;
}

uint32_t hashString(const char* key, int length) {
    uint32_t hash = 2166136261u;
    for (int i = 0; i < length; i++) {
        hash ^= (uint8_t)key[i];
        hash *= 16777619;
    }
    return hash;
}

ObjectString* takeString(char* chars, int length) {
    uint32_t hash = hashString(chars, length);
    ObjectString* interned = tableFindString(&vm.strings, chars, length, hash);
    if (interned != NULL) {
        FREE_ARRAY(char, chars, length + 1);
        return interned;
    }
    return allocateString(chars, length, hash);
}

ObjectString* copyString(const char* chars, int length) {
    uint32_t hash = hashString(chars, length);
    ObjectString* interned = tableFindString(&vm.strings, chars, length, hash);
    if (interned != NULL) return interned;

    char* heapChars = ALLOCATE(char, length + 1);
    memcpy(heapChars, chars, length);
    heapChars[length] = '\0';
    return allocateString(heapChars, length, hash);
}

ObjectUpValue* newUpValue(Value* slot)
{
    ObjectUpValue* upValue = ALLOCATE_OBJECT(ObjectUpValue, OBJECT_UPVALUE);
    upValue->location = slot;
    upValue->next = NULL;
    upValue->closed = NIL_VALUE;
    return upValue;
}

static void printFunction(ObjectFunction* function) {
    if (function->name == NULL) {
        printf("<script>");
    }
    else {
        printf("<fn %s>", function->name->chars);
    }
}

void printObject(Value value) {
    switch (OBJECT_TYPE(value))
    {
    case OBJECT_BOUND_METHOD:
        printFunction(AS_BOUND_METHOD(value)->method->function);
        break;
    case OBJECT_CLASS:
        printf("%s", AS_CLASS(value)->name->chars);
        break;
    case OBJECT_CLOSURE:
        printFunction(AS_CLOSURE(value)->function);
        break;
    case OBJECT_FUNCTION:
        printFunction(AS_FUNCTION(value));
        break;
    case OBJECT_INSTANCE:
        printf("%s instance", AS_INSTANCE(value)->loxClass->name->chars);
        break;
    case OBJECT_NATIVE:
        printf("<native fn>");
        break;
    case OBJECT_STRING:
        printf("%s", AS_CSTRING(value));
        break;
    case OBJECT_UPVALUE:
        printf("upValue");
        break;
    }
}