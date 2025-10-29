using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

internal static class MarkdownToWordConverter
{
    public static Stream ConvertMarkdownToWord(string markdownText)
    {
        var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Define styles
            AddStyles(mainPart);

            // Parse and convert markdown
            var lines = markdownText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int i = 0;

            while (i < lines.Length)
            {
                var line = lines[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    body.AppendChild(new Paragraph());
                    i++;
                    continue;
                }

                // Check for tables
                if (line.TrimStart().StartsWith("|"))
                {
                    i = ProcessTable(body, lines, i);
                    continue;
                }

                // Check for horizontal rule
                if (Regex.IsMatch(line.Trim(), @"^(\*{3,}|-{3,}|_{3,})$"))
                {
                    AddHorizontalLine(body);
                    i++;
                    continue;
                }

                // Check for headers
                var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (headerMatch.Success)
                {
                    var level = headerMatch.Groups[1].Value.Length;
                    var text = headerMatch.Groups[2].Value;
                    AddHeading(body, text, level);
                    i++;
                    continue;
                }

                // Check for unordered list
                var ulMatch = Regex.Match(line, @"^(\s*)[-*+]\s+(.+)$");
                if (ulMatch.Success)
                {
                    i = ProcessList(body, lines, i, false, mainPart);
                    continue;
                }

                // Check for ordered list
                var olMatch = Regex.Match(line, @"^(\s*)\d+\.\s+(.+)$");
                if (olMatch.Success)
                {
                    i = ProcessList(body, lines, i, true, mainPart);
                    continue;
                }

                // Check for blockquote
                if (line.TrimStart().StartsWith(">"))
                {
                    i = ProcessBlockquote(body, lines, i);
                    continue;
                }

                // Regular paragraph
                AddParagraph(body, line);
                i++;
            }

            mainPart.Document.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private static void AddStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        // Heading styles
        for (int i = 1; i <= 6; i++)
        {
            var headingStyle = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = $"Heading{i}",
                CustomStyle = false
            };

            headingStyle.AppendChild(new Name() { Val = $"Heading {i}" });
            headingStyle.AppendChild(new BasedOn() { Val = "Normal" });
            headingStyle.AppendChild(new NextParagraphStyle() { Val = "Normal" });

            var styleRunProperties = new StyleRunProperties();
            styleRunProperties.AppendChild(new Bold());
            styleRunProperties.AppendChild(new FontSize() { Val = (32 - i * 2).ToString() });
            headingStyle.AppendChild(styleRunProperties);

            var styleParagraphProperties = new StyleParagraphProperties();
            styleParagraphProperties.AppendChild(new SpacingBetweenLines()
            {
                Before = "240",
                After = "120"
            });
            headingStyle.AppendChild(styleParagraphProperties);

            styles.AppendChild(headingStyle);
        }

        // Quote style
        var quoteStyle = new Style()
        {
            Type = StyleValues.Paragraph,
            StyleId = "Quote",
            CustomStyle = true
        };
        quoteStyle.AppendChild(new Name() { Val = "Quote" });

        var quoteParagraphProperties = new StyleParagraphProperties();
        quoteParagraphProperties.AppendChild(new Indentation() { Left = "720" });
        var quoteBorders = new ParagraphBorders();
        quoteBorders.AppendChild(new LeftBorder()
        {
            Val = BorderValues.Single,
            Color = "CCCCCC",
            Size = 12,
            Space = 4
        });
        quoteParagraphProperties.AppendChild(quoteBorders);
        quoteStyle.AppendChild(quoteParagraphProperties);

        var quoteRunProperties = new StyleRunProperties();
        quoteRunProperties.AppendChild(new Italic());
        quoteRunProperties.AppendChild(new Color() { Val = "666666" });
        quoteStyle.AppendChild(quoteRunProperties);

        styles.AppendChild(quoteStyle);

        stylesPart.Styles = styles;
    }

    private static void AddHeading(Body body, string text, int level)
    {
        var paragraph = body.AppendChild(new Paragraph());
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.ParagraphStyleId = new ParagraphStyleId() { Val = $"Heading{level}" };
        paragraph.AppendChild(paragraphProperties);

        AddFormattedText(paragraph, text);
    }

    private static void AddParagraph(Body body, string text)
    {
        var paragraph = body.AppendChild(new Paragraph());
        AddFormattedText(paragraph, text);
    }

    private static void AddFormattedText(Paragraph paragraph, string text)
    {
        // Parse inline formatting: bold, italic, code
        var pattern = @"(\*\*\*|___|__|\*\*|\*|_|~~|`)(.*?)\1|(\[([^\]]+)\]\(([^\)]+)\))";
        var lastIndex = 0;
        var matches = Regex.Matches(text, pattern);

        if (matches.Count == 0)
        {
            // No formatting, just add the text
            paragraph.AppendChild(CreateRun(text, false, false, false, false));
            return;
        }

        foreach (Match match in matches)
        {
            // Add text before match
            if (match.Index > lastIndex)
            {
                var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                paragraph.AppendChild(CreateRun(beforeText, false, false, false, false));
            }

            if (!string.IsNullOrEmpty(match.Groups[3].Value)) // Link
            {
                var linkText = match.Groups[4].Value;
                var linkUrl = match.Groups[5].Value;
                paragraph.AppendChild(CreateRun(linkText, false, false, false, false));
            }
            else
            {
                var delimiter = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                bool isBold = delimiter == "**" || delimiter == "***" || delimiter == "__" || delimiter == "___";
                bool isItalic = delimiter == "*" || delimiter == "_" || delimiter == "***" || delimiter == "___";
                bool isStrikethrough = delimiter == "~~";
                bool isCode = delimiter == "`";

                paragraph.AppendChild(CreateRun(content, isBold, isItalic, isStrikethrough, isCode));
            }

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text
        if (lastIndex < text.Length)
        {
            var remainingText = text.Substring(lastIndex);
            paragraph.AppendChild(CreateRun(remainingText, false, false, false, false));
        }
    }

    private static Run CreateRun(string text, bool bold, bool italic, bool strikethrough, bool code)
    {
        var run = new Run();
        var runProperties = new RunProperties();

        if (bold)
            runProperties.AppendChild(new Bold());

        if (italic)
            runProperties.AppendChild(new Italic());

        if (strikethrough)
            runProperties.AppendChild(new Strike());

        if (code)
        {
            runProperties.AppendChild(new RunFonts() { Ascii = "Courier New" });
            runProperties.AppendChild(new Shading()
            {
                Val = ShadingPatternValues.Clear,
                Color = "auto",
                Fill = "F0F0F0"
            });
        }

        if (runProperties.HasChildren)
            run.AppendChild(runProperties);

        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private static int ProcessList(Body body, string[] lines, int startIndex, bool isOrdered, MainDocumentPart mainPart)
    {
        var i = startIndex;
        GetOrCreateNumbering(mainPart);
        var numId = isOrdered ? 2 : 1;

        while (i < lines.Length)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                break;
            }

            Match match;
            if (isOrdered)
                match = Regex.Match(line, @"^(\s*)\d+\.\s+(.+)$");
            else
                match = Regex.Match(line, @"^(\s*)[-*+]\s+(.+)$");

            if (!match.Success)
                break;

            var indent = match.Groups[1].Value.Length / 2;
            var text = match.Groups[2].Value;

            var paragraph = body.AppendChild(new Paragraph());
            var paragraphProperties = new ParagraphProperties();

            var numberingProperties = new NumberingProperties();
            numberingProperties.AppendChild(new NumberingLevelReference() { Val = indent });
            numberingProperties.AppendChild(new NumberingId() { Val = numId });
            paragraphProperties.AppendChild(numberingProperties);

            paragraph.AppendChild(paragraphProperties);
            AddFormattedText(paragraph, text);

            i++;
        }

        return i;
    }

    private static void GetOrCreateNumbering(MainDocumentPart mainPart)
    {
        if (mainPart.NumberingDefinitionsPart != null)
            return;

        var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
        var numbering = new Numbering();

        // Bullet list
        var abstractNum0 = new AbstractNum() { AbstractNumberId = 0 };
        for (int i = 0; i < 9; i++)
        {
            var level = new Level() { LevelIndex = i };
            level.AppendChild(new NumberingFormat() { Val = NumberFormatValues.Bullet });
            level.AppendChild(new LevelText() { Val = "•" });
            level.AppendChild(new LevelJustification() { Val = LevelJustificationValues.Left });

            var previousParagraphProperties = new PreviousParagraphProperties();
            previousParagraphProperties.AppendChild(new Indentation()
            {
                Left = (720 * (i + 1)).ToString(),
                Hanging = "360"
            });
            level.AppendChild(previousParagraphProperties);

            abstractNum0.AppendChild(level);
        }
        numbering.AppendChild(abstractNum0);

        // Numbered list
        var abstractNum1 = new AbstractNum() { AbstractNumberId = 1 };
        for (int i = 0; i < 9; i++)
        {
            var level = new Level() { LevelIndex = i };
            level.AppendChild(new StartNumberingValue() { Val = 1 });
            level.AppendChild(new NumberingFormat() { Val = NumberFormatValues.Decimal });
            level.AppendChild(new LevelText() { Val = $"%{i + 1}." });
            level.AppendChild(new LevelJustification() { Val = LevelJustificationValues.Left });

            var previousParagraphProperties = new PreviousParagraphProperties();
            previousParagraphProperties.AppendChild(new Indentation()
            {
                Left = (720 * (i + 1)).ToString(),
                Hanging = "360"
            });
            level.AppendChild(previousParagraphProperties);

            abstractNum1.AppendChild(level);
        }
        numbering.AppendChild(abstractNum1);

        // Number instances
        var numberingInstance1 = new NumberingInstance() { NumberID = 1 };
        numberingInstance1.AppendChild(new AbstractNumId() { Val = 0 });
        numbering.AppendChild(numberingInstance1);

        var numberingInstance2 = new NumberingInstance() { NumberID = 2 };
        numberingInstance2.AppendChild(new AbstractNumId() { Val = 1 });
        numbering.AppendChild(numberingInstance2);

        numberingPart.Numbering = numbering;
        numberingPart.Numbering.Save();
    }

    private static int ProcessBlockquote(Body body, string[] lines, int startIndex)
    {
        var i = startIndex;
        var quoteText = "";

        while (i < lines.Length)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                break;
            }

            if (!line.TrimStart().StartsWith(">"))
                break;

            var text = line.TrimStart().Substring(1).TrimStart();
            quoteText += (quoteText.Length > 0 ? " " : "") + text;
            i++;
        }

        var paragraph = body.AppendChild(new Paragraph());
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.ParagraphStyleId = new ParagraphStyleId() { Val = "Quote" };
        paragraph.AppendChild(paragraphProperties);

        AddFormattedText(paragraph, quoteText);

        return i;
    }

    private static void AddHorizontalLine(Body body)
    {
        var paragraph = body.AppendChild(new Paragraph());
        var paragraphProperties = new ParagraphProperties();

        var paragraphBorders = new ParagraphBorders();
        paragraphBorders.AppendChild(new BottomBorder()
        {
            Val = BorderValues.Single,
            Color = "auto",
            Size = 6,
            Space = 1
        });
        paragraphProperties.AppendChild(paragraphBorders);
        paragraph.AppendChild(paragraphProperties);
    }

    private static int ProcessTable(Body body, string[] lines, int startIndex)
    {
        var i = startIndex;
        var tableRows = new System.Collections.Generic.List<string[]>();

        // Read all table rows
        while (i < lines.Length)
        {
            var line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("|"))
                break;

            // Skip separator line
            if (Regex.IsMatch(line, @"^\|[\s\-:|\|]+\|$"))
            {
                i++;
                continue;
            }

            var cells = line.Split('|')
                .Skip(1)
                .Take(line.Split('|').Length - 2)
                .Select(c => c.Trim())
                .ToArray();

            tableRows.Add(cells);
            i++;
        }

        if (tableRows.Count == 0)
            return i;

        // Create table
        var table = new Table();

        // Table properties
        var tableProperties = new TableProperties();
        tableProperties.AppendChild(new TableBorders(
            new TopBorder() { Val = BorderValues.Single, Size = 4 },
            new BottomBorder() { Val = BorderValues.Single, Size = 4 },
            new LeftBorder() { Val = BorderValues.Single, Size = 4 },
            new RightBorder() { Val = BorderValues.Single, Size = 4 },
            new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 4 },
            new InsideVerticalBorder() { Val = BorderValues.Single, Size = 4 }
        ));
        tableProperties.AppendChild(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });
        table.AppendChild(tableProperties);

        // Add rows
        bool isHeader = true;
        foreach (var rowData in tableRows)
        {
            var tableRow = new TableRow();

            foreach (var cellText in rowData)
            {
                var tableCell = new TableCell();

                // Cell properties
                var cellProperties = new TableCellProperties();
                if (isHeader)
                {
                    cellProperties.AppendChild(new Shading()
                    {
                        Val = ShadingPatternValues.Clear,
                        Color = "auto",
                        Fill = "D0D0D0"
                    });
                }
                tableCell.AppendChild(cellProperties);

                // Cell content
                var paragraph = new Paragraph();
                var run = new Run();

                if (isHeader)
                {
                    var runProperties = new RunProperties();
                    runProperties.AppendChild(new Bold());
                    run.AppendChild(runProperties);
                }

                run.AppendChild(new Text(cellText));
                paragraph.AppendChild(run);
                tableCell.AppendChild(paragraph);

                tableRow.AppendChild(tableCell);
            }

            table.AppendChild(tableRow);
            isHeader = false;
        }

        body.AppendChild(table);
        body.AppendChild(new Paragraph()); // Add spacing after table

        return i;
    }
}