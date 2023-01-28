#ifndef clox_value_h
#define clox_value_h

#include <string.h>

#include "common.h"

typedef struct Object Object;
typedef struct ObjectString ObjectString;

#ifdef NAN_BOXING

#define SIGN_BIT    ((uint64_t)0x8000000000000000)
#define QNAN        ((uint64_t)0x7ffc000000000000)

#define TAG_NIL     1
#define TAG_FALSE   2
#define TAG_TRUE    3
#define TAG_OBJECT  (SIGN_BIT | QNAN)

typedef uint64_t Value;

#define FALSE_VALUE             ((Value)(uint64_t)(QNAN | TAG_FALSE))
#define TRUE_VALUE              ((Value)(uint64_t)(QNAN | TAG_TRUE))
#define NIL_VALUE               ((Value)(uint64_t)(QNAN | TAG_NIL))

#define IS_BOOL(value)          (((value) | 1) == TRUE_VALUE)
#define IS_NIL(value)           ((value) == NIL_VALUE)
#define IS_NUMBER(value)        (((value) & QNAN) != QNAN)
#define IS_OBJECT(value)        (((value) & (TAG_OBJECT)) == (TAG_OBJECT))

#define AS_BOOL(value)          ((value) == TRUE_VALUE)
#define AS_NUMBER(value)        valueToNum(value)
#define AS_OBJECT(value)        ((Object*)(uintptr_t)((value) & ~(TAG_OBJECT)))

#define BOOL_VALUE(b)           ((b) ? TRUE_VALUE : FALSE_VALUE)
#define NUMBER_VALUE(num)       numToValue(num)
#define OBJECT_VALUE(object)    (Value)(TAG_OBJECT | (uint64_t)(uintptr_t)(object))

static inline Value numToValue(double num) {
    Value value;
    memcpy(&value, &num, sizeof(double));
    return value;
}

static inline double valueToNum(Value value) {
    double num;
    memcpy(&num, &value, sizeof(Value));
    return num;
}

#else

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

#define BOOL_VALUE(value)	((Value){VALUE_BOOL, {.boolean = value}})
#define NIL_VALUE		    ((Value){VALUE_NIL, {.number = 0}})
#define NUMBER_VALUE(value)	((Value){VALUE_NUMBER, {.number = value}})
#define OBJECT_VALUE(value)	((Value){VALUE_OBJECT, {.object = (Object*)value}})

#endif // NAN_BOXING

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