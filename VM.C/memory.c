#include <stdlib.h>

#include "memory.h"
#include "vm.h"

#ifdef DEBUG_LOG_GC
#include <stdio.h>
#include "debug.h"
#endif

#define GC_HEAP_GROW_FACTOR 2

static void* reallocWrapper(void* block, size_t size) {
    void* pointer = realloc(block, size);

    if (pointer == NULL) exit(1);

    return pointer;
}

void* reallocate(void* pointer, size_t oldSize, size_t newSize) {
    vm.bytesAllocated += newSize - oldSize;

    if (newSize > oldSize) {
#ifdef DEBUG_STRESS_GC
        collectGarbage();
#endif

        if (vm.bytesAllocated > vm.nextGC) {
            collectGarbage();
        }
    }

    if (newSize == 0) {
        free(pointer);
        return NULL;
    }

    return reallocWrapper(pointer, newSize);
}

void markObject(Object* object) {
    if (object == NULL) return;
    if (object->isMarked) return;

#ifdef DEBUG_LOG_GC
    printf("%p mark ", (void*)object);
    printValue(OBJECT_VALUE(object));
    printf("\n");
#endif

    object->isMarked = true;

    if (vm.grayCapacity < vm.grayCount + 1) {
        vm.grayCapacity = GROW_CAPACITY(vm.grayCapacity);
        vm.grayStack = (Object**)reallocWrapper(vm.grayStack, sizeof(Object*) * vm.grayCapacity);
    }

    vm.grayStack[vm.grayCount++] = object;
}

void markValue(Value value) {
    if (IS_OBJECT(value)) markObject(AS_OBJECT(value));
}

static void markArray(ValueArray* array) {
    for (int i = 0; i < array->count; i++) {
        markValue(array->values[i]);
    }
}

static void blackenObject(Object* object) {
#ifdef DEBUG_LOG_GC
    printf("%p blacken ", (void*)object);
    printValue(OBJECT_VALUE(object));
    printf("\n");
#endif

    switch (object->type)
    {
    case OBJECT_CLASS: {
        ObjectClass* loxClass = (ObjectClass*)object;
        markObject((Object*)loxClass->name);
        markTable(&loxClass->methods);
        break;
    }
    case OBJECT_CLOSURE: {
        ObjectClosure* closure = (ObjectClosure*)object;
        markObject((Object*)closure->function);
        for (int i = 0; i < closure->upValueCount; i++) {
            markObject((Object*)closure->upValues[i]);
        }
        break;
    }
    case OBJECT_FUNCTION: {
        ObjectFunction* function = (ObjectFunction*)object;
        markObject((Object*)function->name);
        markArray(&function->chunk.constants);
        break;
    }
    case OBJECT_INSTANCE: {
        ObjectInstance* instance = (ObjectInstance*)object;
        markObject((Object*)instance->loxClass);
        markTable(&instance->fields);
        break;
    }
    case OBJECT_UPVALUE: {
        markValue(((ObjectUpValue*)object)->closed);
        break;
    }
    case OBJECT_NATIVE:
    case OBJECT_STRING:
        break;
    }
}

static void freeObject(Object* object) {
#ifdef DEBUG_LOG_GC
    printf("%p free trype %d\n", (void*)object, object->type);
#endif

    switch (object->type)
    {
    case OBJECT_CLASS: {
        ObjectClass* loxClass = (ObjectClass*)object;
        freeTable(&loxClass->methods);
        FREE(ObjectClass, object);
        break;
    }
    case OBJECT_CLOSURE: {
        ObjectClosure* closure = (ObjectClosure*)object;
        FREE_ARRAY(ObjectUpValue*, closure->upValues, closure->upValueCount);
        FREE(OBJECT_CLOSURE, object);
        break;
    }
    case OBJECT_FUNCTION: {
        ObjectFunction* function = (ObjectFunction*)object;
        freeChunk(&function->chunk);
        FREE(OBJECT_FUNCTION, object);
        break;
    }
    case OBJECT_INSTANCE: {
        ObjectInstance* instance = (ObjectInstance*)object;
        freeTable(&instance->fields);
        FREE(ObjectInstance, object);
        break;
    }
    case OBJECT_NATIVE: {
        FREE(ObjectNative, object);
        break;
    }
    case OBJECT_STRING: {
        ObjectString* string = (ObjectString*)object;
        FREE_ARRAY(char, string->chars, string->length + 1);
        FREE(ObjectString, object);
        break;
    }
    case OBJECT_UPVALUE: {
        FREE(OBJECT_UPVALUE, object);
        break;
    }
    }
}

static void markRoots() {
    for (Value* slot = vm.stack; slot < vm.stackTop; slot++) {
        markValue(*slot);
    }

    for (int i = 0; i < vm.frameCount; i++) {
        markObject((Object*)vm.frames[i].closure);
    }

    for (ObjectUpValue* upValue = vm.openUpValues; upValue != NULL; upValue = upValue->next) {
        markObject((Object*)upValue);
    }

    markTable(&vm.globals);
    markCompilerRoots();
}

static void traceReferences() {
    while (vm.grayCount > 0) {
        Object* object = vm.grayStack[--vm.grayCount];
        blackenObject(object);
    }
}

static void sweep() {
    Object* previous = NULL;
    Object* object = vm.objects;

    while (object != NULL) {
        if (object->isMarked) {
            object->isMarked = false;
            previous = object;
            object = object->next;
        }
        else {
            Object* unreached = object;
            object = object->next;

            if (previous != NULL) {
                previous->next = object;
            }
            else {
                vm.objects = object;
            }

            freeObject(unreached);
        }
    }
}

void collectGarbage()
{
#ifdef DEBUG_LOG_GC
    printf("-- gc begin --\n");
    size_t before = vm.bytesAllocated;
#endif

    markRoots();
    traceReferences();
    tableRemoveWhite(&vm.strings);
    sweep();

    vm.nextGC = vm.bytesAllocated * GC_HEAP_GROW_FACTOR;

#ifdef DEBUG_LOG_GC
    printf("-- gc end --\n");
    printf("   collected %zu bytes (from %zu to %zu) next at %zu\n",
        before - vm.bytesAllocated,
        before,
        vm.bytesAllocated,
        vm.nextGC);
#endif
}

void freeObjects() {
    Object* object = vm.objects;
    while (object != NULL) {
        Object* next = object->next;
        freeObject(object);
        object = next;
    }

    free(vm.grayStack);
}