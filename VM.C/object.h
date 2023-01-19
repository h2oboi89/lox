#ifndef clox_object_h
#define clox_object_h

#include "common.h"
#include "chunk.h"
#include "value.h"

#define OBJECT_TYPE(value)  (AS_OBJECT(value)->type)

#define IS_FUNCTION(value)  isObjectType(value, OBJECT_FUNCTION)
#define IS_STRING(value)    isObjectType(value, OBJECT_STRING)

#define AS_FUNCTION(value)  ((ObjectFunction*)AS_OBJECT(value))
#define AS_STRING(value)    ((ObjectString*)AS_OBJECT(value))
#define AS_CSTRING(value)   (AS_STRING(value))->chars

typedef enum {
    OBJECT_FUNCTION,
    OBJECT_STRING,
} ObjectType;

struct Object {
    ObjectType type;
    struct Object* next;
};

typedef struct {
    Object object;
    int arity;
    Chunk chunk;
    ObjectString* name;
} ObjectFunction;

struct ObjectString {
    Object object;
    int length;
    char* chars;
    uint32_t hash;
};

static inline bool isObjectType(Value value, ObjectType type) {
    return IS_OBJECT(value) && AS_OBJECT(value)->type == type;
}

ObjectFunction* newFunction();
ObjectString* takeString(char* chars, int length);
ObjectString* copyString(const char* chars, int length);
void printObject(Value value);

#endif // !clox_object_h