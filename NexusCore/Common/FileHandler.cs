using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Serilog;
using static NexusCore.Model.CommonResponseClass;

namespace NexusCore.Common
{
    public class FileHandler
    {
        protected IConfiguration config { get; set; }

        private string AccessKey;
        private string SecretKey;
        private string Bucket;
        private string Url;
        private string ServiceURL;

        private RegionEndpoint Region = RegionEndpoint.USEast1;

        public FileHandler(IConfiguration _config)
        {
            config = _config;
            AccessKey = config["AWS:AccessKey"];
            SecretKey = config["AWS:SecretKey"];
            Bucket = config["AWS:Bucket"];
            Url = config["AWS:Url"];
            ServiceURL = config["AWS:ServiceURL"];
        }

        public async Task<CommonResponse> UploadFile(IFormFile file)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {

                var credentials = new BasicAWSCredentials(AccessKey, SecretKey);

                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Region.SystemName)
                };



                using (var amazonClient = new AmazonS3Client(credentials, s3Config))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        file.CopyTo(memoryStream);
                        string filename = string.Concat(
                                            Path.GetFileNameWithoutExtension(file.FileName),
                                            "_", DateTime.UtcNow.ToString(Constants.DateTimeFormat),
                                            Path.GetExtension(file.FileName)
                                            );
                        if (!String.IsNullOrEmpty(filename))
                        {
                            filename = filename.Replace(" ", "");
                            filename = filename.Replace("-", "_");
                            filename = filename.Replace("+", "_");
                            filename = filename.Replace(",", "_");
                        }
                        string filePath = Url + Constants.TempFolder + "/" + filename;

                        var request = new PutObjectRequest
                        {
                            BucketName = Bucket,
                            Key = Constants.TempFolder + "/" + filename,
                            ContentType = file.ContentType,
                            CannedACL = S3CannedACL.Private,
                            InputStream = memoryStream,
                        };

                        PutObjectResponse putObjectResponse = await amazonClient.PutObjectAsync(request);

                        if (putObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            commonResponse.IsSucess = true;
                            commonResponse.StatusCode = StatusCodes.FileUploadSuccessfully;
                            commonResponse.Data = new { filePath };
                        }

                    }
                }

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
                Log.Error(ex, "Error in file upload");
            }
            return commonResponse;
        }

        public async Task<Stream> ReadObjectData(string folderName, string fileName)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var tempUrl = amazonClient.GetPreSignedURL(new GetPreSignedUrlRequest
                        {
                            BucketName = Bucket,
                            Key = folderName + "/" + fileName,
                            Expires = DateTime.Now.AddSeconds(120)
                        });
                        var request = new GetObjectRequest
                        {
                            BucketName = Bucket,
                            Key = folderName + "/" + fileName
                        };

                        using (var getObjectResponse = await amazonClient.GetObjectAsync(request))
                        {
                            using (var responseStream = getObjectResponse.ResponseStream)
                            {
                                var stream = new MemoryStream();
                                await responseStream.CopyToAsync(stream);
                                stream.Position = 0;
                                return stream;
                            }
                        }

                    }
                }

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when reading an object"
                        , e.Message);
                throw new Exception("Read object operation failed.", e);
            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
                Log.Error(ex, "Error in read file");
                throw new Exception("Read object operation failed.", ex);
            }
        }

        public Task<string> getPreSignedUrl(string key)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = Bucket,
                            Key = key,
                            Expires = DateTime.Now.AddSeconds(300)
                        };
                        return Task.FromResult(amazonClient.GetPreSignedURL(request));


                    }
                }

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when geneateting url"
                        , e.Message);
                throw new Exception("Read object operation failed.", e);
            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
                Log.Error(ex, "Error in generate url");
                throw new Exception("Read object operation failed.", ex);
            }
        }


        public async Task<CommonResponse> MoveFile(string filePath, string destinationDirectory, string sourceDirectory = Constants.TempFolder)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                var credentials = new BasicAWSCredentials(AccessKey, SecretKey);

                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Region.SystemName)
                };


                int sourceDirectoryIndex = filePath.IndexOf(sourceDirectory + "/") + sourceDirectory.Length + 1;
                string fileName = filePath.Substring(sourceDirectoryIndex, filePath.Length - sourceDirectoryIndex);
                using (var amazonClient = new AmazonS3Client(credentials, s3Config))
                {
                    var copyRequest = new CopyObjectRequest
                    {

                        SourceBucket = Bucket,
                        SourceKey = sourceDirectory + "/" + fileName,
                        DestinationBucket = Bucket,
                        DestinationKey = destinationDirectory + "/" + fileName,
                        CannedACL = S3CannedACL.Private
                    };

                    await amazonClient.CopyObjectAsync(copyRequest);

                    if (sourceDirectory == Constants.TempFolder)
                    {
                        await amazonClient.DeleteObjectAsync(Bucket, sourceDirectory + "/" + fileName);
                    }

                    string newfilePath = filePath.Replace(sourceDirectory, destinationDirectory);
                    commonResponse.IsSucess = true;
                    commonResponse.StatusCode = StatusCodes.FileUploadSuccessfully;
                    commonResponse.Data = newfilePath;
                }

            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
                Log.Error(ex, "Error in file move");
            }
            return commonResponse;
        }


        public async Task<CommonResponse> MoveFileWithNewFileName(string filePath, string destinationDirectory, string sourceDirectory = Constants.TempFolder, string newFileName = null)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {

                int sourceDirectoryIndex = filePath.IndexOf(sourceDirectory + "/") + sourceDirectory.Length + 1;
                string fileName = filePath.Substring(sourceDirectoryIndex, filePath.Length - sourceDirectoryIndex);

                if (string.IsNullOrEmpty(newFileName))
                {
                    newFileName = fileName;
                }

                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    var copyRequest = new CopyObjectRequest
                    {

                        SourceBucket = Bucket,
                        SourceKey = sourceDirectory + "/" + fileName,
                        DestinationBucket = Bucket,
                        DestinationKey = destinationDirectory + "/" + newFileName,
                        CannedACL = S3CannedACL.Private
                    };

                    await amazonClient.CopyObjectAsync(copyRequest);

                    if (sourceDirectory == Constants.TempFolder)
                    {
                        await amazonClient.DeleteObjectAsync(Bucket, sourceDirectory + "/" + fileName);
                    }

                    string newfilePath = filePath.Replace(sourceDirectory, destinationDirectory).Replace(fileName, newFileName);
                    commonResponse.IsSucess = true;
                    commonResponse.StatusCode = StatusCodes.FileUploadSuccessfully;
                    commonResponse.Data = newfilePath;
                }

            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
                Log.Error(ex, "Error in file move");
            }
            return commonResponse;
        }


        public async Task<CommonResponse> DeleteFile(List<string> fileNames)
        {
            CommonResponse commonResponse = new CommonResponse();
            List<KeyVersion> listKeys = new List<KeyVersion>();
            try
            {
                listKeys = fileNames?.Select(x => new KeyVersion { Key = x.Replace(Url, "") }).ToList();

                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    var deleteObj = new DeleteObjectsRequest
                    {
                        BucketName = Bucket,
                        Objects = listKeys,
                    };

                    DeleteObjectsResponse deleteObject = await amazonClient.DeleteObjectsAsync(deleteObj);

                    if (deleteObject.HttpStatusCode == System.Net.HttpStatusCode.OK || deleteObject.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        commonResponse.IsSucess = true;
                        commonResponse.StatusCode = StatusCodes.FileDeletedSuccessfully;
                    }
                    else
                    {
                        commonResponse.IsSucess = false;
                        commonResponse.StatusCode = Common.StatusCodes.ErrorHasOccuredInFileDelete;
                    }

                }

            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileDelete;
                Log.Error(ex, "Error in file delete");
            }
            return commonResponse;
        }

        public async Task<CommonResponse> DeleteFile(string fileName)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    var deleteObject = await amazonClient.DeleteObjectAsync(Bucket, fileName.Replace(Url, ""));

                    if (deleteObject.HttpStatusCode == System.Net.HttpStatusCode.OK || deleteObject.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        commonResponse.IsSucess = true;
                        commonResponse.StatusCode = StatusCodes.FileDeletedSuccessfully;
                    }
                    else
                    {
                        commonResponse.IsSucess = false;
                        commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileDelete;
                    }
                }

            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileDelete;
                Log.Error(ex, "Error in file delete");
            }
            return commonResponse;
        }

        public async Task<CommonResponse> ListingObjectsAsync(string directory = Constants.TempFolder, Func<S3Object, bool> predicate = null)
        {
            CommonResponse commonResponse = new CommonResponse();
            List<string> listStr = new List<string>();

            try
            {
                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    ListObjectsV2Request request = new ListObjectsV2Request
                    {
                        BucketName = Bucket,
                        Prefix = directory
                    };

                    ListObjectsV2Response response;
                    do
                    {
                        response = await amazonClient.ListObjectsV2Async(request);

                        IEnumerable<S3Object> objects = response.S3Objects
                            .Where(x => x.Key != directory + "/");

                        // ✅ APPLY PREDICATE SAFELY
                        if (predicate != null)
                        {
                            objects = objects.Where(x => predicate(x) == true);
                        }

                        foreach (var entry in objects)
                        {
                            listStr.Add(entry.Key);
                        }

                        request.ContinuationToken = response.NextContinuationToken;

                    } while (response.IsTruncated == true);
                }

                commonResponse.Data = listStr;
                commonResponse.IsSucess = true;
                commonResponse.StatusCode = StatusCodes.RecordFetchedSuccessfully;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while listing objects");
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileUpload;
            }

            return commonResponse;
        }


        public async Task<string> GetPreSignedDocUrl(string url)
        {
            Uri panUri;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out panUri)
                && (panUri.Scheme == Uri.UriSchemeHttp || panUri.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                string key = panUri.LocalPath[0] == '/' ? panUri.LocalPath.Substring(1, panUri.LocalPath.Length - 1) : "";
                if (!String.IsNullOrEmpty(key))
                {
                    return await Task.Run(() => getPreSignedUrl(key));
                }
                else
                {
                    return url;
                }
            }
            else
            {
                return url;
            }
        }

        public async Task<CommonResponse> DeleteFileByBucket(string fileName, string Bucket = Constants.TempFolder)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                using (var amazonClient = new AmazonS3Client(AccessKey, SecretKey, Region))
                {
                    var deleteObject = await amazonClient.DeleteObjectAsync(Bucket, fileName.Replace(Url, ""));

                    if (deleteObject.HttpStatusCode == System.Net.HttpStatusCode.OK || deleteObject.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        commonResponse.IsSucess = true;
                        commonResponse.StatusCode = StatusCodes.FileDeletedSuccessfully;
                    }
                    else
                    {
                        commonResponse.IsSucess = false;
                        commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileDelete;
                    }
                }

            }
            catch (System.Exception ex)
            {
                commonResponse.IsSucess = false;
                commonResponse.StatusCode = StatusCodes.ErrorHasOccuredInFileDelete;
                Log.Error(ex, "Error in file delete");
            }
            return commonResponse;
        }
        }
    }

