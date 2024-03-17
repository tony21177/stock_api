
using AutoMapper;
using stock_api.Models;
using stock_api.Service.ValueObject;
using Microsoft.AspNetCore.Mvc;

namespace stock_api.Service
{
    public class FileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        //private readonly IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;


        public FileUploadService(ILogger<FileUploadService> logger, IWebHostEnvironment webHostEnvironment, StockDbContext dbContext, IMapper mapper)
        {
            _logger = logger;
            //_fileProvider = fileProvider;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<FileDetail>> PostFilesAsync(List<IFormFile> files, List<string> fileFolderNames)
        {
            long size = files.Sum(f => f.Length);

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileDetailList = new List<FileDetail>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var fileName = formFile.FileName;
                    //var fileFolder = _fileProvider.GetFileInfo(uploadsFolder + "/" + string.Join("/", fileFolderNames)).PhysicalPath;
                    var fileFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads", string.Join("/", fileFolderNames));
                    var filePath = Path.Combine(fileFolder, fileName);
                    if (!Directory.Exists(fileFolder))
                    {
                        try
                        {
                            // If the folder doesn't exist, create it
                            Directory.CreateDirectory(fileFolder);
                            _logger.LogInformation("Folder created successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.StackTrace);
                            return new List<FileDetail>();
                        }
                    }
                    var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var fileDetail = new FileDetail
                    {
                        AttId = Guid.NewGuid().ToString() + "_" + nowTimestamp,
                        FileName = fileName,
                        FilePath = filePath,
                        FileType = formFile.ContentType,

                        CreatedTime = nowTimestamp,
                    };
                    using var stream = File.Create(filePath);
                    await formFile.CopyToAsync(stream);
                    fileDetail.FileSizeNumber = stream.Length;
                    fileDetail.FileSizeText = fileDetail.FileSizeNumber + " bytes";
                    fileDetailList.Add(fileDetail);
                }
            }
            return fileDetailList;
        }

        public FileStreamResult Download(FileDetail fileDetail)
        {
            try
            {
                var fileStream = new FileStream(fileDetail.FilePath!, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new FileStreamResult(fileStream, "application/octet-stream")
                {
                    FileDownloadName = fileDetail.FileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return null; // Or handle the error in a way that makes sense for your application
            }
        }

        public Task<string?> RenameFileAsync(FileDetail fileDetail, string newFileName)
        {
            string oldFilePath = fileDetail.FileName;
            string originalDirectory = Path.GetDirectoryName(oldFilePath)!;
            var fileExtenstion = Path.GetExtension(oldFilePath)!;
            string? newFilePath = null;
            try
            {

                if (fileExtenstion != null)
                {
                    newFilePath = Path.Combine(originalDirectory, newFileName + fileExtenstion);
                }
                else
                {
                    newFilePath = Path.Combine(originalDirectory, newFileName);
                }
                _logger.LogInformation("original path:${oldFilePath}", oldFilePath);
                _logger.LogInformation("new path:${newFilePath}", newFilePath);
                File.Move(oldFilePath, newFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                newFilePath = null;
                return Task.FromResult(newFilePath);
            }
            return Task.FromResult(newFilePath);
        }

        public Task DeleteAsync(FileDetail fileDetail)
        {
            try
            {
                var deleteFilePath = fileDetail.FilePath;
                if (File.Exists(deleteFilePath))
                {
                    // Attempt to delete the file
                    File.Delete(deleteFilePath);
                }
                else
                {
                    _logger.LogInformation("the file Path ${deleteFilePath} does not exist!", deleteFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
            return Task.CompletedTask;
        }

        public bool AddFileDetailInfo(List<FileDetailInfo> fileDetailInfoList)
        {
            try
            {
                fileDetailInfoList.ForEach(info =>
                {
                    _dbContext.FileDetailInfos.Add(info);
                });

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("[AddFileDetailInfo]:${error}", ex.StackTrace);
                return false;
            }
            return true;
        }

        public FileDetail? GetFileDetail(string attid)
        {
            var fileDetailInfo = _dbContext.FileDetailInfos.Where(info => info.AttId == attid).FirstOrDefault();
            if (fileDetailInfo == null) return null;
            return _mapper.Map<FileDetail>(fileDetailInfo);
        }
    }
}
