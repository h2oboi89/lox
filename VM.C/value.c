#include <stdio.h>
#include <string.h>

#include "value.h"
#include "memory.h"
#include "object.h"

void initValueArray(ValueArray* array) {
    array->count = 0;
    array->capacity = 0;
    array->values = NULL;
}

void writeValueArray(ValueArray* array, Value value) {
    if (array->capacity < array->count + 1) {
        int oldCapacity = array->capacity;
        array->capacity = GROW_CAPACITY(oldCapacity);
        array->values = GROW_ARRAY(Value, array->values,
            oldCapacity, array->capacity);
    }

    array->values[array->count] = value;
    array->count++;
}

void freeValueArray(ValueArray* array) {
    FREE_ARRAY(Value, array->values, array->capacity);
    initValueArray(array);
}

void printValue(Value value) {
    switch (value.type)
    {
    case VALUE_BOOL:    printf(AS_BOOL(value) ? "true" : "false"); break;
    case VALUE_NIL:     printf("nil"); break;
    case VALUE_NUMBER:  printf("%g", AS_NUMBER(value)); break;
    case VALUE_OBJECT:  printObject(value); break;
    default: break;
    }
}

bool valuesEqual(Value a, Value b) {
    if (a.type != b.type) return false;

    switch (a.type)
    {
    case VALUE_BOOL:    return AS_BOOL(a) == AS_BOOL(b);
    case VALUE_NIL:     return true;
    case VALUE_NUMBER:  return AS_NUMBER(a) == AS_NUMBER(b);
    case VALUE_OBJECT:  return AS_OBJECT(a) == AS_OBJECT(b);
    default:            return false;
    }
}