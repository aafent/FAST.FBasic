REM test code
let a=10 ' set the value 10 to variable a

let ss=">>>

select id,name,type from sysobjects "+"

where 1=1"

dump "LIST:"
print ss
