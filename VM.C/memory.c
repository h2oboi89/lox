#include <stdlib.h>

#include "memory.h"
#include "vm.h"

#ifdef DEBUG_LOG_GC
#include <stdio.h>
#include "debug.h"
#endif

void* reallocate(void* pointer, size_t oldSize, size_t newSize) {
    if (newSize > oldSize) {
#ifdef DEBUG_STRESS_GC
        collectGarbage();
#endif
    }

    if (newSize == 0) {
        free(pointer);
        return NULL;
    }

    void* result = realloc(pointer, newSize);
    if (result == NULL) exit(1);
    return result;
}

void markObject(Object* object) {
    if (object == NULL) return;
    if (object->isMarked) return;

#ifdef DEBUG_LOG_GC
    printf("%p mark ", (void*)object);
    printValue(OBJECT_VAL(object));
    printf("\n");
#endif

    object->isMarked = true;

    if (vm.grayCapacity < vm.grayCount + 1) {
        vm.grayCapacity = GROW_CAPACITY(vm.grayCapacity);
        vm.grayStack = (Object**)realloc(vm.grayStack, sizeof(Object*) * vm.grayCapacity);

        if (vm.grayStack == NULL) exit(1);
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
    printValue(OBJECT_VAL(object));
    printf("\n");
#endif

    switch (object->type)
    {
    case OBJECT_CLOSURE:
        ObjectClosure* closure = (ObjectClosure*)object;
        markObject((Object*)closure->function);
        for (int i = 0; i < closure->upValueCount; i++) {
            markObject((Object*)closure->upValues[i]);
        }
        break;
    case OBJECT_FUNCTION:
        ObjectFunction* function = (ObjectFunction*)object;
        markObject((Object*)function->name);
        markArray(&function->chunk.constants);
        break;
    case OBJECT_UPVALUE:
        markValue(((ObjectUpValue*)object)->closed);
        break;
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

void collectGarbage()
{
#ifdef DEBUG_LOG_GC
    printf("-- gc begin --\n");
#endif

    markRoots();
    traceReferences();

#ifdef DEBUG_LOG_GC
    printf("-- gc end --\n");
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