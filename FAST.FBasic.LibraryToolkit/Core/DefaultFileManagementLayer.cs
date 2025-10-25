using System;
namespace FAST.FBasicInterpreter
{
    public class DefaultFileManagementLayer : IFBasicFileManagementLayer
    {
        public string RootFolder {  set; get; } =".";

        public IFBasicFileDescriptor RequestedFile { set; private get; }

        private string fullFilePath; 


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootFolder"></param>
        public DefaultFileManagementLayer(string rootFolder)
        {
            this.RootFolder=rootFolder;
        }

        public IFBasicFileManagementLayer Handler(IFBasicFileDescriptor file)
        {
            this.RequestedFile = file;
            this.buildFullFilePath();
            return this;
        }


        public string GetSourceProgram()
        {
            var program = fullFilePath;
            var ext=Path.GetExtension(program);
            if (ext.ToLower() !=  ".bas")  program+=".bas";
            return File.ReadAllText(program);
        }

        public FileStream OutputFileStream(FileShare fileShare)
        {
            return File.Create(fullFilePath);
        }

        public FileStream ReadOnlyFileStream()
        {
            return new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
        }

        private void buildFullFilePath()
        {
            string library=RequestedFile.Library;
            string path=RequestedFile.Path;
            if (string.IsNullOrEmpty(library)) library=".";
            if (string.IsNullOrEmpty(path)) path = ".";

            this.fullFilePath= Path.Combine(RootFolder,library,path, RequestedFile.FileName);
        }

    }
}
