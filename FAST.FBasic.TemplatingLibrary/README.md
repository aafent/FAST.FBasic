This is a library for FBASIC

This library is depending of the package: DocumentFormat.OpenXml 






## Supported Markdown Elements:

1. **Headings** (H1-H6): `# Heading 1` through `###### Heading 6`
2. **Bold**: `**text**` or `__text__`
3. **Italic**: `*text*` or `_text*`
4. **Bold + Italic**: `***text***` or `___text___`
5. **Strikethrough**: `~~text~~`
6. **Inline Code**: `` `code` ``
7. **Blockquotes**: `> quote text`
8. **Unordered Lists**: `- item`, `* item`, or `+ item`
9. **Ordered Lists**: `1. item`
10. **Horizontal Rules**: `---`, `***`, or `___`
11. **Tables**: Standard markdown tables with `|` separators
12. **Links**: `[text](url)` (displayed as text)

## Usage Example:

```csharp
string markdown = @"
# My Document

This is **bold** and this is *italic*.

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
";

```

## Key Features:

- **Proper styling** with heading styles, quote styles, and formatting
- **Nested lists** support (indentation-based)
- **Table formatting** with borders and header row shading
- **Inline formatting** combinations (bold + italic)
- **Memory stream** return for flexibility (save to file, return as HTTP response, etc.)

The code avoids common pitfalls and uses proper OpenXML structure to ensure compatibility with Word.
