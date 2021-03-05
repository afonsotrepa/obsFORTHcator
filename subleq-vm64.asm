;Virtual machine that emulates a subleq machine.
;Subleq code is stored in the code array from compiled.asm
BITS 64
%include "compiled.asm"

section .bss
input: 	resb 10

section .data
	db '\"You must take your opponent into a deep dark forest where 2+2=5, and the path leading out is only wide enough for one.\" ― Mikhail Tal'
	

section .text
global _start
_start: ;get input from command line arguments
	pop rax ;ignore arg count
	pop rax ;ignore program name
	pop rsi ;get input string
	mov rdi, input
	mov rcx, 10 ;length of input
.copy: 	mov al, [rsi]
	mov [rdi], al
	inc rdi
	inc rsi
	loop .copy

	mov rdi, code ;initialize the instruction pointer
.loop: 	mov rax, [rdi] ;get A
	lea rax, [rax*8] ;times 8 bytes (qword)
	mov rax, [rax+code]

	mov rbx, [rdi+8] ;get B
	;check for output instruction
	cmp rbx, -1
	je .emit
	lea rbx, [rbx*8] ;times 8 bytes (qword)


	;do subleq
	sub [rbx+code], rax
	jle .jmp

	;go to next instruction
.next: 	add rdi, 3*8
	jmp .loop

	;jump to C
.jmp: 	mov rdi, [rdi+16]
	cmp rdi, -1
	je .bye

	lea rdi, [rdi*8]
	add rdi, code
	jmp .loop

	;called when C == -1 and leq == true
.bye: 	mov rax, 60 ;sys_exit
	mov rdi, 0 ;exit code
	syscall

.emit: 	push rdi ;save rdi
	push rax ;write value to emit on the stack
	mov rax, 1 ;sys_write
	mov rdi, 1 ;file descriptor (stdout)
	mov rsi, rsp ;buf (stack)
	mov rdx, 1 ;len
	syscall
	
	pop rax
	pop rdi
	jmp .next
