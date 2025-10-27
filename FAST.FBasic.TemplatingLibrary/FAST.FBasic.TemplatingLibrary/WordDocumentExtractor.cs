using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace FAST.FBasicInterpreter.TemplatingLibrary
{
    internal static class WordDocumentExtractor
    {
        public static void ExtractContentBetweenMarkers(
            Stream inputStream,
            Stream outputStream,
            string beginMarker,
            string endMarker,
            string markerPrefix,
            string markerSuffix,
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

        private static TextMap BuildTextMap(Body body)
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

        private static List<OpenXmlElement> GetElementsInRange(TextMap textMap, int startPos, int endPos)
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

        private static OpenXmlElement ExtractPartialElement(OpenXmlElement element, string fullText, int startOffset, int endOffset)
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


        public static Stream ReplacePlaceholderWithDocument(
            Stream mainDocumentStream,
            Stream contentStream,
            string placeholderName,
            string placeholderPrefix,
            string placeholderSuffix,
            out bool placeholderFound)
        {
            placeholderFound = false;

            // Build full placeholder
            string fullPlaceholder = placeholderPrefix + placeholderName + placeholderSuffix;

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
                    if (mainBody == null)
                    {
                        var outputStream = new MemoryStream();
                        mainMemoryStream.CopyTo(outputStream);
                        return outputStream;
                    }

                    // Build text map
                    var textMap = BuildTextMap(mainBody);
                    string fullText = textMap.FullText;

                    // Find placeholder position
                    int placeholderIndex = fullText.IndexOf(fullPlaceholder, StringComparison.Ordinal);

                    if (placeholderIndex == -1)
                    {
                        // Placeholder not found - save as is
                        mainDoc.MainDocumentPart.Document.Save();
                        mainMemoryStream.Position = 0;
                        //mainMemoryStream.CopyTo(outputStream);
                        placeholderFound = false;
                        return mainMemoryStream;
                    }

                    placeholderFound = true;

                    int placeholderEnd = placeholderIndex + fullPlaceholder.Length;

                    // Find elements that contain the placeholder
                    var elementsToProcess = new List<TextMapElement>();
                    foreach (var mapElement in textMap.Elements)
                    {
                        if (mapElement.StartIndex < placeholderEnd && mapElement.EndIndex > placeholderIndex)
                        {
                            elementsToProcess.Add(mapElement);
                        }
                    }

                    if (elementsToProcess.Count == 0)
                    {
                        mainDoc.MainDocumentPart.Document.Save();
                        mainMemoryStream.Position = 0;
                        //mainMemoryStream.CopyTo(outputStream);
                        return mainMemoryStream;
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
                        int relativeStart = placeholderIndex - firstElement.StartIndex;
                        int relativeEnd = placeholderEnd - firstElement.StartIndex;

                        beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);
                        afterPart = ExtractPartialElement(firstElement.Element, firstElement.Text, relativeEnd, firstElement.Text.Length);
                    }
                    else
                    {
                        // Placeholder spans multiple elements
                        int relativeStart = placeholderIndex - firstElement.StartIndex;
                        beforePart = ExtractPartialElement(firstElement.Element, firstElement.Text, 0, relativeStart);

                        int relativeEnd = placeholderEnd - lastElement.StartIndex;
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
                //mainMemoryStream.CopyTo(outputStream);
                return mainMemoryStream;
            }
        }

        public static void RemoveContentBetweenMarkers(
            Stream inputStream,
            Stream outputStream,
            string beginMarker,
            string endMarker,
            string markerPrefix,
            string markerSuffix,
            out bool markersFound)
        {
            markersFound = false;

            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var body = doc.MainDocumentPart?.Document?.Body;
                    if (body == null)
                    {
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(outputStream);
                        return;
                    }

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
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(outputStream);
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
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(outputStream);
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

                memoryStream.Position = 0;
                memoryStream.CopyTo(outputStream);
            }
        }
    }
}

// Example usage:
/*
// 1. Extract content between markers
using (var inputStream = File.OpenRead("input.docx"))
using (var outputStream = File.Create("output.docx"))
{
    WordDocumentExtractor.ExtractContentBetweenMarkers(
        inputStream,
        outputStream,
        "START",
        "END",
        "{{",
        "}}",
        out bool found
    );
    Console.WriteLine($"Markers found: {found}");
}

// 2. Replace placeholder with content from another document
using (var mainStream = File.OpenRead("main.docx"))
using (var contentStream = File.OpenRead("content.docx"))
using (var outputStream = File.Create("output.docx"))
{
    WordDocumentExtractor.ReplacePlaceholderWithDocument(
        mainStream,
        outputStream,
        contentStream,
        "CONTENT",
        "{{",
        "}}",
        out bool found
    );
    Console.WriteLine($"Placeholder found: {found}");
}

// 3. Remove content between markers
using (var inputStream = File.OpenRead("input.docx"))
using (var outputStream = File.Create("output.docx"))
{
    WordDocumentExtractor.RemoveContentBetweenMarkers(
        inputStream,
        outputStream,
        "REMOVE_START",
        "REMOVE_END",
        "{{",
        "}}",
        out bool found
    );
    Console.WriteLine($"Markers found and content removed: {found}");
}
*/