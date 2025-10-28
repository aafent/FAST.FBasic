using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace FAST.FBasic.TemplatingLibrary
{
    public class WordMarker
    {
        private string markerPrefix;
        private string markerSuffix;

        public WordMarker(string markerPrefix, string markerSuffix)
        {
            this.markerPrefix = markerPrefix ?? throw new ArgumentNullException(nameof(markerPrefix));
            this.markerSuffix = markerSuffix ?? throw new ArgumentNullException(nameof(markerSuffix));
        }


        public void DuplicateRowWithMarker(
            Stream inputStream,
            string markerName,
            out bool found)
        {
            found = false;
            string fullMarker = markerPrefix + markerName + markerSuffix;
            inputStream.Position = 0;

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(inputStream, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                var tables = body.Descendants<Table>();

                foreach (var table in tables)
                {
                    var rows = table.Elements<TableRow>().ToList();

                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        var cells = row.Elements<TableCell>();

                        // Check if any cell contains the marker
                        bool markerFoundInRow = false;
                        foreach (var cell in cells)
                        {
                            var cellText = cell.InnerText;
                            if (cellText.Contains(fullMarker))
                            {
                                markerFoundInRow = true;
                                break;
                            }
                        }

                        if (markerFoundInRow)
                        {
                            // Clone the row
                            var clonedRow = (TableRow)row.CloneNode(true);

                            // Insert the cloned row after the current row
                            row.InsertAfterSelf(clonedRow);

                            // Remove the marker from the original row (not the cloned one)
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                ReplaceTextInCell(cell, fullMarker, string.Empty);
                            }

                            found = true;

                            // Save changes
                            wordDoc.MainDocumentPart.Document.Save();
                            return; // Exit after first match
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Searches all tables in a Word document and removes rows containing a specific marker text.
        /// The document is modified directly within the provided MemoryStream.
        /// </summary>
        /// <param name="docStream">A MemoryStream containing the .docx file. The stream must be writable.</param>
        /// <param name="marker">The text marker to search for within table cells. The search is case-sensitive.</param>
        public void RemoveRowsByMarker(Stream docStream, string marker, out bool found)
        {
            found = false;
            docStream.Position = 0;

            marker = markerPrefix + marker + markerSuffix;

            // Open the WordprocessingDocument from the stream for editing (isEditable = true)
            using (WordprocessingDocument document = WordprocessingDocument.Open(docStream, true))
            {
                MainDocumentPart? mainPart = document.MainDocumentPart;
                if (mainPart == null || mainPart.Document == null || mainPart.Document.Body == null)
                {
                    // Document is empty or malformed
                    return;
                }

                // Get all tables in the main document body.
                // We use .ToList() to create a copy, avoiding issues if the collection is
                // modified by nested table operations (though unlikely here).
                List<Table> tables = mainPart.Document.Body.Elements<Table>().ToList();

                foreach (Table table in tables)
                {
                    // We need to identify rows to delete first, then delete them
                    // to avoid modifying the collection while iterating over it.
                    List<TableRow> rowsToDelete = new List<TableRow>();

                    // Iterate through all rows in the current table
                    foreach (TableRow row in table.Elements<TableRow>())
                    {
                        bool markerFound = false;

                        // Iterate through all cells in the current row
                        foreach (TableCell cell in row.Elements<TableCell>())
                        {
                            // cell.InnerText gets all text within the cell,
                            // even if it's split across multiple runs or paragraphs.
                            if (cell.InnerText.Contains(marker))
                            {
                                markerFound = true;
                                break; // Found marker in this row, no need to check other cells
                            }
                        }

                        if (markerFound)
                        {
                            // Mark this row for deletion
                            rowsToDelete.Add(row);
                        }
                    }

                    // Now, remove all the marked rows from this table
                    foreach (TableRow rowToRemove in rowsToDelete)
                    {
                        found = true;
                        rowToRemove.Remove();
                    }
                }

                // Save the changes to the main document part.
                // This ensures the modifications are written back to the MemoryStream.
                //mainPart.Document.Save();
            }
            docStream.Position = 0;
        }


        public void ExtractContentBetweenMarkers(
            Stream inputStream,
            Stream outputStream,
            string beginMarker,
            string endMarker,
            out bool markersFound)
        {
            markersFound = false;

            // Create a copy of the input stream for the output
            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (WordprocessingDocument inputDoc = WordprocessingDocument.Open(memoryStream, false))
                using (WordprocessingDocument outputDoc = WordprocessingDocument.Create(outputStream, inputDoc.DocumentType))
                {
                    // Clone the document structure
                    foreach (var part in inputDoc.Parts)
                    {
                        outputDoc.AddPart(part.OpenXmlPart, part.RelationshipId);
                    }

                    var inputBody = inputDoc.MainDocumentPart?.Document?.Body;
                    var outputBody = outputDoc.MainDocumentPart?.Document?.Body;

                    if (inputBody == null || outputBody == null)
                    {
                        return;
                    }

                    // Build full markers
                    string fullBeginMarker = markerPrefix + beginMarker + markerSuffix;
                    string fullEndMarker = markerPrefix + endMarker + markerSuffix;

                    // Extract text with element tracking
                    var textMap = BuildTextMap(inputBody);
                    string fullText = textMap.FullText;

                    // Find marker positions
                    int beginIndex = fullText.IndexOf(fullBeginMarker, StringComparison.Ordinal);
                    int endIndex = fullText.IndexOf(fullEndMarker, StringComparison.Ordinal);

                    if (beginIndex == -1 || endIndex == -1 || beginIndex >= endIndex)
                    {
                        // Markers not found or invalid order - clear the body
                        outputBody.RemoveAllChildren();
                        markersFound = false;
                        outputDoc.MainDocumentPart.Document.Save();
                        return;
                    }

                    markersFound = true;

                    // Calculate content range (after begin marker, before end marker)
                    int contentStart = beginIndex + fullBeginMarker.Length;
                    int contentEnd = endIndex;

                    // Find elements to keep
                    var elementsToKeep = GetElementsInRange(textMap, contentStart, contentEnd);

                    // Clear output body and add only the elements in range
                    outputBody.RemoveAllChildren();

                    foreach (var element in elementsToKeep)
                    {
                        outputBody.AppendChild(element.CloneNode(true));
                    }

                    outputDoc.MainDocumentPart.Document.Save();
                }
            }
        }


        public void RemoveContentBetweenMarkers(
                    Stream inputStream,
                    string beginMarker,
                    string endMarker,
                    out bool markersFound)
        {
            markersFound = false;

            using (WordprocessingDocument doc = WordprocessingDocument.Open(inputStream, true))
            {
                var body = doc.MainDocumentPart?.Document?.Body;

                // Build full markers
                string fullBeginMarker = markerPrefix + beginMarker + markerSuffix;
                string fullEndMarker = markerPrefix + endMarker + markerSuffix;

                // Build text map
                var textMap = BuildTextMap(body);
                string fullText = textMap.FullText;

                // Find marker positions
                int beginIndex = fullText.IndexOf(fullBeginMarker, StringComparison.Ordinal);
                int endIndex = fullText.IndexOf(fullEndMarker, StringComparison.Ordinal);

                if (beginIndex == -1 || endIndex == -1 || beginIndex >= endIndex)
                {
                    // Markers not found or invalid order - save as is
                    doc.MainDocumentPart.Document.Save();
                    markersFound = false;
                    return;
                }

                markersFound = true;

                // Range to remove (including markers)
                int removeStart = beginIndex;
                int removeEnd = endIndex + fullEndMarker.Length;

                // Find elements that overlap with the range to remove
                var elementsToProcess = new List<TextMapElement>();
                foreach (var mapElement in textMap.Elements)
                {
                    if (mapElement.StartIndex < removeEnd && mapElement.EndIndex > removeStart)
                    {
                        elementsToProcess.Add(mapElement);
                    }
                }

                if (elementsToProcess.Count == 0)
                {
                    doc.MainDocumentPart.Document.Save();
                    return;
                }

                var firstElement = elementsToProcess.First();
                var lastElement = elementsToProcess.Last();

                // Create before and after parts
                OpenXmlElement beforePart = null;
                OpenXmlElement afterPart = null;

                if (firstElement == lastElement)
                {
                    // Content to remove is within a single element
                    int relativeStart = removeStart - firstElement.StartIndex;
                    int relativeEnd = removeEnd - firstElement.StartIndex;

                    beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);
                    afterPart = ExtractPartialElement(firstElement.Element, firstElement.Text, relativeEnd, firstElement.Text.Length);
                }
                else
                {
                    // Content spans multiple elements
                    int relativeStart = removeStart - firstElement.StartIndex;
                    beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);

                    int relativeEnd = removeEnd - lastElement.StartIndex;
                    afterPart = ExtractPartialElement(lastElement.Element, lastElement.Text, relativeEnd, lastElement.Text.Length);
                }

                // Find insertion point
                var insertionPoint = firstElement.Element;
                var parent = insertionPoint.Parent;

                // Remove all elements that contained the content
                foreach (var elementToRemove in elementsToProcess)
                {
                    elementToRemove.Element.Remove();
                }

                // Insert remaining content
                var insertAfter = parent.Elements().TakeWhile(e => e != insertionPoint).LastOrDefault();

                if (beforePart != null && beforePart.HasChildren)
                {
                    if (insertAfter == null)
                        parent.InsertAt(beforePart, 0);
                    else
                        insertAfter = parent.InsertAfter(beforePart, insertAfter);
                }

                if (afterPart != null && afterPart.HasChildren)
                {
                    if (insertAfter == null)
                        parent.InsertAt(afterPart, 0);
                    else
                        parent.InsertAfter(afterPart, insertAfter);
                }

                doc.MainDocumentPart.Document.Save();
            }

            inputStream.Position = 0;
            return;
        }



        public void ReplaceMarkerWithDocument(
                    Stream mainDocumentStream,
                    Stream contentStream,
                    string marker,
                    out bool found)
        {
            found = false;

            // Build full placeholder
            string fullMarker = markerPrefix + marker + markerSuffix;

            using (var mainMemoryStream = new MemoryStream())
            using (var contentMemoryStream = new MemoryStream())
            {
                mainDocumentStream.CopyTo(mainMemoryStream);
                mainMemoryStream.Position = 0;

                contentStream.CopyTo(contentMemoryStream);
                contentMemoryStream.Position = 0;

                // Load content document to extract its body elements
                List<OpenXmlElement> contentElements;
                using (WordprocessingDocument contentDoc = WordprocessingDocument.Open(contentMemoryStream, false))
                {
                    var contentBody = contentDoc.MainDocumentPart?.Document?.Body;
                    if (contentBody == null)
                    {
                        contentElements = new List<OpenXmlElement>();
                    }
                    else
                    {
                        contentElements = contentBody.Elements().Select(e => e.CloneNode(true)).ToList();
                    }
                }

                contentMemoryStream.Position = 0;

                using (WordprocessingDocument mainDoc = WordprocessingDocument.Open(mainMemoryStream, true))
                {
                    var mainBody = mainDoc.MainDocumentPart?.Document?.Body;

                    // Build text map
                    var textMap = BuildTextMap(mainBody);
                    string fullText = textMap.FullText;

                    // Find placeholder position
                    int markerIndex = fullText.IndexOf(fullMarker, StringComparison.Ordinal);

                    if (markerIndex == -1)
                    {
                        // Placeholder not found - save as is
                        mainDoc.MainDocumentPart.Document.Save();
                        mainMemoryStream.Position = 0;
                        found = false;
                        return;
                    }

                    found = true;

                    int markerEnd = markerIndex + fullMarker.Length;

                    // Find elements that contain the placeholder
                    var elementsToProcess = new List<TextMapElement>();
                    foreach (var mapElement in textMap.Elements)
                    {
                        if (mapElement.StartIndex < markerEnd && mapElement.EndIndex > markerIndex)
                        {
                            elementsToProcess.Add(mapElement);
                        }
                    }

                    if (elementsToProcess.Count == 0)
                    {
                        mainDoc.MainDocumentPart.Document.Save();
                        mainMemoryStream.Position = 0;
                        return;
                    }

                    // Process replacement
                    var firstElement = elementsToProcess.First();
                    var lastElement = elementsToProcess.Last();

                    // Create before and after parts
                    OpenXmlElement beforePart = null;
                    OpenXmlElement afterPart = null;

                    if (firstElement == lastElement)
                    {
                        // Placeholder is within a single element
                        int relativeStart = markerIndex - firstElement.StartIndex;
                        int relativeEnd = markerEnd - firstElement.StartIndex;

                        beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);
                        afterPart = ExtractPartialElement(firstElement.Element, firstElement.Text, relativeEnd, firstElement.Text.Length);
                    }
                    else
                    {
                        // Placeholder spans multiple elements
                        int relativeStart = markerIndex - firstElement.StartIndex;
                        beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);

                        int relativeEnd = markerEnd - lastElement.StartIndex;
                        afterPart = ExtractPartialElement(lastElement.Element, lastElement.Text, relativeEnd, lastElement.Text.Length);
                    }

                    // Find insertion point
                    var insertionPoint = firstElement.Element;
                    var parent = insertionPoint.Parent;

                    // Remove all elements that contained the placeholder
                    foreach (var elementToRemove in elementsToProcess)
                    {
                        elementToRemove.Element.Remove();
                    }

                    // Insert new content at the correct position
                    var insertAfter = parent.Elements().TakeWhile(e => e != insertionPoint).LastOrDefault();

                    if (beforePart != null && beforePart.HasChildren)
                    {
                        if (insertAfter == null)
                            parent.InsertAt(beforePart, 0);
                        else
                            insertAfter = parent.InsertAfter(beforePart, insertAfter);
                    }

                    foreach (var contentElement in contentElements)
                    {
                        var cloned = contentElement.CloneNode(true);
                        if (insertAfter == null)
                            parent.InsertAt(cloned, 0);
                        else
                            insertAfter = parent.InsertAfter(cloned, insertAfter);
                    }

                    if (afterPart != null && afterPart.HasChildren)
                    {
                        if (insertAfter == null)
                            parent.InsertAt(afterPart, 0);
                        else
                            parent.InsertAfter(afterPart, insertAfter);
                    }

                    mainDoc.MainDocumentPart.Document.Save();
                }

                mainMemoryStream.Position = 0;
                return;
            }
        }



        private List<OpenXmlElement> GetElementsInRange(TextMap textMap, int startPos, int endPos)
        {
            var result = new List<OpenXmlElement>();

            foreach (var mapElement in textMap.Elements)
            {
                // Check if this element overlaps with our range
                if (mapElement.EndIndex <= startPos || mapElement.StartIndex >= endPos)
                {
                    // Element is completely outside the range
                    continue;
                }

                // Element overlaps with our range
                if (mapElement.StartIndex >= startPos && mapElement.EndIndex <= endPos)
                {
                    // Element is completely inside the range
                    result.Add(mapElement.Element);
                }
                else if (mapElement.StartIndex < startPos && mapElement.EndIndex > endPos)
                {
                    // Range is completely inside this element - need to extract part
                    var clonedElement = ExtractPartialElement(mapElement.Element, mapElement.Text,
                        startPos - mapElement.StartIndex, endPos - mapElement.StartIndex);
                    if (clonedElement != null)
                    {
                        result.Add(clonedElement);
                    }
                }
                else if (mapElement.StartIndex < startPos)
                {
                    // Element starts before range, ends inside range
                    var clonedElement = ExtractPartialElement(mapElement.Element, mapElement.Text,
                        startPos - mapElement.StartIndex, mapElement.Text.Length);
                    if (clonedElement != null)
                    {
                        result.Add(clonedElement);
                    }
                }
                else if (mapElement.EndIndex > endPos)
                {
                    // Element starts inside range, ends after range
                    var clonedElement = ExtractPartialElement(mapElement.Element, mapElement.Text,
                        0, endPos - mapElement.StartIndex);
                    if (clonedElement != null)
                    {
                        result.Add(clonedElement);
                    }
                }
            }

            return result;
        }

        private OpenXmlElement ExtractPartialElement(OpenXmlElement element, string fullText, int startOffset, int endOffset)
        {
            if (element is not Paragraph paragraph)
            {
                return null;
            }

            var newParagraph = (Paragraph)paragraph.CloneNode(false); // Clone properties only

            // Copy paragraph properties
            if (paragraph.ParagraphProperties != null)
            {
                newParagraph.ParagraphProperties = (ParagraphProperties)paragraph.ParagraphProperties.CloneNode(true);
            }

            int currentPos = 0;
            string targetText = fullText.Substring(startOffset, endOffset - startOffset);
            int targetPos = 0;

            foreach (var run in paragraph.Elements<Run>())
            {
                var newRun = (Run)run.CloneNode(false);
                if (run.RunProperties != null)
                {
                    newRun.RunProperties = (RunProperties)run.RunProperties.CloneNode(true);
                }

                bool runHasContent = false;

                foreach (var textElement in run.Elements<Text>())
                {
                    string textContent = textElement.Text;
                    int textStart = currentPos;
                    int textEnd = currentPos + textContent.Length;

                    // Check if this text overlaps with our target range
                    if (textEnd > startOffset && textStart < endOffset)
                    {
                        int extractStart = Math.Max(0, startOffset - textStart);
                        int extractEnd = Math.Min(textContent.Length, endOffset - textStart);
                        string extractedText = textContent.Substring(extractStart, extractEnd - extractStart);

                        if (!string.IsNullOrEmpty(extractedText))
                        {
                            var newText = new Text(extractedText);
                            if (textElement.Space != null)
                            {
                                newText.Space = textElement.Space;
                            }
                            newRun.AppendChild(newText);
                            runHasContent = true;
                        }
                    }

                    currentPos += textContent.Length;
                }

                if (runHasContent)
                {
                    newParagraph.AppendChild(newRun);
                }
            }

            return newParagraph.HasChildren ? newParagraph : null;
        }

        private TextMap BuildTextMap(Body body)
        {
            var textMap = new TextMap();
            var sb = new StringBuilder();
            int position = 0;

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                int paraStart = position;
                var paraText = new StringBuilder();

                foreach (var run in paragraph.Elements<Run>())
                {
                    foreach (var text in run.Elements<Text>())
                    {
                        string textContent = text.Text;
                        paraText.Append(textContent);
                        position += textContent.Length;
                    }
                }

                string paragraphText = paraText.ToString();
                sb.Append(paragraphText);

                textMap.Elements.Add(new TextMapElement
                {
                    Element = paragraph,
                    StartIndex = paraStart,
                    EndIndex = position,
                    Text = paragraphText
                });

                position = position; // Paragraphs are continuous
            }

            textMap.FullText = sb.ToString();
            return textMap;
        }



        private void ReplaceTextInCell(TableCell cell, string findText, string replaceText)
        {
            var texts = cell.Descendants<Text>();

            foreach (var text in texts)
            {
                if (text.Text.Contains(findText))
                {
                    text.Text = text.Text.Replace(findText, replaceText);
                }
            }
        }


        private class TextMap
        {
            public string FullText { get; set; } = string.Empty;
            public List<TextMapElement> Elements { get; set; } = new List<TextMapElement>();
        }

        private class TextMapElement
        {
            public OpenXmlElement Element { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public string Text { get; set; }
        }


    }
}