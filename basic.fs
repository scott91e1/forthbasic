
13 constant bc-cr
10 constant bc-lf
27 constant bc-escape
48 constant bc-digit-lo
57 constant bc-digit-hi
0 constant bc-false
-1 constant bc-true
256 constant bc-max-line-size
128 constant bc-max-lines

variable b-window-width
variable b-window-height
variable b-window-size

: b-update-window-size ( -- )
    \ get new text window dimensions
    form b-window-width ! b-window-height ! 
    b-window-width @ b-window-height @ * b-window-size ! ;

b-update-window-size

: b-update-window-size? ( -- t )
    \ check if window dimensions have changed
    form b-window-width <> b-window-height <> or ;

: b-NEWLINE
    \ output newline character(s)
    bc-cr emit bc-lf emit ;

: b-ESC ( -- )
    \ output escape character
    bc-escape emit ;

: b-CSI ( -- )
    \ output control sequence introducer ( ESC [ )
    b-ESC ." [" ;

: b-SEMIC ( -- ) 
    \ output semicolon
    ." ;" ;

: b-outnum ( n -- )
    \ output decimal number (without surrounding blanks)
    dup 0= if 
        \ if the value is zero, only output that
        drop ." 0"
    else
        \ if the number is negative, output minus sign and negate
        dup 0< if
            ." -"
            negate
        endif
        \ divide number by 10
        dup 10 /
        \ if non-zero, recurse with that number
        dup 0<> if
            recurse
        else
            drop
        endif
        \ output least significant digit of decimal number
        10 mod bc-digit-lo + emit
    endif ;

: b-auto-update-window ( -- )
    \ check if window update is necessary, and do so if so
    b-update-window-size? if b-update-window-size then ;

: b-cursor-y-invalid? ( n -- t )
    \ check if cursor Y position is invalid
    dup 0< over b-window-height >= or nip ;

: b-cursor-x-invalid? ( n -- t )
    \ check if cursor X position is invalid
    dup 0< over b-window-width >= or nip ;

variable b-cursor-x
variable b-cursor-y

: b-set-cursor ( x y -- )
    b-cursor-y ! b-cursor-x ! ;

: b-not ( n -- ~n )
    invert ;

: b-locate ( x y -- )
    \ set cursor to specified screen position (starting from 1,1)
    b-auto-update-window
    ( x y -- x-1 y-1 )
    1- swap 1- swap
    ( x y -- x y t )
    dup b-cursor-y-invalid?
    ( x y t -- x y t t )
    2 pick b-cursor-x-invalid?
    ( x y t t -- ) 
    or b-not if 2dup 1+ swap 1+ swap b-set-cursor at-xy then ;

: b-cls ( -- )
    \ clear screen and set cursor to top left screen position
    page 1 1 b-locate ;

b-cls

." Forth BASIC v0.1 - Copyright (c) Ekkehard Morgenstern. All rights reserved."
b-NEWLINE
." Licensable under the GNU General Public License (GPL) v3 or higher."
b-NEWLINE
." Written for use with GNU Forth (aka GForth)."
b-NEWLINE
b-NEWLINE
1 5 b-locate

: b-handle-cursor-up ( -- )
    ;

: b-handle-cursor-down ( -- )
    ;

: b-handle-cursor-left ( -- )
    ;

: b-handle-cursor-right ( -- )
    ;

: b-handle-page-up ( -- )
    ;

: b-handle-page-down ( -- )
    ;

: b-handle-home ( -- )
    ;

: b-handle-end ( -- )
    ;

: b-handle-insert ( -- )
    ;

: b-handle-delete ( -- )
    ;

: b-handle-f1 ( -- )
    ;

: b-handle-f2 ( -- )
    ;

: b-handle-f3 ( -- )
    ;

: b-handle-f4 ( -- )
    ;

: b-handle-f5 ( -- )
    ;

: b-handle-f6 ( -- )
    ;

: b-handle-f7 ( -- )
    ;

: b-handle-f8 ( -- )
    ;

: b-handle-f9 ( -- )
    ;

: b-handle-f10 ( -- )
    ;

: b-handle-f11 ( -- )
    ;

: b-handle-f12 ( -- )
    ;

: b-handle-escape ( -- )
    bye ;

: b-scroll-up ( -- )
    \ not really doing anything here, console scrolls automatically
    ;

: b-anticipate-return-event ( x y -- x y )
    \ recompute anticipated cursor position as if return had been pressed
    \ reset x to 1
    swap drop 1 swap
    \ add 1 to y 
    1+ 
    \ check if window height has been exceeded
    dup b-window-height > if
        \ yes: sub 1 from y
        1-
        \ scroll up
        b-scroll-up
    endif ;

: b-anticipate-return ( -- x y )
    \ get remembered cursor position
    b-cursor-x @ b-cursor-y @
    \ recompute anticipated cursor position as if return had been pressed
    b-anticipate-return-event ;

: b-anticipate-next-char ( -- x y )
    \ get remembered cursor position
    b-cursor-x @ b-cursor-y @
    \ add 1 to x and check if window width has been exceeded
    over 1+ b-window-width > if
        \ yes: recompute anticipated cursor position as if return had been pressed
        b-anticipate-return-event
    else
        \ no: add 1 to x for real
        swap 1+ swap
    endif ;

: b-handle-return ( -- )
    \ return key has been pressed
    \ get anticipated cursor position
    b-anticipate-return ( -- x y )
    \ output newline sequence 
    b-NEWLINE
    \ locate to anticipated position
    b-locate ;


: b-input-handler ( -- )
    key? if
        ekey ekey>char if ( c ) 
            case
                13      of b-handle-return endof
                27      of b-handle-escape endof
                ( c ) \ default handling:
                dup 32 < over 126 > or if
                else
                    \ printable character: first check if window size has changed
                    b-auto-update-window
                    \ compute anticipated cursor position
                    b-anticipate-next-char ( -- x y )
                    \ output character
                    2 pick emit
                    \ locate to anticipated position
                    b-locate
                endif
            endcase

        else ekey>fkey if ( key-id )
            case
                \ cursor keys
                k-up    of b-handle-cursor-up       endof
                k-down  of b-handle-cursor-down     endof
                k-left  of b-handle-cursor-left     endof
                k-right of b-handle-cursor-right    endof
                \ pageup / pagedown
                k-prior of b-handle-page-up         endof
                k-next  of b-handle-page-down       endof
                \ home / end
                k-home  of b-handle-home            endof
                k-end   of b-handle-end             endof
                \ insert / delete
                k-insert of b-handle-insert         endof
                k-delete of b-handle-delete         endof
                \ function keys
                k-f1    of b-handle-f1              endof
                k-f2    of b-handle-f2              endof
                k-f3    of b-handle-f3              endof
                k-f4    of b-handle-f4              endof
                k-f5    of b-handle-f5              endof
                k-f6    of b-handle-f6              endof
                k-f7    of b-handle-f7              endof
                k-f8    of b-handle-f8              endof
                k-f9    of b-handle-f9              endof
                k-f10   of b-handle-f10             endof
                k-f11   of b-handle-f11             endof
                k-f12   of b-handle-f12             endof
            endcase
        else ( keyboard-event )
            drop

        then then
    then ;

: b-screen-editor ( -- )
    \ BASIC screen editor
    begin b-input-handler again ;

b-screen-editor

