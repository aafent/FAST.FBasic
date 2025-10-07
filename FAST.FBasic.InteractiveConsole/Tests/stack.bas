REM test code for stack management statements and functions
REM Library: FBasicStack
REM Category: String Functions
REM
assert stackcnt() = 0 'ensure that you are starting with empty stack


push 10
print "Stack Count=" + stackcnt()

let b = "Hello"
push b
print "Stack Count=" + stackcnt()

stackpeek msg
print "First on stack=" + msg

pop c
print "c=" + c
print "Stack Count=" + stackcnt()

pop d
print "d=" + d

print "Stack Count=" + stackcnt()

assert stackcnt() = 0

push 123 'will not have any effect


Halt

