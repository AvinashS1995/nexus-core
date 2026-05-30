using System;
namespace NexusCore.Model
{
	public class FileModel
	{
        public class INP_BulkDeleteFileModel
        {
            public string FileNamesWithDirectory { get; set; }
            
        }

        public class INP_SaveUploadFile
        {
            public string UploadFileName { get; set; }
            public string UploadFilePath { get; set; }
            public string UploadFileType { get; set; }
            public int UploadFileSize { get; set; }
        }
    }
}

