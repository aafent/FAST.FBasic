REM Word Templating - example4 
REM Demonstration of Markdown code
REM 
rem (v) Declare and prepare 
FILESTREAM Templates, INMEMORY, "", "other", "StandardText.docx"   
FILESTREAM dst, out, "", "Output", "example4.docx"
WORDEMPTYDOC NewDoc

WORDFROMMARKDOWN "# Mixed Word document with markdown", NewDoc

WORDEXTRACTPART Templates, part, "MarkdownExampleBegin", "MarkdownExampleEnd"
WORDAPPEND part, NewDoc

let md="This is **bold** and this is *italic*.
## Features
- Bullet points
- Multiple levels

1. Numbered lists
2. Also work

> This is a quote
---
| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |
"
WORDFROMMARKDOWN md, NewDoc
scopy NewDoc, dst

Halt
