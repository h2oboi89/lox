#ifndef clox_object_h
#define clox_object_h

#include "common.h"
#include "chunk.h"
#include "table.h"
#include "value.h"

#define OBJECT_TYPE(value)  (AS_OBJECT(value)->type)

#define IS_CLASS(value)     isObjectType(value, OBJECT_CLASS)
#define IS_CLOSURE(value)   isObjectType(value, OBJECT_CLOSURE)
#define IS_FUNCTION(value)  isObjectType(value, OBJECT_FUNCTION)
#define IS_INSTANCE(value)  isObjectType(value, OBJECT_INSTANCE)
#define IS_NATIVE(value)    isObjectType(value, OBJECT_NATIVE)
#define IS_STRING(value)    isObjectType(value, OBJECT_STRING)

#define AS_CLASS(value)     ((ObjectClass*)AS_OBJECT(value))
#define AS_CLOSURE(value)   ((ObjectClosure*)AS_OBJECT(value))
#define AS_FUNCTION(value)  ((ObjectFunction*)AS_OBJECT(value))
#define AS_INSTANCE(value)  ((ObjectInstance*)AS_OBJECT(value))
#define AS_NATIVE(value)    (((ObjectNative*)AS_OBJECT(value))->function)
#define AS_STRING(value)    ((ObjectString*)AS_OBJECT(value))
#define AS_CSTRING(value)   (AS_STRING(value))->chars

typedef enum ObjectType {
    OBJECT_CLASS,
    OBJECT_CLOSURE,
    OBJECT_FUNCTION,
    OBJECT_INSTANCE,
    OBJECT_NATIVE,
    OBJECT_STRING,
    OBJECT_UPVALUE,
} ObjectType;

struct Object {
    ObjectType type;
    bool isMarked;
    struct Object* next;
};

typedef struct ObjectFunction {
    Object object;
    int arity;
    int upValueCount;
    Chunk chunk;
    ObjectString* name;
} ObjectFunction;

typedef Value(*NativeFn)(int argCount, Value* args);

typedef struct ObjectNative {
    Object object;
    NativeFn function;
} ObjectNative;

struct ObjectString {
    Object object;
    int length;
    char* chars;
    uint32_t hash;
};

typedef struct ObjectUpValue {
    Object object;
    Value* location;
    Value closed;
    struct ObjectUpValue* next;
} ObjectUpValue;

typedef struct ObjectClosure {
    Object object;
    ObjectFunction* function;
    ObjectUpValue** upValues;
    int upValueCount;
} ObjectClosure;

typedef struct ObjectClass {
    Object object;
    ObjectString* name;
} ObjectClass;

typedef struct ObjectInstance {
    Object object;
    ObjectClass* loxClass;
    Table fields;
} ObjectInstance;

static inline bool isObjectType(Value value, ObjectType type) {
    return IS_OBJECT(value) && AS_OBJECT(value)->type == type;
}

ObjectClass* newClass(ObjectString* name);
ObjectClosure* newClosure(ObjectFunction* function);
ObjectFunction* newFunction();
ObjectInstance* newInstance(ObjectClass* loxClass);
ObjectNative* newNative(NativeFn function);
ObjectString* takeString(char* chars, int length);
ObjectString* copyString(const char* chars, int length);
ObjectUpValue* newUpValue(Value* slot);
void printObject(Value value);

#endif // !clox_object_h