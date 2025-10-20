REM Test program for Arrays
REM

coldim arr1, c1, c2
let [arr1.c1(1)]="1,1"
let [arr1.c2(1)]="1,2"

let [arr1.c1(2)]="2,1"
let [arr1.c2(2)]="2,2"

let [arr1.c1(3)]="3,1"
let [arr1.c2(3)]="3,2"

let [arr1.c1(4)]="4,1"
let [arr1.c2(4)]="4,2"

foreach arr1
   print [arr1.c1]+"  "+EOD("arr1")
endforeach arr1
print "EOD=" +EOD("arr1")

Halt