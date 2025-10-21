using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FAST.FBasic.TemplatingLibrary
{
    public class WordProcessor
    {
        public MemoryStream SearchAndReplace(string inputFilePath, Dictionary<string, string> replacements)
        {
            using var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
            var outputStream = new MemoryStream();
            inputStream.CopyTo(outputStream);
            outputStream.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(outputStream, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

                foreach (var text in body.Descendants<Text>())
                {
                    foreach (var kvp in replacements)
                    {
                        if (text.Text.Contains(kvp.Key))
                        {
                            text.Text = text.Text.Replace(kvp.Key, kvp.Value);
                        }
                    }
                }

                wordDoc.MainDocumentPart.Document.Save();
            }

            outputStream.Position = 0;
            return outputStream;
        }

        public string ExtractAllText(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                return body.InnerText;
            }
        }


        private void example1()
        {
            var processor = new WordProcessor();
            var replacements = new Dictionary<string, string>
                                {
                                    { "{Name}", "Andreas" },
                                    { "{Role}", "Director" }
                                };

            using var resultStream = processor.SearchAndReplace("input.docx", replacements);
            using var fileStream = new FileStream("output.docx", FileMode.Create, FileAccess.Write);
            resultStream.CopyTo(fileStream);
        }

        private void example2()
        {
            var processor = new WordProcessor();
            var replacements = new Dictionary<string, string>
                                {
                                    { "{Name}", "Andreas" },
                                    { "{Role}", "Director" }
                                };
            using var resultStream = processor.SearchAndReplace("input.docx", replacements);
            // Return resultStream to caller, e.g., in a web API response
        }





    }
}
