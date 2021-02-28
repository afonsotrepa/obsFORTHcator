all: a.out

a.out: subleq-vm.o
	ld --omagic $^

subleq-vm.o: subleq-vm.asm compiled.asm
	nasm -felf64 $< -o $@

compiled.asm: compiler.fs obs.fs
	gforth compiler.fs | tee compiled.asm

run: a.out
	./a.out KEYKEYKEY
