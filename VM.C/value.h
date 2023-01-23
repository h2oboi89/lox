#ifndef clox_value_h
#define clox_value_h

#include "common.h"

typedef struct Object Object;
typedef struct ObjectString ObjectString;

typedef enum ValueType {
    VAL_BOOL,
    VAL_NIL,
    VAL_NUMBER,
    VAL_OBJECT,
} ValueType;

typedef struct Value{
    ValueType type;
    union as {
        bool boolean;
        double number;
        Object* object;
    } as;
} Value;

#define IS_BOOL(value)		((value).type == VAL_BOOL)
#define IS_NIL(value)		((value).type == VAL_NIL)
#define IS_NUMBER(value)	((value).type == VAL_NUMBER)
#define IS_OBJECT(value)	((value).type == VAL_OBJECT)

#define AS_BOOL(value)		((value).as.boolean)
#define AS_NUMBER(value)	((value).as.number)
#define AS_OBJECT(value)	((value).as.object)

#define BOOL_VAL(value)		((Value){VAL_BOOL, {.boolean = value}})
#define NIL_VAL				((Value){VAL_NIL, {.number = 0}})
#define NUMBER_VAL(value)	((Value){VAL_NUMBER, {.number = value}})
#define OBJECT_VAL(value)	((Value){VAL_OBJECT, {.object = (Object*)value}})

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