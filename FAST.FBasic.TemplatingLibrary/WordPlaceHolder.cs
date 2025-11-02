using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FAST.FBasic.TemplatingLibrary
{
    internal class WordPlaceHolder
    {
        readonly string placeholderPrefix;
        readonly string placeholderSuffix;

        private Dictionary<string,int> replacementCount;
        private bool policyReplaceALL = false;
        private bool policyReplaceFIRST = false;


        public WordPlaceHolder(string placeholderPrefix, string placeholderSuffix)
        {
            this.placeholderPrefix= placeholderPrefix;
            this.placeholderSuffix= placeholderSuffix;  
        }
        public WordPlaceHolder():this("","")
        {

        }


        public string ExtractAllText(Stream inputStream)
        {
            using (var wordDoc = WordprocessingDocument.Open(inputStream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                return body.InnerText;
            }
        }


        /// <summary>
        /// Replaces placeholders in a Word document with their corresponding values.
        /// </summary>
        /// <param name="documentPath">Path to the Word document</param>
        /// <param name="placeholders">Dictionary of placeholder keys (without prefix/suffix) and replacement values</param>
        /// <param name="phPrefix">Placeholder prefix (e.g., "{{")</param>
        /// <param name="phSuffix">Placeholder suffix (e.g., "}}")</param>
        public void ReplacePlaceholders(
            Stream wordStream,
            Dictionary<string, string> placeholders,
            string policy
            )
        {            
            policy=policy?.ToUpper()!;
            if (policy.Contains("ALL")) policyReplaceALL = true;
            if (policy.Contains("FIRST")) policyReplaceFIRST = true;
            replacementCount=new();

            wordStream.Seek(0, SeekOrigin.Begin);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(wordStream, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Process all paragraphs in the document
                foreach (var paragraph in body.Descendants<Paragraph>().ToList())
                {
                    ReplacePlaceholdersInParagraph(paragraph, placeholders);
                }

                // Also process headers and footers
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    foreach (var paragraph in headerPart.Header.Descendants<Paragraph>().ToList())
                    {
                        ReplacePlaceholdersInParagraph(paragraph, placeholders);
                    }
                }

                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                {
                    foreach (var paragraph in footerPart.Footer.Descendants<Paragraph>().ToList())
                    {
                        ReplacePlaceholdersInParagraph(paragraph, placeholders);
                    }
                }

                doc.Save();
            }

            wordStream.Position = 0;
            return;
        }

        private void ReplacePlaceholdersInParagraph(
            Paragraph paragraph,
            Dictionary<string, string> placeholders
            )
        {
            // Build full placeholder strings
            var fullPlaceholders = placeholders.ToDictionary(
                kvp => placeholderPrefix + kvp.Key + placeholderSuffix,
                kvp => kvp.Value
            );

            // Get all runs with text
            var runs = paragraph.Elements<Run>().ToList();
            if (!runs.Any()) return;

            // Build a list of run-text pairs with their cumulative positions
            var runTextMap = new List<RunTextInfo>();
            int cumulativePos = 0;

            foreach (var run in runs)
            {
                foreach (var textElement in run.Elements<Text>())
                {
                    runTextMap.Add(new RunTextInfo
                    {
                        Run = run,
                        TextElement = textElement,
                        StartPosition = cumulativePos,
                        Text = textElement.Text ?? string.Empty
                    });
                    cumulativePos += textElement.Text?.Length ?? 0;
                }
            }

            if (!runTextMap.Any()) return;

            // Concatenate all text
            string fullText = string.Concat(runTextMap.Select(rt => rt.Text));

            // Find all placeholder occurrences
            var replacementsToDo = FindAllPlaceholders(fullText, fullPlaceholders);
            if (!replacementsToDo.Any()) return;

            // Process replacements from end to start to maintain positions
            foreach (var replacement in replacementsToDo.OrderByDescending(r => r.StartIndex))
            {
                ApplyReplacementOptimized(paragraph, runTextMap, replacement, fullText);
            }
        }

        private List<ReplacementInfo> FindAllPlaceholders(
            string text,
            Dictionary<string, string> fullPlaceholders)
        {
            var replacements = new List<ReplacementInfo>();

            foreach (var placeholder in fullPlaceholders)
            {
                int index = 0;
                while ((index = text.IndexOf(placeholder.Key, index, StringComparison.Ordinal)) >= 0)
                {
                    bool addThisReplacement = false;
                    if (policyReplaceALL) addThisReplacement=true;

                    if (policyReplaceFIRST)
                    {
                        addThisReplacement = !(replacementCount.ContainsKey(placeholder.Key)); 
                    }

                    if (addThisReplacement)
                    {
                        if (!replacementCount.ContainsKey(placeholder.Key))
                            replacementCount.Add(placeholder.Key,1);
                        else
                            replacementCount[placeholder.Key]++;

                        replacements.Add(new ReplacementInfo
                        {
                            PlaceHolder = placeholder.Key,
                            StartIndex = index,
                            EndIndex = index + placeholder.Key.Length - 1,
                            ReplacementText = placeholder.Value
                        });
                    }
                    index += placeholder.Key.Length;
                }
            }

            return replacements;
        }

        private void ApplyReplacementOptimized(
            Paragraph paragraph,
            List<RunTextInfo> runTextMap,
            ReplacementInfo replacement,
            string originalFullText)
        {
            int startPos = replacement.StartIndex;
            int endPos = replacement.EndIndex;

            // Find which run-text elements are affected
            var affectedItems = runTextMap
                .Where(rt => !(rt.StartPosition + rt.Text.Length - 1 < startPos || rt.StartPosition > endPos))
                .ToList();

            if (!affectedItems.Any()) return;

            var firstItem = affectedItems.First();
            var lastItem = affectedItems.Last();

            // Calculate positions within first and last text elements
            int startOffsetInFirst = startPos - firstItem.StartPosition;
            int endOffsetInLast = endPos - lastItem.StartPosition;

            // Build the new text for the first element
            string beforePlaceholder = firstItem.Text.Substring(0, startOffsetInFirst);
            string afterPlaceholder = lastItem.Text.Substring(endOffsetInLast + 1);
            string newText = beforePlaceholder + replacement.ReplacementText + afterPlaceholder;

            // Update the first text element
            firstItem.TextElement.Text = newText;
            if (newText.StartsWith(" ") || newText.EndsWith(" ") || newText.Contains("  "))
            {
                firstItem.TextElement.Space = SpaceProcessingModeValues.Preserve;
            }

            // If the placeholder spans multiple text elements, clean up the others
            if (affectedItems.Count > 1)
            {
                // Remove all other affected text elements
                for (int i = 1; i < affectedItems.Count; i++)
                {
                    affectedItems[i].TextElement.Remove();
                }

                // Remove empty runs
                var runsToCheck = affectedItems.Select(item => item.Run).Distinct().ToList();
                foreach (var run in runsToCheck)
                {
                    if (!run.Elements<Text>().Any() ||
                        run.Elements<Text>().All(t => string.IsNullOrEmpty(t.Text)))
                    {
                        run.Remove();
                    }
                }
            }

            // Update the map for subsequent replacements
            firstItem.Text = newText;
            for (int i = 1; i < affectedItems.Count; i++)
            {
                runTextMap.Remove(affectedItems[i]);
            }

            // Recalculate positions for items after the replacement
            int positionAdjustment = newText.Length - (endPos - startPos + 1);
            foreach (var item in runTextMap.Where(rt => rt.StartPosition > endPos))
            {
                item.StartPosition += positionAdjustment;
            }
        }

        private class RunTextInfo
        {
            public Run Run { get; set; }
            public Text TextElement { get; set; }
            public int StartPosition { get; set; }
            public string Text { get; set; }
        }

        private class ReplacementInfo
        {
            public string PlaceHolder { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public string ReplacementText { get; set; }
        }

        public void AddPageBreak(Stream documentStream)
        {
            // Ensure stream is at the beginning
            documentStream.Position = 0;

            using (WordprocessingDocument doc = WordprocessingDocument.Open(documentStream, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Add a page break
                body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                doc.MainDocumentPart.Document.Save();
            }

            // Reset position for further use
            documentStream.Position = 0;
        }


        public Stream CreateEmptyWordDocument()
        {
            // Create a new memory stream to hold the Word document
            var memoryStream = new MemoryStream();

            // Create a WordprocessingDocument inside the memory stream
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                // Add the main document part
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                // Create an empty document with a body element
                mainPart.Document = new Document(new Body());
                mainPart.Document.Save();
            }

            // Reset stream position before returning
            memoryStream.Position = 0;
            return memoryStream;
        }

    }



}

