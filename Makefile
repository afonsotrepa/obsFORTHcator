#Find arch
ifeq ($(shell uname -p),x86_64)
	ARCH := AMD64
else
	ARCH := x86
endif

all: a.out

a.out: subleq-vm.o
	ld --omagic $^

subleq-vm.o: subleq-vm64.asm subleq-vm32.asm compiled.asm
ifeq ($(ARCH),AMD64)
	nasm -felf64 subleq-vm64.asm -o $@
else
	nasm -felf32 subleq-vm32.asm -o $@
endif

compiled.asm: compiler.fs obs.fs
	gforth compiler.fs | tee compiled.asm

run: a.out
	./a.out KEYKEYKEY
