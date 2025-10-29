using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FAST.FBasic.TemplatingLibrary
{
    public class WordDocumentMerger
    {
        /// <summary>
        /// Appends the content of the second document to the first document
        /// </summary>
        /// <param name="whatToAppendDocStream">The main document stream that will receive the content</param>
        /// <param name="destinationDocumentStream">The document stream to append</param>
        public void AppendDocument(Stream whatToAppendDocStream, Stream destinationDocumentStream)
        {
            // Ensure streams are at the beginning
            whatToAppendDocStream.Position = 0;
            destinationDocumentStream.Position = 0;

            using (WordprocessingDocument mainDoc = WordprocessingDocument.Open(whatToAppendDocStream, true))
            using (WordprocessingDocument docToAppend = WordprocessingDocument.Open(destinationDocumentStream, false))
            {
                // Get the main document body
                var mainBody = mainDoc.MainDocumentPart.Document.Body;

                // Get the document to append body
                var appendBody = docToAppend.MainDocumentPart.Document.Body;

                // Import styles from the document to append
                ImportStyles(mainDoc.MainDocumentPart, docToAppend.MainDocumentPart);

                // Import numbering definitions
                ImportNumbering(mainDoc.MainDocumentPart, docToAppend.MainDocumentPart);

                // Clone and append each element from the second document
                foreach (var element in appendBody.Elements())
                {
                    var clonedElement = element.CloneNode(true);
                    mainBody.AppendChild(clonedElement);
                }

                // Process images and other relationships
                ProcessRelationships(mainDoc.MainDocumentPart, docToAppend.MainDocumentPart);

                mainDoc.MainDocumentPart.Document.Save();
            }

            // Reset position for further use
            whatToAppendDocStream.Position = 0;
        }

        private void ImportStyles(MainDocumentPart mainPart, MainDocumentPart appendPart)
        {
            if (appendPart.StyleDefinitionsPart == null)
                return;

            if (mainPart.StyleDefinitionsPart == null)
            {
                var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylesPart.Styles = new Styles();
            }

            var mainStyles = mainPart.StyleDefinitionsPart.Styles;
            var appendStyles = appendPart.StyleDefinitionsPart.Styles;

            foreach (var style in appendStyles.Elements<Style>())
            {
                var styleId = style.StyleId?.Value;
                if (styleId != null && !mainStyles.Elements<Style>().Any(s => s.StyleId == styleId))
                {
                    mainStyles.AppendChild(style.CloneNode(true));
                }
            }
        }

        private static void ImportNumbering(MainDocumentPart mainPart, MainDocumentPart appendPart)
        {
            if (appendPart.NumberingDefinitionsPart == null)
                return;

            if (mainPart.NumberingDefinitionsPart == null)
            {
                var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
                numberingPart.Numbering = new Numbering();
            }

            var mainNumbering = mainPart.NumberingDefinitionsPart.Numbering;
            var appendNumbering = appendPart.NumberingDefinitionsPart.Numbering;

            foreach (var element in appendNumbering.Elements())
            {
                mainNumbering.AppendChild(element.CloneNode(true));
            }
        }

        private static void ProcessRelationships(MainDocumentPart mainPart, MainDocumentPart appendPart)
        {
            // Handle images and other embedded parts
            foreach (var imagePart in appendPart.ImageParts)
            {
                var imageStream = imagePart.GetStream();
                var newImagePart = mainPart.AddImagePart(imagePart.ContentType);

                using (var writer = newImagePart.GetStream(FileMode.Create))
                {
                    imageStream.CopyTo(writer);
                }
            }
        }

    }

}
