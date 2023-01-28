#include <stdio.h>
#include <time.h>

int fib(int n) {
    if (n < 2) return n;

    return fib(n - 1) + fib(n - 2);
}

int main(int argc, const char* argv[]) {
    long before = clock();
    printf("%d\n", fib(40));
    long after = clock();
    printf("0.%3d\n", after - before);
}