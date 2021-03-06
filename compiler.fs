warnings off
next-arg 2constant ARCH \ given by the Makefile. 0 0 if none given
: n" POSTPONE ." POSTPONE cr ; immediate
: | ; \ used for code division/making code easier to read
: label 0 [compile] value ;

\ subleq assembler, outputs NASM format data
variable current-addr | 0 current-addr !
: $ current-addr @ ;  | : $+ $ + ; | : .$ ." 	;addr: " $ . cr ;

ARCH s" AMD64" compare [if] s" dd" [else] s" dq" [then]
2>r : data [ 2r> ] 2literal type ;
9 constant tab
: data, tab emit data space . cr | $ 1+  current-addr ! ;

: subleq, ( A B C --) .$ | swap rot | 3 0 DO data, LOOP | cr ;


label Z \ cell to be used as a zero
label DSP \ data stack pointer 

: nxt 	3 $+ ; \ next instruction

: init-code
	n" global code"
	n" section .data"
	." code:" ;
init-code

label DAT label INC label END
: handle-literal ( c-addr u --)
	s>number? 0= if ." Word not found and not a literal." cr bye then
	drop >r
	21 $+ to DAT | DAT 1+ to INC | INC 1+ to END
	\ zero out data stack before pushing
	DSP  	| 6 $+ 	| nxt	subleq,
	DSP  	| nxt 1+ | nxt 	subleq,
	0 	| 0 	| nxt 	subleq,
	\ write to data stack
	DSP  	| nxt 1+ | nxt 	subleq,
	DAT  	| 0 	| nxt 	subleq,
	\ increment dsp
	INC  	| DSP 	| nxt 	subleq,

	0 	| 0 	| END 	subleq,
	r> negate data, \ DAT
	1 data, ; \ INC

label DEC label END
: comp-emit
	12 $+ to DEC | DEC 1+ to END
	\ decrement stack
	DEC  	| DSP 	| nxt 	subleq,
	\ get DSP
	DSP  	| nxt	| nxt 	subleq,
	\ write to -1 to emit
	0 	| -1 	| nxt 	subleq, 

	0 	| 0 	| END 	subleq,
	-1 data, ; \ DEC

label DEC label INC label END
: comp--
	18 $+ to DEC | DEC 1+ to INC | INC 1+ to END
	\ decrement stack
	DEC  	| DSP 	| nxt  	subleq,
	\ get first arg address
	DSP  	| 9 $+ 	| nxt  	subleq,
	\ get second arg address
	DSP  	| 7 $+ 	| nxt 	subleq,
	\ decrement second arg address
	INC  	| 4 $+ 	| nxt 	subleq,
	\ do subtraction
	0 	| 0 	| nxt 	subleq,
	
	0 	| 0 	| END 	subleq,
	-1 data, \ DEC
	1 data, ; \ INC

label DEC label INC label T1 label T2 label END
: comp-swap
	57 $+ to DEC | DEC 1+ to INC | INC 1+ to T1 | T1 1+ to T2 | T2 1+ to END
	\ decrement stack
	DEC  	| DSP 	| nxt  	subleq,

	\ get cell value 
	DSP  	| nxt 	| nxt 	subleq,
	0 	| T1  	| nxt 	subleq,

	\ zero out cell to prepare for swap
	DSP  	| 7 $+ 	| nxt 	subleq,
	DSP  	| nxt 	| nxt 	subleq,
	0 	| 0 	| nxt 	subleq,


	\ decrement stack
	DEC  	| DSP 	| nxt  	subleq,
	
	\ get cell value 
	DSP  	| nxt 	| nxt 	subleq,
	0 	| T2  	| nxt 	subleq,

	\ zero out cell to prepare for swap
	DSP  	| 7 $+ 	| nxt 	subleq,
	DSP  	| nxt 	| nxt 	subleq,
	0 	| 0 	| nxt 	subleq,

	\ write new value
	DSP  	| nxt 1+ | nxt 	subleq,
	T1  	| 0 	| nxt 	subleq,

	\ increment stack
	INC  	| DSP  | nxt  	subleq,

	\ write new value
	DSP  	| nxt 1+ | nxt 	subleq,
	T2  	| 0 	| nxt 	subleq,

	\ increment stack
	INC  	| DSP 	| nxt  	subleq,

	
	0 	| 0 	| END 	subleq,
	-1 data, \ DEC
	1 data, \ INC
	0 data, \ T1
	0 data, \ T2
	;

: comp-negate
	s" 0" handle-literal | comp-swap comp-- ;


: comp 	( c-addr u --) \ find word and execute compile function
	2dup   C" comp-" PAD 6 chars cmove
	dup 5 chars +   PAD c!
	PAD 6 chars +   swap cmove
	PAD find
	0 <> if execute 2drop
	else drop handle-literal then ;


: comp-bye 0 | 0 | -1 subleq, ;

: comp-test
	\ 6 $+  | 7 $+  | $ 	subleq,
	\ 0 	| 0 	| -1  	subleq,
	\ -1 | -100000000 | 0 	subleq, 
	 9 $+ 	| -1   	| 0     subleq,
	 3 $+ 	| 4 $+ 	| -3 $+ subleq,
	 1    	| 0    	| 0     subleq,
	'A    	| 0    	| 0     subleq, ;

label L label U label H label END
: comp-hello $ to L | 15 $+ to U | 16 $+ to H | 23 $+ to END
	H  	| -1 	| 0  	subleq, \ L
	U  	| L  	| 0  	subleq,
	U  	| 4 $+  | 0  	subleq,
	Z  	| H  	| END 	subleq,
	Z  	| Z  	| L  	subleq,
	-1 data, \ U
	'h data, 'e data, 'l data, 'l data, 'o data, 10 data, 0 data, ; \ H

: read 	( -- c-addr u) POSTPONE parse-name ; immediate
: end? 	( u -- u) dup 0 = if bye then ;
: comment ( c-addr u -- c-addr u) ." 	;;" | 2dup type | cr ;
: main 	$ to Z 	0 | 0 | 3 33 + subleq, \ init Z and jump to compiled code
	." 	times 32 " data n"  0" | 32 $+ current-addr ! \ allocate dstack
	$ to DSP | -32 $+ negate data, \ allocate and init data stack pointer
	begin read end? comment comp again ;

\ read in the file to compile
8192 constant max-filesize
max-filesize allocate throw 			| constant filebuf
s" obs.fs" r/o open-file throw 			| constant fileid
filebuf max-filesize fileid read-file throw  	| constant filesize
\ compile it
filebuf filesize | ' main | execute-parsing bye
