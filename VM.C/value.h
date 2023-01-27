#ifndef clox_value_h
#define clox_value_h

#include "common.h"

typedef struct Object Object;
typedef struct ObjectString ObjectString;

typedef enum ValueType {
    VALUE_BOOL,
    VALUE_NIL,
    VALUE_NUMBER,
    VALUE_OBJECT,
} ValueType;

typedef struct Value{
    ValueType type;
    union as {
        bool boolean;
        double number;
        Object* object;
    } as;
} Value;

#define IS_BOOL(value)		((value).type == VALUE_BOOL)
#define IS_NIL(value)		((value).type == VALUE_NIL)
#define IS_NUMBER(value)	((value).type == VALUE_NUMBER)
#define IS_OBJECT(value)	((value).type == VALUE_OBJECT)

#define AS_BOOL(value)		((value).as.boolean)
#define AS_NUMBER(value)	((value).as.number)
#define AS_OBJECT(value)	((value).as.object)

#define BOOL_VAL(value)		((Value){VALUE_BOOL, {.boolean = value}})
#define NIL_VAL				((Value){VALUE_NIL, {.number = 0}})
#define NUMBER_VAL(value)	((Value){VALUE_NUMBER, {.number = value}})
#define OBJECT_VALUE(value)	((Value){VALUE_OBJECT, {.object = (Object*)value}})

typedef struct ValueArray {
    int capacity;
    int count;
    Value* values;
} ValueArray;

bool valuesEqual(Value a, Value b);
void initValueArray(ValueArray* array);
void writeValueArray(ValueArray* array, Value value);
void freeValueArray(ValueArray* array);
void printValue(Value value);

#endif // !clox_value_h