SHELL := C:\Windows\System32\cmd.exe

PACKAGE_ROOT := $(shell echo $(USERPROFILE))\.nuget\packages

TDD_TOOL := $(PACKAGE_ROOT)\nunit.consolerunner\3.16.0\tools\nunit3-console.exe
TDD_DIR := .\OpenCover

COVERAGE_TOOL := $(PACKAGE_ROOT)\opencover\4.7.1221\tools\OpenCover.Console.exe
COVERAGE_REPORT_TOOL := $(PACKAGE_ROOT)\reportgenerator\5.1.13\tools\net7.0\ReportGenerator.exe
COVERAGE_REPORT := $(TDD_DIR)\results.xml

CONFIG := Debug

TESTS = .\Interpreter.Tests\bin\$(CONFIG)\net7.0\Interpreter.Tests.dll

OPENCOVER_ASSEMBLY_FILTER := -nunit.framework;-Interpreter.Tests;

GIT_LONG_HASH := $(shell git rev-parse HEAD)
GIT_SHORT_HASH := $(shell git rev-parse --short HEAD)

# Deletes directory if it exists
# $1 Directory to delete
define delete_dir
	@if EXIST $1 rmdir $1 /s /q;
endef

.DEFAULT_GOAL := tdd

.PHONY: build
build:
	dotnet build -c $(CONFIG) -v q --nologo

.PHONY: tdd
tdd: build
	$(call delete_dir,$(TDD_DIR))
	@md $(TDD_DIR)
	$(COVERAGE_TOOL) -target:$(TDD_TOOL) -targetargs:"$(TESTS) --work=$(TDD_DIR)" -register:user -output:$(COVERAGE_REPORT)
	$(COVERAGE_REPORT_TOOL) -reports:$(COVERAGE_REPORT) -targetdir:$(TDD_DIR) -assemblyFilters:$(OPENCOVER_ASSEMBLY_FILTER) -verbosity:Warning -tag:$(GIT_LONG_HASH)

.PHONY: debug
debug: CONFIG := Debug
debug: build tdd

.PHONY: release
release: CONFIG := Release
release: build tdd

.PHONY: ast
ast: release
	.\Interpreter.GenerateAst\bin\Release\net7.0\generate_ast.exe .\Interpreter.Framework\AST\

.PHONY: run
run: release
	.\Interpreter\bin\Release\net7.0\lox.exe

.PHONY: clean
clean:
	dotnet clean --nologo -v quiet
