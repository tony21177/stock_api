using stock_api.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class AnnouncementService
    {
        private readonly HandoverContext _dbContext;
        private readonly ILogger<AnnouncementService> _logger;

        public AnnouncementService(HandoverContext dbContext, ILogger<AnnouncementService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public List<Announcement> GetAllAnnouncements()
        {
            return _dbContext.Announcements.ToList();
        }

        public Announcement? GetAnnouncementByAnnounceId(string annoucementId)
        {
            return _dbContext.Announcements.Where(annoucement => annoucement.AnnounceId == annoucementId).FirstOrDefault();
        }

        public List<Announcement> GetAnnouncementsByAnnounceIdList(List<string> annoucementIdList)
        {
            return _dbContext.Announcements.Where(annoucement => annoucementIdList.Contains(annoucement.AnnounceId)).ToList();
        }

        public List<Announcement> GetFilteredAnnouncements(ListAnnoucementRequest request)
        {
            var query = _dbContext.Announcements.AsQueryable();

            // Apply filters based on the request parameters
            if (!string.IsNullOrEmpty(request.CreatorID))
                query = query.Where(a => a.CreatorId == request.CreatorID);

            if (request.IsActive.HasValue)
                query = query.Where(a => a.IsActive == request.IsActive);

            if (!string.IsNullOrEmpty(request.Title))
                query = query.Where(a => a.Title.Contains(request.Title));

            if (!string.IsNullOrEmpty(request.Content))
                query = query.Where(a => a.Content.Contains(request.Content));
            // Ordering
            query = request.OrderBy switch
            {
                "Title" => request.IsAsc ? query.OrderBy(a => a.Title) : query.OrderByDescending(a => a.Title),
                "Content" => request.IsAsc ? query.OrderBy(a => a.Content) : query.OrderByDescending(a => a.Content),
                "BeginPublishTime" => request.IsAsc ? query.OrderBy(a => a.BeginPublishTime) : query.OrderByDescending(a => a.BeginPublishTime),
                "EndPublishTime" => request.IsAsc ? query.OrderBy(a => a.EndPublishTime) : query.OrderByDescending(a => a.EndPublishTime),
                "BeginViewTime" => request.IsAsc ? query.OrderBy(a => a.BeginViewTime) : query.OrderByDescending(a => a.BeginViewTime),
                "EndViewTime" => request.IsAsc ? query.OrderBy(a => a.EndViewTime) : query.OrderByDescending(a => a.EndViewTime),
                "CreatedTime" => request.IsAsc ? query.OrderBy(a => a.CreatedTime) : query.OrderByDescending(a => a.CreatedTime),
                "UpdatedTime" => request.IsAsc ? query.OrderBy(a => a.UpdatedTime) : query.OrderByDescending(a => a.UpdatedTime),
                _ => request.IsAsc ? query.OrderBy(a => a.Id) : query.OrderByDescending(a => a.Id),// Default ordering by Id
            };

            // Pagination
            if (request.IsPagination && request.PageIndex.HasValue && request.PageSize.HasValue)
                query = query.Skip((request.PageIndex.Value - 1) * request.PageSize.Value).Take(request.PageSize.Value);

            return query.ToList();
        }

        public List<AnnounceAttachment> GetAttachmentsByAnnounceIds(List<string> inAnnounceIds)
        {
            var query = _dbContext.AnnounceAttachments.AsQueryable();

            // Apply filter based on the list of AnnounceIds
            query = query.Where(a => inAnnounceIds.Contains(a.AnnounceId));

            return query.ToList();
        }

        public void DeleteAttachmentByAttIds(List<string> attIdList)
        {
            _dbContext.AnnounceAttachments.Where(attachment => attIdList.Contains(attachment.AttId)).ExecuteDelete();
            return;
        }

        public Announcement? CreateAnnouncement(Announcement announcement, List<string> readerUserIdList, Models.Member creator, List<string> attIdList)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    // 生成新的 AnnounceId
                    announcement.AnnounceId = Guid.NewGuid().ToString();
                    announcement.CreatorId = creator.UserId; announcement.CreatorName = creator.DisplayName;

                    // 將 Announcement 實例添加到 _dbContext.Announcements 中
                    _dbContext.Announcements.Add(announcement);

                    readerUserIdList.ForEach(
                        userId =>
                        {
                            var newAnnouceReader = new AnnouceReader
                            {
                                ReaderId = Guid.NewGuid().ToString(),
                                AnnounceId = announcement.AnnounceId,
                                UserId = userId, //收件人
                                IsRead = false,
                                IsActive = true,
                            };
                            _dbContext.AnnouceReaders.Add(newAnnouceReader);
                            var myAnnouncemnet = new MyAnnouncement
                            {
                                Title = announcement.Title,
                                Content = announcement.Content,
                                BeginPublishTime = announcement.BeginPublishTime,
                                EndPublishTime = announcement.EndPublishTime,
                                BeginViewTime = announcement.BeginViewTime,
                                EndViewTime = announcement.EndViewTime,
                                IsActive = announcement.IsActive,
                                AnnounceId = announcement.AnnounceId,
                                CreatorId = creator.UserId,
                                UserId = userId, //收件人
                                IsBookToTop = false,
                                IsRemind = false,
                            };
                            _dbContext.MyAnnouncements.Add(myAnnouncemnet);
                        });

                    if (attIdList.Count > 0)
                    {
                        UpdateAnnounceAttachments(attIdList, announcement.AnnounceId, announcement.CreatorId);
                    }

                    AddAnnouncementHistory(null, announcement, new List<string> { }, readerUserIdList, new List<string> { }, attIdList, AnnoucementActionEnum.Create);

                    // 保存更改到資料庫
                    _dbContext.SaveChanges();

                    // 提交事務
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // 處理事務失敗的例外
                    // 這裡可以根據實際需求進行錯誤處理
                    _logger.LogError("事務失敗：{msg}", ex.Message);
                    return null;
                }
            }

            // 返回新創建的 Announcement 實例


            return announcement;
        }

        public bool UpdateAnnouncement(string announceId, Announcement newAnnouncement, Announcement originalAnnouncement, List<AnnouceReader> annouceReaders,
            UpdateAnnouncementRequest updateAnnouncementRequest, List<AnnounceAttachment> announceAttachments, List<MyAnnouncement> myAnnouncements)
        {
            using var scope = new TransactionScope();
            try
            {
                UpdateAnnouncementByAnnounceId(newAnnouncement, announceId);

                if (updateAnnouncementRequest.ReaderUserIdList != null)
                {
                    List<string> originalUserIdsInReaders = annouceReaders.Select(x => x.UserId).ToList();
                    List<string> newUserIdsInReaders = updateAnnouncementRequest.ReaderUserIdList;

                    List<string> userIdsOnlyInOriginalInReaders = originalUserIdsInReaders.Except(newUserIdsInReaders).ToList();
                    List<string> userIdsOnlyInNewInReaders = newUserIdsInReaders.Except(originalUserIdsInReaders).ToList();

                    List<string> originalUserIdsInMyAnnoucements = myAnnouncements.Select(x => x.UserId).ToList();
                    List<string> newUserIdsInMyAnnoucements = updateAnnouncementRequest.ReaderUserIdList;

                    List<string> userIdsOnlyInOriginalInMyAnnoucementsaders = originalUserIdsInReaders.Except(newUserIdsInReaders).ToList();
                    List<string> userIdsOnlyInNewInMyAnnoucementss = newUserIdsInReaders.Except(originalUserIdsInReaders).ToList();


                    //_dbContext.AnnouceReaders.Where(annouceReader => userIdsOnlyInOriginalInReaders.Contains(annouceReader.UserId)).ExecuteDelete();
                    //_dbContext.MyAnnouncements.Where(myAnnouncement => originalUserIdsInMyAnnoucements.Contains(myAnnouncement.UserId)).ExecuteDelete();

                    _dbContext.AnnouceReaders.Where(annouceReader => originalUserIdsInReaders.Contains(annouceReader.UserId) && annouceReader.AnnounceId == announceId).ExecuteDelete();
                    _dbContext.MyAnnouncements.Where(myAnnouncement => originalUserIdsInMyAnnoucements.Contains(myAnnouncement.UserId) && myAnnouncement.AnnounceId == announceId).ExecuteDelete();
                    newUserIdsInReaders.ForEach(
                    userId =>
                    {
                        var newAnnouceReader = new AnnouceReader
                        {
                            ReaderId = Guid.NewGuid().ToString(),
                            AnnounceId = announceId,
                            UserId = userId, //收件人
                            IsRead = false,
                            IsActive = true,
                        };
                        _dbContext.AnnouceReaders.Add(newAnnouceReader);
                        var myAnnouncemnet = new MyAnnouncement
                        {
                            Title = newAnnouncement.Title != null ? newAnnouncement.Title : originalAnnouncement.Title,
                            Content = newAnnouncement.Content != null ? newAnnouncement.Content : originalAnnouncement.Content,
                            BeginPublishTime = newAnnouncement.BeginPublishTime != null ? newAnnouncement.BeginPublishTime : originalAnnouncement.BeginPublishTime,
                            EndPublishTime = newAnnouncement.EndPublishTime != null ? newAnnouncement.EndPublishTime : originalAnnouncement.EndPublishTime,
                            BeginViewTime = newAnnouncement.BeginViewTime != null ? newAnnouncement.BeginViewTime : originalAnnouncement.BeginViewTime,
                            EndViewTime = newAnnouncement.EndViewTime != null ? newAnnouncement.EndViewTime : originalAnnouncement.EndViewTime,
                            IsActive = newAnnouncement.IsActive != null ? newAnnouncement.IsActive : originalAnnouncement.IsActive,
                            AnnounceId = announceId,
                            CreatorId = originalAnnouncement.CreatorId,
                            UserId = userId, //收件人
                            IsBookToTop = false,
                            IsRemind = false,
                        };
                        _dbContext.MyAnnouncements.Add(myAnnouncemnet);
                    });
                }
                if (updateAnnouncementRequest.AttIdList != null)
                {
                    //UpdateAnnounceAttachments(updateAnnouncementRequest.AttIdList, announcement.AnnounceId, announcement.CreatorId);
                    var originalAttachAttIds = announceAttachments.Select(attachment => attachment.AttId).ToList();
                    var newAttIds = updateAnnouncementRequest.AttIdList;
                    List<string> attIdsOnlyInOriginal = originalAttachAttIds.Except(newAttIds).ToList();
                    List<string> attIdsOnlyInNew = newAttIds.Except(originalAttachAttIds).ToList();
                    if (attIdsOnlyInOriginal.Count > 0)
                    {
                        UpdateAnnounIdToNullForAnnounceAttachment(attIdsOnlyInOriginal);
                    }
                    if (attIdsOnlyInNew.Count > 0)
                    {
                        UpdateAnnounceAttachments(attIdsOnlyInNew, announceId, originalAnnouncement.CreatorId);
                    }

                }
                List<string> oldReaderUserIdList = annouceReaders.Select(reader => reader.UserId).ToList();
                List<string> newReaderUserIdList = oldReaderUserIdList;
                if (updateAnnouncementRequest.ReaderUserIdList != null)
                {
                    newReaderUserIdList = updateAnnouncementRequest.ReaderUserIdList;
                }
                List<string> oldAttIdList = announceAttachments.Select(attachment => attachment.AttId).ToList();
                List<string> newAttIdList = oldAttIdList;
                if (updateAnnouncementRequest.AttIdList != null)
                {
                    newAttIdList = updateAnnouncementRequest.AttIdList;
                }

                AddAnnouncementHistory(originalAnnouncement, newAnnouncement, oldReaderUserIdList, newReaderUserIdList, oldAttIdList, newAttIdList, AnnoucementActionEnum.Update);

                // 保存更改到資料庫
                _dbContext.SaveChanges();

                // 提交事務
                scope.Complete();
            }
            catch (Exception ex)
            {
                // 處理事務失敗的例外
                // 這裡可以根據實際需求進行錯誤處理
                _logger.LogError("事務失敗：{msg}", ex.Message);
                return false;
            }

            return true;
        }


        public List<AnnounceAttachment> AnnounceAttachments(List<FileDetail> fileDetails)
        {
            var lastIndex = _dbContext.AnnounceAttachments
                .OrderByDescending(a => a.Index)
                .Select(a => a.Index)
                .FirstOrDefault();

            var newAnnounceAttachments = fileDetails.Select(fileDetail =>
            {
                lastIndex++;
                var announceAttachment = new AnnounceAttachment
                {
                    AttId = Guid.NewGuid().ToString(),
                    Index = lastIndex,
                    FileName = fileDetail.FileName,
                    FilePath = fileDetail.FilePath,
                    FileSizeNumber = fileDetail.FileSizeNumber,
                    FileSizeText = fileDetail.FileSizeText,
                    FileType = fileDetail.FileType,
                };

                _dbContext.AnnounceAttachments.Add(announceAttachment);

                return announceAttachment;
            }).ToList();
            _dbContext.SaveChanges();
            return newAnnounceAttachments;
        }

        public AnnounceAttachment? GetAttachment(string attId)
        {
            return _dbContext.AnnounceAttachments.Where(at => at.AttId == attId).FirstOrDefault();
        }

        public List<AnnounceAttachment> GetAnnounceAttachmentsByAttIds(List<string> attIdList)
        {
            if (attIdList.Count == 0)
            {
                return new List<AnnounceAttachment>();
            }

            return _dbContext.AnnounceAttachments.Where(attachment => attIdList.Contains(attachment.AttId)).ToList();
        }

        public List<AnnouceReader> GetAnnouceReadersByUserIds(List<string> userIds)
        {
            if (userIds.Count == 0)
            {
                return new List<AnnouceReader>();
            }
            return _dbContext.AnnouceReaders.Where(annouceReader => userIds.Contains(annouceReader.UserId)).ToList();
        }
        public List<AnnouceReader> GetAnnouceReaderByAnnouncementId(string announceId)
        {
            return _dbContext.AnnouceReaders.Where(annouceReader => annouceReader.AnnounceId == announceId).ToList();
        }

        public List<MyAnnouncement> GetMyAnnouncements(string announceId)
        {
            return _dbContext.MyAnnouncements.Where(myAnnouncement => myAnnouncement.AnnounceId == announceId).ToList();
        }
        public MyAnnouncement? GetMyAnnouncements(string announceId, string userId)
        {
            return _dbContext.MyAnnouncements.Where(myAnnouncement => myAnnouncement.AnnounceId == announceId && myAnnouncement.UserId == userId).FirstOrDefault();
        }
        public List<MyAnnouncement> GetMyAnnouncementsByUserId(string userId)
        {
            return _dbContext.MyAnnouncements.Where(myAnnouncement => myAnnouncement.UserId == userId).ToList();
        }
        public bool UpdateMyAnnouncements(int id, UpdateMyAnnouncementRequest request)
        {
            var myAnnouncementToUpdate = _dbContext.MyAnnouncements.FirstOrDefault(_myAnnouncement => _myAnnouncement.Id == id);

            if (myAnnouncementToUpdate != null)
            {
                // Update properties if the corresponding request properties are not null
                if (request.IsBookToTop.HasValue)
                {
                    myAnnouncementToUpdate.IsBookToTop = request.IsBookToTop ?? myAnnouncementToUpdate.IsBookToTop;
                }

                if (request.IsRemind.HasValue)
                {
                    myAnnouncementToUpdate.IsRemind = request.IsRemind ?? myAnnouncementToUpdate.IsRemind;
                }
                _dbContext.SaveChanges();

                return true;
            }

            return false;
        }


        public AnnouceReader? UpdateAnnounceReaderToRead(string announceId, string userId)
        {
            var announcementToUpdate = _dbContext.AnnouceReaders
                .FirstOrDefault(a => a.AnnounceId == announceId && a.UserId == userId);

            if (announcementToUpdate != null)
            {
                announcementToUpdate.IsRead = true;
                announcementToUpdate.ReadTime = DateTime.Now;

                _dbContext.SaveChanges();

                return announcementToUpdate;
            }

            return null; // Return null if the announcement is not found
        }

        public bool DeleteByAnnounceId(string announceId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // Step 1: Delete from 'AnnouceReader'
                _dbContext.AnnouceReaders.Where(announceReader => announceReader.AnnounceId == announceId).ExecuteDelete();


                // Step 2: Delete from 'MyAnnouncement'
                _dbContext.MyAnnouncements.Where(myAnnouncement => myAnnouncement.AnnounceId == announceId).ExecuteDelete();

                // Step 3: Delete from 'Announcement'
                _dbContext.Announcements.Where(announcement => announcement.AnnounceId == announceId).ExecuteDelete();
                UpdateAnnounIdToNullForAnnounceAttachment(announceId);

                // Save changes to the database
                _dbContext.SaveChanges();

                // If all deletions were successful, commit the transaction
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions and log if necessary
                Console.WriteLine($"Error deleting records: {ex.Message}");

                // Rollback the transaction in case of an exception
                transaction.Rollback();
                return false;
            }

        }

        public bool InActiveByAnnounceId(string announceId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var announcement = _dbContext.Announcements.Where(_announcement => _announcement.AnnounceId == announceId).FirstOrDefault();
                if (announcement != null)
                {
                    var announceReaderList = _dbContext.AnnouceReaders.Where(annouceReader => annouceReader.AnnounceId == announceId).ToList();
                    var myAnnouncementList = _dbContext.MyAnnouncements.Where(myAnnouncement => myAnnouncement.AnnounceId == announceId).ToList();
                    var attachments = _dbContext.AnnounceAttachments.Where(attachment => attachment.AnnounceId == announceId).ToList();
                    announceReaderList.ForEach(annouceReader => annouceReader.IsActive = false);
                    myAnnouncementList.ForEach(myAnnouncement => myAnnouncement.IsActive = false);
                    var readerUserIdList = announceReaderList.Select(r => r.UserId).ToList();
                    var attIdList = attachments.Select(a => a.AttId).ToList();
                    InActiveAnnoucementHistory(announcement, attIdList, readerUserIdList);
                    announcement.IsActive = false;

                }

                // Save changes to the database
                _dbContext.SaveChanges();

                // If all deletions were successful, commit the transaction
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions and log if necessary
                Console.WriteLine($"Error InActiveByAnnounceId records: {ex.Message}");

                // Rollback the transaction in case of an exception
                transaction.Rollback();
                return false;
            }
        }

        public void UpdateAnnounceAttachments(List<string> attIds, string annoucementId, string creatorId)
        {
            // Construct the SQL UPDATE statement
            var updateSql = $@"
            UPDATE announce_attachment
            SET AnnounceId = '{annoucementId}', CreatorId = '{creatorId}'
            WHERE AttId IN ({string.Join(",", attIds.Select(id => $"'{id}'"))})";

            // Execute the raw SQL update statement
            _dbContext.Database.ExecuteSqlRaw(updateSql);
        }

        public void UpdateAnnounIdToNullForAnnounceAttachment(List<string> attIds)
        {
            // Construct the SQL UPDATE statement
            var updateSql = $@"
            UPDATE announce_attachment
            SET AnnounceId = NULL, CreatorId = NULL
            WHERE AttId IN ({string.Join(",", attIds.Select(id => $"'{id}'"))})";

            // Execute the raw SQL update statement
            _dbContext.Database.ExecuteSqlRaw(updateSql);
        }

        public void UpdateAnnounIdToNullForAnnounceAttachment(string announceId)
        {
            // Construct the parameterized SQL UPDATE statement
            var updateSql = @"
            UPDATE announce_attachment
            SET AnnounceId = NULL, CreatorId = NULL
            WHERE AnnounceId = @announceId";

            // Execute the parameterized SQL update statement with the announceId parameter
            _dbContext.Database.ExecuteSqlRaw(updateSql, new MySqlParameter("@announceId", announceId));
        }

        public void UpdateAnnouncementByAnnounceId(Announcement updateAnnouncement, string announceId)
        {
            // Construct the SQL UPDATE statement
            var updateSql = $@"
        UPDATE announcement
        SET 
            Title = {(updateAnnouncement.Title != null ? $"'{updateAnnouncement.Title}'" : "Title")}, 
            Content = {(updateAnnouncement.Content != null ? $"'{updateAnnouncement.Content}'" : "Content")},
            BeginPublishTime = {(updateAnnouncement.BeginPublishTime != null ? $"'{updateAnnouncement.BeginPublishTime:yyyy-MM-dd HH:mm:ss}'" : "BeginPublishTime")},
            EndPublishTime = {(updateAnnouncement.EndPublishTime != null ? $"'{updateAnnouncement.EndPublishTime:yyyy-MM-dd HH:mm:ss}'" : "EndPublishTime")},
            BeginViewTime = {(updateAnnouncement.BeginViewTime != null ? $"'{updateAnnouncement.BeginViewTime:yyyy-MM-dd HH:mm:ss}'" : "BeginViewTime")},
            EndViewTime = {(updateAnnouncement.EndViewTime != null ? $"'{updateAnnouncement.EndViewTime:yyyy-MM-dd HH:mm:ss}'" : "EndViewTime")},
            IsActive = {(updateAnnouncement.IsActive != null ? updateAnnouncement.IsActive.ToString() : "IsActive")}
        WHERE AnnounceId = '{announceId}'";

            // Execute the raw SQL update statement
            _dbContext.Database.ExecuteSqlRaw(updateSql);
        }

        public void InActiveAnnoucementHistory(Announcement originalAnnouncement, List<string> attIdList, List<string> readerUserIdList)
        {
            var newAnnounceHistory = new AnnouncementHistory
            {
                OldTitle = originalAnnouncement.Title,
                NewTitle = originalAnnouncement.Title,
                OldContent = originalAnnouncement.Content,
                NewContent = originalAnnouncement.Content,
                OldBeginPublishTime = originalAnnouncement.BeginPublishTime,
                NewBeginPublishTime = originalAnnouncement.BeginPublishTime,
                OldEndPublishTime = originalAnnouncement.EndPublishTime,
                NewEndPublishTime = originalAnnouncement.EndPublishTime,
                OldBeginViewTime = originalAnnouncement.BeginViewTime,
                NewBeginViewTime = originalAnnouncement.BeginViewTime,
                OldEndViewTime = originalAnnouncement.EndViewTime,
                NewEndViewTime = originalAnnouncement.EndViewTime,
                OldIsActive = originalAnnouncement.IsActive,
                NewIsActive = false,
                CreatorId = originalAnnouncement.CreatorId,
                CreatorName = originalAnnouncement.CreatorName,
                AnnounceId = originalAnnouncement.AnnounceId,
                OldAttId = string.Join(",", attIdList),
                NewAttId = string.Join(",", attIdList),
                OldReaderUserIdList = string.Join(",", readerUserIdList),
                NewReaderUserIdList = string.Join(",", readerUserIdList),
                Action = AnnoucementActionEnum.InActive.ToString(),
            };

            _dbContext.AnnouncementHistories.Add(newAnnounceHistory);
        }

        public void AddAnnouncementHistory(Announcement? originalAnnouncement, Announcement newAnnouncement,
            List<string> oldReaderUserIdList, List<string> newReaderUserIdList,
            List<string> oldAttId, List<string> newAttId, AnnoucementActionEnum actionEnum)
        {
            var newAnnounceHistory = new AnnouncementHistory
            {
                OldTitle = originalAnnouncement?.Title ?? null,
                NewTitle = newAnnouncement.Title,
                OldContent = originalAnnouncement?.Content ?? null,
                NewContent = newAnnouncement.Content,
                OldBeginPublishTime = originalAnnouncement?.BeginPublishTime ?? null,
                NewBeginPublishTime = newAnnouncement.BeginPublishTime,
                OldEndPublishTime = originalAnnouncement?.EndPublishTime ?? null,
                NewEndPublishTime = newAnnouncement.EndPublishTime,
                OldBeginViewTime = originalAnnouncement?.BeginViewTime ?? null,
                NewBeginViewTime = newAnnouncement.BeginViewTime,
                OldEndViewTime = originalAnnouncement?.EndViewTime ?? null,
                NewEndViewTime = newAnnouncement.EndViewTime,
                OldIsActive = originalAnnouncement?.IsActive ?? null,
                NewIsActive = newAnnouncement.IsActive,
                CreatorId = originalAnnouncement?.CreatorId ?? newAnnouncement.CreatorId,
                CreatorName = originalAnnouncement?.CreatorName ?? newAnnouncement.CreatorName,
                AnnounceId = originalAnnouncement?.AnnounceId ?? newAnnouncement.AnnounceId,
                OldAttId = string.Join(",", oldAttId),
                NewAttId = string.Join(",", newAttId),
                OldReaderUserIdList = string.Join(",", oldReaderUserIdList),
                NewReaderUserIdList = string.Join(",", newReaderUserIdList),
                Action = actionEnum.ToString(),
            };

            _dbContext.AnnouncementHistories.Add(newAnnounceHistory);
        }

        public bool IsUserReadAnnouncement(string announcementId, string userId)
        {
            var annoucementReader = _dbContext.AnnouceReaders.Where(annouceReader => annouceReader.UserId == userId && annouceReader.AnnounceId == announcementId).FirstOrDefault();
            if (annoucementReader != null)
            {
                return annoucementReader.IsRead;
            }
            return false;
        }


        public List<AnnouncementHistory> GetAnnouncementHistoriesByAnnounceId(string announceId)
        {
            var announcementHistoryList = _dbContext.AnnouncementHistories.Where(history => history.AnnounceId == announceId).OrderByDescending(history => history.Id).ToList();
            return announcementHistoryList;
        }

        public List<Announcement> GetUnreadAnnouncement(Member reader)
        {
            List<AnnouceReader> unreadAnnouceReaderList = _dbContext.AnnouceReaders.Where(ar => ar.IsRead != true && ar.UserId == reader.UserId).ToList();
            List<string> unreadAnnounceIdList = unreadAnnouceReaderList.Select(uar => uar.AnnounceId).ToList();
            List<Announcement> unreadAnnouncements = GetAnnouncementsByAnnounceIdList(unreadAnnounceIdList);
            return unreadAnnouncements.Where(ua => ua.IsActive == true).ToList();
        }
    }
}
