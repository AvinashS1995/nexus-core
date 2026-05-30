using System;
using System.Data;
using NexusCore.Common;
using NexusCore.Model;
using NexusCore.Repositories.Interfaces;
using NexusCore.Repositories.Interfaces;
using static NexusCore.Model.FileModel;

namespace NexusCore.Repositories
{
    public class CommonRepository : BaseRepository<FileModel>, ICommonRepository
    {
        public CommonRepository(DbContext mySqlDatabase) : base(mySqlDatabase)
        {
        }

       

       public async Task<long> UploadFile(string UploadFileName, string UploadFilePath, string UploadFileType, long UploadFileSize, string LoginEmpno)
        {
            object result = null;

            using (var command = CreateCommand())
            {
                command.CommandText = "SPSaveUploadFile";
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "UploadFileName", UploadFileName);
                AddParameter(command, "UploadFilePath", UploadFilePath);
                AddParameter(command, "UploadFileType", UploadFileType);
                AddParameter(command, "UploadFileSize", UploadFileSize);
                AddParameter(command, "LoginEmpno", LoginEmpno);

                result = await ExecuteScalarAsync(command);
            }

            return result != null ? Convert.ToInt64(result) : -100;
        }
    }
}

