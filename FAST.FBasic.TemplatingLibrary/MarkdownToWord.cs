using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

/// <summary>
/// Provides a utility method to convert a simple Markdown text stream into a Word (.docx) document stream.
/// This version addresses the compiler error for 'ParagraphStyle' by using 'Style', which may be required
/// in some environments, and uses the confirmed type 'SpaceProcessingModeValues'.
/// </summary>
public static class MarkdownToWordConverter
{
    // The OpenXML Document Model (OXML) is verbose. This custom method focuses on converting basic
    // block-level elements: H1, H2, standard paragraphs, and tables.

    /// <summary>
    /// Converts an input stream containing Markdown text into a Word Document stored in a MemoryStream.
    /// </summary>
    /// <param name="markdownStream">The input stream containing Markdown formatted text.</param>
    /// <returns>A MemoryStream containing the generated Word (.docx) document.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input stream is null.</exception>
    public static MemoryStream ConvertMarkdownToWord(Stream markdownStream)
    {
        if (markdownStream == null)
        {
            throw new ArgumentNullException(nameof(markdownStream), "Markdown input stream cannot be null.");
        }

        // 1. Read the Markdown content
        string markdownContent;
        using (var reader = new StreamReader(markdownStream, Encoding.UTF8))
        {
            markdownContent = reader.ReadToEnd();
        }

        // Split content into lines, removing empty entries initially
        var lines = markdownContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 2. Initialize the OpenXML document in memory
        var memoryStream = new MemoryStream();

        // WordprocessingDocumentType.Document creates a standard DOCX structure
        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
        {
            // Add the main document part
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // 3. Process each line of Markdown using an index-based loop for multi-line elements (like tables)
            for (int i = 0; i < lines.Length; i++)
            {
                var trimmedLine = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                OpenXmlElement elementToAdd = null;

                // --- Table Detection (Requires Header + Separator) ---
                if (IsPotentialHeader(trimmedLine) && i + 1 < lines.Length && IsSeparatorRow(lines[i + 1]))
                {
                    // Found a table structure (Header + Separator). Parse the whole block.
                    int nextIndex;
                    elementToAdd = CreateTable(lines, i, out nextIndex);
                    i = nextIndex - 1; // Set index to the line before the next element (loop will increment it)
                }
                // -----------------------------------------------------
                else if (trimmedLine.StartsWith("# "))
                {
                    // H1 Heading
                    elementToAdd = CreateParagraphWithStyle("Heading1", trimmedLine.Substring(2));
                }
                else if (trimmedLine.StartsWith("## "))
                {
                    // H2 Heading
                    elementToAdd = CreateParagraphWithStyle("Heading2", trimmedLine.Substring(3));
                }
                else
                {
                    // Standard Paragraph
                    elementToAdd = CreateParagraphWithText(trimmedLine);
                }

                if (elementToAdd != null)
                {
                    body.AppendChild(elementToAdd);
                }
            }

            // 4. Save and close the document
            mainPart.Document.Save();
        } // The 'using' statement closes the document and flushes content to the MemoryStream

        // 5. Reset stream position and return
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// Helper to determine if a line could be a table header.
    /// </summary>
    private static bool IsPotentialHeader(string line)
    {
        var trimmed = line.Trim();
        // Must start and end with '|' and contain at least one other pipe to separate columns
        return trimmed.StartsWith("|") && trimmed.EndsWith("|") && trimmed.IndexOf('|', 1) > 0;
    }

    /// <summary>
    /// Helper to determine if a line is a table separator row (e.g., |---|---|).
    /// </summary>
    private static bool IsSeparatorRow(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("|") || !trimmed.EndsWith("|")) return false;

        // A separator row should consist mostly of hyphens and pipe characters
        return trimmed.Split('|', StringSplitOptions.RemoveEmptyEntries)
            // Checks that each column separator part only contains hyphens or colons (for alignment flags, which we ignore but validate)
            .All(col => col.Trim().All(c => c == '-' || c == ':'));
    }

    /// <summary>
    /// Parses a block of Markdown lines into an OpenXML Table element.
    /// </summary>
    private static Table CreateTable(string[] lines, int startIndex, out int endIndex)
    {
        var table = new Table();

        // 1. Table Properties (Mandatory for formatting, including borders)
        TableProperties tableProps = new TableProperties(
            new TableBorders(
                new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
            ),
            // Set table width to 100% (using 5000 Dxa units for percentage)
            new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }
        );
        table.AppendChild(tableProps);

        // 2. Parse all rows (Header, Separator, Data)
        var rowLines = new System.Collections.Generic.List<string>();
        int i = startIndex;

        // Collect Header (i) and Separator (i+1)
        if (i < lines.Length) rowLines.Add(lines[i]);
        i++;
        if (i < lines.Length && IsSeparatorRow(lines[i])) rowLines.Add(lines[i]);
        i++;

        // Collect Data rows until an empty line or end of document, or a line not starting with '|'
        for (; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrEmpty(trimmed) || !trimmed.StartsWith("|"))
            {
                break; // End of table block
            }
            rowLines.Add(lines[i]);
        }
        endIndex = i; // The index of the line after the table block

        // 3. Convert parsed rows to OpenXML TableRows (skipping the separator row [1])
        for (int rowIndex = 0; rowIndex < rowLines.Count; rowIndex++)
        {
            if (rowIndex == 1) continue; // Skip the separator row (index 1)

            bool isHeader = (rowIndex == 0);
            TableRow tableRow = new TableRow();

            // Split line by '|' and skip the first and last empty elements caused by leading/trailing pipes
            var cells = rowLines[rowIndex].Split('|', StringSplitOptions.None)
                .Skip(1)
                .SkipLast(1)
                .Select(c => c.Trim())
                .ToArray();

            foreach (var cellText in cells)
            {
                // Create cell with content and styling
                TableCell tableCell = CreateTableCell(cellText, isHeader);
                tableRow.AppendChild(tableCell);
            }

            table.AppendChild(tableRow);
        }

        return table;
    }

    /// <summary>
    /// Helper to create a TableCell with content, styling the header row differently.
    /// </summary>
    private static TableCell CreateTableCell(string content, bool isHeader)
    {
        TableCell tableCell = new TableCell();

        // 1. Cell Properties
        TableCellProperties cellProps = new TableCellProperties();

        // Add light gray shading for header cells
        if (isHeader)
        {
            cellProps.Append(new Shading()
            {
                Val = ShadingPatternValues.Clear,
                Fill = "D9D9D9" // Light gray color
            });
        }
        tableCell.Append(cellProps);

        // 2. Cell Content (must be wrapped in a Paragraph)
        Paragraph paragraph = new Paragraph();

        Run run = new Run();

        // Make header text bold
        if (isHeader)
        {
            run.Append(new RunProperties(new Bold()));
        }

        // Using fully qualified name for SpaceProcessingModeValues
        run.Append(new Text(content) { Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);

        tableCell.Append(paragraph);
        return tableCell;
    }

    /// <summary>
    /// Helper to create a paragraph with a specific Word style (e.g., Heading1).
    /// </summary>
    private static Paragraph CreateParagraphWithStyle(string styleId, string text)
    {
        var paragraph = new Paragraph(
            new ParagraphProperties(
                // *** FIX: Changed DocumentFormat.OpenXml.Wordprocessing.ParagraphStyle to Style 
                // to resolve the reported compiler error in your environment. ***
                new Style() { StyleId = styleId }
            ),
            new Run(
                new Text(text)
                {
                    // Using fully qualified name for SpaceProcessingModeValues
                    Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve
                }
            )
        );
        return paragraph;
    }

    /// <summary>
    /// Helper to create a standard paragraph.
    /// </summary>
    private static Paragraph CreateParagraphWithText(string text)
    {
        var paragraph = new Paragraph(
            new Run(
                new Text(text)
                {
                    // Using fully qualified name for SpaceProcessingModeValues
                    Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve
                }
            )
        );
        return paragraph;
    }
}
