;Virtual machine that emulates a subleq machine.
;Subleq code is stored in the code array from compiled.asm
BITS 32
%include "compiled.asm"

section .bss
input: 	resb 10

section .data
	db '\"You must take your opponent into a deep dark forest where 2+2=5, and the path leading out is only wide enough for one.\" ― Mikhail Tal'
	

section .text
global _start
_start: ;get input from command line arguments
	pop eax ;ignore arg count
	pop eax ;ignore program name
	pop esi ;get input string
	mov edi, input
	mov ecx, 10 ;length of input
.copy: 	mov al, [esi]
	mov [edi], al
	inc edi
	inc esi
	loop .copy

	mov edi, code ;initialize the instruction pointer
.loop: 	mov eax, [edi] ;get A
	lea eax, [eax*4] ;times 4 bytes (dword)
	mov eax, [eax+code]

	mov ebx, [edi+4] ;get B
	;check for output instruction
	cmp ebx, -1
	je .emit
	lea ebx, [ebx*4] ;times 4 bytes (dword)


	;do subleq
	sub [ebx+code], eax
	jle .jmp

	;go to next instruction
.next: 	add edi, 3*4
	jmp .loop

	;jump to C
.jmp: 	mov edi, [edi+8]
	cmp edi, -1
	je .bye

	lea edi, [edi*4]
	add edi, code
	jmp .loop

	;called when C == -1 and leq == true
.bye: 	mov eax, 1 ;sys_exit
	mov ebx, 0 ;exit code
	int 0x80

.emit: 	push eax ;write value to emit on the stack
	mov eax, 4 ;sys_write
	mov ebx, 1 ;file descriptor (stdout)
	mov ecx, esp ;buf (stack)
	mov edx, 1 ;len
	int 0x80
	
	pop eax
	jmp .next
