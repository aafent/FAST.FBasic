rem
AiPROVIDER manProvider, deepseek, * 
AiPROVIDER womanProvider, gemini,* 
AiSESSION man, manProvider, "Act as a 22-year-old man named Johnny. 
Your goal is to make the woman you are talking to start a social conversesion. 
For each prompt, you will generate only one reply to her."

AiSESSION woman, womanProvider, "Act as a 26-year-old woman named Anna. 
Your goal is to drive the man you are talking to a social conversesion. 
For each prompt, you will generate only one reply to him."

let times=0
let manReponse="Hi what is your name?"

loop:
let times=times+1

print "John: "+manReponse
AiPROMPT woman, womanResponse, manReponse
print "Anna: "+womanResponse

AiPROMPT man, manReponse, womanResponse
print ""

if times < 5 then goto loop

halt

