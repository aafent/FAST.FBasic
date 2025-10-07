REM Test program for GOTO Test
REM Category: Flow Control
REM
GoTo abc
assert 0

def:
assert 1
End

abc:
assert 1

GoTo def
assert 0