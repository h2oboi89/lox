#pragma once
#ifndef clox_common_h
#define clox_common_h

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#if _DEBUG
#define DEBUG_PRINT_CODE
#define DEBUG_TRACE_EXECUTION
#endif

#define UINT8_COUNT (UINT8_MAX + 1)

#endif // !clox_common_h