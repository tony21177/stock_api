using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using MaiBackend.PublicApi.Consts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Transactions;

namespace stock_api.Service
{
    public class HandoverService
    {
        private readonly HandoverContext _dbContext;
        private readonly ILogger<HandoverService> _logger;
        private readonly IMapper _mapper;

        public HandoverService(HandoverContext dbContext, ILogger<HandoverService> logger, IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }

        public List<HandoverSheetMain> GetAllHandoverSheetMain()
        {
            return _dbContext.HandoverSheetMains.ToList();
        }

        public HandoverSheetMain? GetSheetMainByMainSheetId(int mainSheetId)
        {
            return _dbContext.HandoverSheetMains.Where(m => m.SheetId == mainSheetId).FirstOrDefault();
        }

        public HandoverSheetGroup? GetSheetGroupBySheetGroupId(int sheetGroupId)
        {
            return _dbContext.HandoverSheetGroups.Where(g => g.SheetGroupId == sheetGroupId).FirstOrDefault();
        }

        public List<HandoverSheetGroup> GetSheetGroupByMainSheetId(int mainSheetId)
        {
            return _dbContext.HandoverSheetGroups.Where(g => g.MainSheetId == mainSheetId).ToList();
        }

        public List<HandoverSheetRow> GetSheetRowsByMainSheetIdAndInSheetGroupIds(int mainSheetId, List<int> sheetGroupId)
        {
            return _dbContext.HandoverSheetRows.Where(r => r.MainSheetId == mainSheetId && r.SheetGroupId.HasValue && sheetGroupId.Contains(r.SheetGroupId.Value)).ToList();
        }

        public List<HandoverSheetMain> GetSheetMainListBySheetRowIdList(List<int> sheetRowIdList)
        {
            List<HandoverSheetRow> handoverSheetRows = _dbContext.HandoverSheetRows.Where(row => sheetRowIdList.Contains(row.SheetRowId)).ToList();

            List<int> mainSheetIdList = handoverSheetRows.Select(row => row.MainSheetId.Value).ToList();

            return _dbContext.HandoverSheetMains.Where(m => mainSheetIdList.Contains(m.SheetId)).ToList();
        }

        //public List<HandoverSheetRow> GetSheetRowsByMainSheetId(int mainSheetId)
        //{
        //    return _dbContext.HandoverSheetRows.Where(r => r.MainSheetId == mainSheetId).ToList();
        //}

        public List<HandoverSheetRowWithGroup> GetSheetRowsByMainSheetId(int mainSheetId)
        {
            var query = from sheetRow in _dbContext.HandoverSheetRows
                        join sheetGroup in _dbContext.HandoverSheetGroups
                        on sheetRow.SheetGroupId equals sheetGroup.SheetGroupId
                        where sheetRow.MainSheetId == mainSheetId
                        select new HandoverSheetRowWithGroup
                        {
                            Id = sheetRow.Id,
                            MainSheetId = sheetRow.MainSheetId,
                            SheetGroupId = sheetRow.SheetGroupId,
                            SheetRowId = sheetRow.SheetRowId,
                            WeekDays = sheetRow.WeekDays,
                            SheetGroupTitle = sheetRow.SheetGroupTitle,
                            RowCategory = sheetRow.RowCategory,
                            MachineBrand = sheetRow.MachineBrand,
                            MachineCode = sheetRow.MachineCode,
                            MachineSpec = sheetRow.MachineSpec,
                            MaintainItemName = sheetRow.MaintainItemName,
                            MaintainItemDescription = sheetRow.MaintainItemDescription,
                            MaintainItemType = sheetRow.MaintainItemType,
                            MaintainAnswerType = sheetRow.MaintainAnswerType,
                            Remarks = sheetRow.Remarks,
                            CreatorName = sheetRow.CreatorName,
                            IsActive = sheetRow.IsActive,
                            CreatedTime = sheetRow.CreatedTime,
                            UpdatedTime = sheetRow.UpdatedTime,
                            IsGroupActive = sheetGroup.IsActive
                        };

            return query.ToList();
        }
        public HandoverDetail? GetHandoverDetail(string handoverDetailId)
        {
            return _dbContext.HandoverDetails.Where(d => d.HandoverDetailId == handoverDetailId).FirstOrDefault();
        }

        public bool InActiveHandoverDetail(HandoverDetail handoverDetail)
        {
            handoverDetail.IsActive = false;
            _dbContext.SaveChanges();
            return true;
        }

        public List<HandoverSheetMain> UpdateHandoverSheetMains(List<HandoverSheetMain> updateHandoverSheetMainList)
        {
            var updatedHandoverSheetMainList = new List<HandoverSheetMain>();
            updateHandoverSheetMainList.ForEach(updateHandoverSheetMain =>
            {
                var existingSheetMain = _dbContext.HandoverSheetMains.Find(updateHandoverSheetMain.SheetId);
                if (existingSheetMain != null)
                {
                    _mapper.Map(existingSheetMain, updateHandoverSheetMain);
                    _dbContext.Entry(existingSheetMain).CurrentValues.SetValues(updateHandoverSheetMain);
                    updatedHandoverSheetMainList.Add(updateHandoverSheetMain);
                }
            });
            _dbContext.SaveChanges();
            return updatedHandoverSheetMainList;
        }

        public int GetMaxSheetMainId()
        {
            var maxSheetId = _dbContext.HandoverSheetMains
                .Select(sheetMain => (int?)sheetMain.SheetId)
                .OrderByDescending(sheetId => sheetId)
                .FirstOrDefault(); // 取得第一個結果或者 null
            if (maxSheetId.HasValue)
            {
                return maxSheetId.Value; // 如果有值，返回該值
            }
            else
            {
                return 0; // 如果為 null，返回預設值
            }
        }


        public int GetMaxSheetGroupId(int mainSheetId)
        {
            var maxSheetGroupId = _dbContext.HandoverSheetGroups
                .Where(sheetGroup => sheetGroup.MainSheetId == mainSheetId)
                .Select(sheetGroup => (int?)sheetGroup.SheetGroupId)
                .OrderByDescending(sheetGroupId => sheetGroupId)
                .FirstOrDefault(); // 取得第一個結果或者 null

            if (maxSheetGroupId.HasValue)
            {
                return maxSheetGroupId.Value; // 如果有值，返回該值
            }
            else
            {
                return (int)(mainSheetId * 100); // 如果為 null，返回預設值
            }
        }

        public int GetMaxSheetRowId(int mainSheetId, int sheetGroupId)
        {
            var maxSheetRowId = _dbContext.HandoverSheetRows
                .Where(row => row.MainSheetId == mainSheetId && row.SheetGroupId == sheetGroupId)
                .Select(row => (int?)row.SheetRowId)
                .OrderByDescending(rowId => rowId)
                .FirstOrDefault(); // 取得第一個結果或者 null

            if (maxSheetRowId.HasValue)
            {
                return maxSheetRowId.Value; // 如果有值，返回該值
            }
            else
            {
                return (int)(sheetGroupId * 1000); // 如果為 null，返回預設值
            }
        }

        public bool CreateHandoverSheetMain(HandoverSheetMain newHandoverSheetMain)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var newId = GetMaxSheetMainId() + 1;
                    newHandoverSheetMain.SheetId = newId;
                    newHandoverSheetMain.Id = Guid.NewGuid().ToString();
                    _dbContext.HandoverSheetMains.Add(newHandoverSheetMain);
                    _dbContext.SaveChanges(true);
                    // 提交事務
                    scope.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    // 處理事務失敗的例外
                    // 這裡可以根據實際需求進行錯誤處理
                    _logger.LogError("事務失敗[CreateHandoverSheetMain]：{msg}", ex.Message);
                    return false;
                }
            }
        }

        public void DeleteHandoverSheetMain(int sheetID)
        {
            var sheetMainToDelete = new HandoverSheetMain { SheetId = sheetID };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(sheetMainToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void InActiveHandoverSheetMain(int sheetID)
        {
            var updateSheetMain = _dbContext.HandoverSheetMains.Where(m => m.SheetId == sheetID).FirstOrDefault();
            if (updateSheetMain != null)
            {
                updateSheetMain.IsActive = false;
                // 將更改應用到資料庫
                _dbContext.SaveChanges();
            }
            return;
        }

        public List<HandoverSheetGroup> GetAllHandoverSheetGroup()
        {
            return _dbContext.HandoverSheetGroups.ToList();
        }

        public List<HandoverSheetGroup> UpdateHandoverSheetGroups(List<HandoverSheetGroup> updateHandoverSheetGroupList)
        {
            var updatedHandoverSheetGroupList = new List<HandoverSheetGroup>();
            updateHandoverSheetGroupList.ForEach(updateHandoverSheetGroup =>
            {
                var existingSheetGroup = _dbContext.HandoverSheetGroups.Where(group => group.SheetGroupId == updateHandoverSheetGroup.SheetGroupId).FirstOrDefault();
                if (existingSheetGroup != null)
                {
                    _mapper.Map(existingSheetGroup, updateHandoverSheetGroup);
                    _dbContext.Entry(existingSheetGroup).CurrentValues.SetValues(updateHandoverSheetGroup);
                    updatedHandoverSheetGroupList.Add(updateHandoverSheetGroup);
                }
            });
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return updatedHandoverSheetGroupList;
        }

        public bool CreateHandoverSheetGroup(HandoverSheetGroup newHandoverSheetGroup)
        {
            using var scope = new TransactionScope();
            try
            {
                var newSheetGroupId = GetMaxSheetGroupId(newHandoverSheetGroup.MainSheetId.Value);
                newHandoverSheetGroup.SheetGroupId = newSheetGroupId + 1;
                newHandoverSheetGroup.Id = Guid.NewGuid().ToString();
                _dbContext.HandoverSheetGroups.Add(newHandoverSheetGroup);
                _dbContext.SaveChanges(true);
                // 提交事務
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                // 處理事務失敗的例外
                // 這裡可以根據實際需求進行錯誤處理
                _logger.LogError("事務失敗[CreateHandoverSheetGroup]：{msg}", ex.Message);
                return false;
            }
        }

        public void DeleteHandoverSheetGroup(int sheetGroudId)
        {
            var sheetGroupToDelete = new HandoverSheetGroup { SheetGroupId = sheetGroudId };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(sheetGroupToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void InActiveHandoverSheetGroup(int sheetGroudId)
        {
            var updateSheetMain = _dbContext.HandoverSheetGroups.Where(m => m.SheetGroupId == sheetGroudId).FirstOrDefault();
            if (updateSheetMain != null)
            {
                updateSheetMain.IsActive = false;
                // 將更改應用到資料庫
                _dbContext.SaveChanges();
            }

            return;
        }

        public List<HandoverSheetRow> GetAllHandoverSheetRow()
        {
            return _dbContext.HandoverSheetRows.ToList();
        }

        public List<HandoverSheetRow> UpdateHandoverSheetRows(List<HandoverSheetRow> updateHandoverSheetRowList)
        {
            var updatedHandoverSheetRowList = new List<HandoverSheetRow>();
            updateHandoverSheetRowList.ForEach(updateHandoverSheetRow =>
            {
                var existingSheetRow = _dbContext.HandoverSheetRows.Where(r => r.SheetRowId == updateHandoverSheetRow.SheetRowId).FirstOrDefault();
                if (existingSheetRow != null)
                {
                    _mapper.Map(existingSheetRow, updateHandoverSheetRow);
                    // 使用 SetValues 來只更新不為 null 的屬性
                    _dbContext.Entry(existingSheetRow).CurrentValues.SetValues(updateHandoverSheetRow);
                    updatedHandoverSheetRowList.Add(updateHandoverSheetRow);
                }
            });
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return updatedHandoverSheetRowList;
        }

        public bool CreateHandoverSheetRow(HandoverSheetRow newHandoverSheetRow)
        {
            using var scope = new TransactionScope();
            try
            {
                var newSheetRowId = GetMaxSheetRowId(newHandoverSheetRow.MainSheetId.Value, newHandoverSheetRow.SheetGroupId.Value);
                newHandoverSheetRow.SheetRowId = newSheetRowId + 1;
                newHandoverSheetRow.Id = Guid.NewGuid().ToString();
                _dbContext.HandoverSheetRows.Add(newHandoverSheetRow);
                _dbContext.SaveChanges(true);
                // 提交事務
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                // 處理事務失敗的例外
                // 這裡可以根據實際需求進行錯誤處理
                _logger.LogError("事務失敗[CreateHandoverSheetRow]：{msg}", ex.Message);
                return false;
            }

        }

        public void DeleteHandoverSheetRow(int sheetRowId)
        {
            ;
            var sheetRowToDelete = new HandoverSheetRow { SheetRowId = sheetRowId };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(sheetRowToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void InActiveHandoverSheetRow(int sheetRowId)
        {
            var updateSheetRow = _dbContext.HandoverSheetRows.Where(m => m.SheetRowId == sheetRowId).FirstOrDefault();
            if (updateSheetRow != null)
            {
                updateSheetRow.IsActive = false;
                // 將更改應用到資料庫
                _dbContext.SaveChanges();
            }

            return;
        }

        public List<SheetSetting> GetAllSettings()
        {
            List<HandoverSheetMain> handoverSheetMainList = GetAllHandoverSheetMain();
            List<SheetSetting> sheetSettingDtoList = _mapper.Map<List<SheetSetting>>(handoverSheetMainList);


            List<HandoverSheetGroup> handoverSheetGroups = GetAllHandoverSheetGroup();
            List<HandoverSheetGroupDto> handoverSheetGroupDtoList = _mapper.Map<List<HandoverSheetGroupDto>>(handoverSheetGroups);
            List<HandoverSheetRow> handoverSheetRows = GetAllHandoverSheetRow();

            handoverSheetGroupDtoList.ForEach(groupDto =>
            {
                List<HandoverSheetRow> matchedHandoverSheetRows = handoverSheetRows.Where(row => row.SheetGroupId == groupDto.SheetGroupId).ToList();
                groupDto.HandoverSheetRowList = matchedHandoverSheetRows;
            });

            sheetSettingDtoList.ForEach(settingDto =>
            {
                List<HandoverSheetGroupDto> matchedSheetGroupDtoList = handoverSheetGroupDtoList.Where(settingGroupDto => settingGroupDto.MainSheetId == settingDto.SheetId).ToList();
                settingDto.HandoverSheetGroupList = matchedSheetGroupDtoList;
            });
            return sheetSettingDtoList;
        }

        public string? CreateHandOverDetail(int mainSheetId, List<RowDetail> rowDetails, String? title, String? content, List<Member> readerMemberList, Member creator, List<string> fileAttIdList)
        {
            if (rowDetails.Count == 0) { return null; }

            var mainSheetSetting = GetSheetMainByMainSheetId(mainSheetId);
            List<HandoverSheetGroup> handoverSheetGroups = GetSheetGroupByMainSheetId(mainSheetId).Where(group => group.IsActive == true).ToList();
            List<int> inSheetGroupIdList = handoverSheetGroups.Select(group => group.SheetGroupId).ToList();
            List<HandoverSheetRow> handoverSheetRows = GetSheetRowsByMainSheetIdAndInSheetGroupIds(mainSheetId, inSheetGroupIdList).Where(row => row.IsActive == true).ToList();

            HandoverSheetRowDetailAndSettings handoverSheetRowDetailAndSettings = _mapper.Map<HandoverSheetRowDetailAndSettings>(mainSheetSetting);
            List<GroupSetting> groupSettings = _mapper.Map<List<GroupSetting>>(handoverSheetGroups);
            List<RowSettingAndDetail> rowSettingAndDetails = _mapper.Map<List<RowSettingAndDetail>>(handoverSheetRows);

            // 補齊handoverSheetRowDetailAndSettings欄位
            List<Reader> readers = readerMemberList.Select(m =>
            {
                return new Reader
                {
                    UserId = m.UserId,
                    Name = m.DisplayName,
                    IsRead = false
                };
            }).ToList();
            handoverSheetRowDetailAndSettings.readers = readers;
            handoverSheetRowDetailAndSettings.HandoverSheetGroupList = groupSettings;

            // 補齊handoverSheetRowDetailAndSettings.HandoverSheetGroupLis欄位
            handoverSheetRowDetailAndSettings.HandoverSheetGroupList.ForEach(group =>
            {
                // 補齊RowSettingAndDetail欄位
                var matchedRowSettingAndDetailList = rowSettingAndDetails.FindAll(r => r.SheetGroupId == group.SheetGroupId);
                matchedRowSettingAndDetailList.ForEach(row =>
                {
                    var matchedRowDetail = rowDetails.Find(rd => rd.SheetRowId == row.SheetRowId);
                    if (matchedRowDetail != null)
                    {
                        row.Status = matchedRowDetail.Status;
                        row.Comment = matchedRowDetail.Comment;
                    }
                    else
                    {
                        _logger.LogError("[CreateHandOverDetail] 區少row  setting:rowId={rowId}的交班資料", row.SheetRowId);
                    }
                });
                group.RowSettingAndDetailList = matchedRowSettingAndDetailList;
            });

            string jsonContent = System.Text.Json.JsonSerializer.Serialize(handoverSheetRowDetailAndSettings);

            // 新增handover_detail
            HandoverDetail newHandoverDetail = new()
            {
                Title = title,
                HandoverDetailId = Guid.NewGuid().ToString(),
                MainSheetId = mainSheetId,
                JsonContent = jsonContent,
                CreatorId = creator.UserId,
                CreatorName = creator.DisplayName,
                FileAttIds = string.Join(",", fileAttIdList),
            };
            if (content != null)
            {
                newHandoverDetail.Content = content;
            }


            using var scope = new TransactionScope();
            try
            {
                _dbContext.HandoverDetails.Add(newHandoverDetail);
                List<HandoverDetailReader> handoverDetailReaders = readerMemberList.Select(reader =>
                {
                    HandoverDetailReader handoverReader = new()
                    {
                        HandoverDetailId = newHandoverDetail.HandoverDetailId,
                        UserId = reader.UserId,
                        UserName = reader.DisplayName,
                        IsRead = false,


                    };
                    return handoverReader;
                }).ToList();
                _dbContext.HandoverDetailReaders.AddRange(handoverDetailReaders);
                AddHandoverDetailHistory(newHandoverDetail, null, newHandoverDetail.Title, null, newHandoverDetail.Content,
                        null, readerMemberList.Select(m => m.UserId).ToList(), null, readerMemberList.Select(m => m.DisplayName).ToList(), null, newHandoverDetail.JsonContent, null, newHandoverDetail.FileAttIds
                        , Enum.GetName(ActionTypeEnum.Create));
                _dbContext.SaveChanges(true);
                // 提交事務
                scope.Complete();
                return jsonContent;
            }
            catch (Exception ex)
            {
                // 處理事務失敗的例外
                // 這裡可以根據實際需求進行錯誤處理
                _logger.LogError("事務失敗[CreateHandOverDetail]：{msg}", ex.Message);
                _logger.LogError("事務失敗[CreateHandOverDetail]：{StackTrace}", ex.StackTrace);
                return null;
            }
        }

        public string? UpdateHandover(HandoverDetail handoverDetail, List<RowDetail> rowDetails, String? title, String? content, List<Member>? readerMemberList, List<string> fileAttidList)
        {
            string oldJsonContent = handoverDetail.JsonContent;
            string oldFileAttIds = handoverDetail.FileAttIds;
            if (oldJsonContent == null)
            {
                _logger.LogError("資料庫有誤,jsonContent為null");
            }
            string? newJsonContent;
            try
            {
                HandoverSheetRowDetailAndSettings handoverSheetRowDetailAndSettings = JsonSerializer.Deserialize<HandoverSheetRowDetailAndSettings>(oldJsonContent);
                List<RowSettingAndDetail> allOriginalRowSettingAndDetail = new();
                if (handoverSheetRowDetailAndSettings == null)
                {
                    _logger.LogError("parse json 出來為null");
                    return null;
                }
                else
                {
                    handoverSheetRowDetailAndSettings?.HandoverSheetGroupList?.ForEach(
                    group =>
                    {
                        allOriginalRowSettingAndDetail.AddRange(group.RowSettingAndDetailList);
                    });


                    rowDetails.ForEach(rowDetail =>
                    {
                        var matchedOriginalRowDetail = allOriginalRowSettingAndDetail.Find(rd => rd.SheetRowId == rowDetail.SheetRowId);
                        if (matchedOriginalRowDetail != null)
                        {
                            matchedOriginalRowDetail.Status = rowDetail.Status;
                            matchedOriginalRowDetail.Comment = rowDetail.Comment;
                        }
                    });
                    newJsonContent = JsonSerializer.Serialize(handoverSheetRowDetailAndSettings);
                }

                List<HandoverDetailReader> handoverDetailReaders = GetHandoverDetailReadersByDetailId(handoverDetail.HandoverDetailId);
                List<string> originalReaderUserIdList = handoverDetailReaders.Select(r => r.UserId).ToList();
                List<string> originalReaderUserNames = handoverDetailReaders.Select(r => r.UserName).ToList();
                List<string> newReaderUserIdList = originalReaderUserIdList;
                List<string> newReaderUserNameList = originalReaderUserNames;
                if (readerMemberList != null)
                {
                    newReaderUserIdList = readerMemberList.Select(r => r.UserId).ToList();
                    newReaderUserNameList = readerMemberList.Select(r => r.DisplayName).ToList();
                }

                var oldTitle = handoverDetail.Title;
                var oldContent = handoverDetail.Content;
                var newTitle = title ?? oldTitle;
                var newContent = content ?? oldContent;

                using var transaction = _dbContext.Database.BeginTransaction();

                try
                {
                    if (title != null)
                    {
                        handoverDetail.Title = title;
                    }
                    if (content != null)
                    {
                        handoverDetail.Content = content;
                    }
                    handoverDetail.JsonContent = newJsonContent;
                    handoverDetail.FileAttIds = string.Join(",", fileAttidList);

                    List<string> toBeDeleteReaderUserIdList = originalReaderUserIdList.Except(newReaderUserIdList).ToList();
                    List<string> toBeAddReaderUserIdList = newReaderUserIdList.Except(originalReaderUserIdList).ToList();

                    List<HandoverDetailReader> toBeAddHandoverDetailReaderList = new();
                    toBeAddReaderUserIdList.ForEach(userId =>
                    {
                        Member newAddReaderMember = readerMemberList.Find(m => m.UserId == userId);

                        HandoverDetailReader handoverDetailReader = new HandoverDetailReader
                        {
                            HandoverDetailId = handoverDetail.HandoverDetailId,
                            UserId = userId,
                            UserName = newAddReaderMember.DisplayName,
                            IsRead = false,
                        };
                        toBeAddHandoverDetailReaderList.Add(handoverDetailReader);
                    });
                    _dbContext.HandoverDetailReaders.AddRange(toBeAddHandoverDetailReaderList);
                    DeleteHandoverDetailReader(handoverDetail.HandoverDetailId, toBeDeleteReaderUserIdList);

                    AddHandoverDetailHistory(handoverDetail, oldTitle, newTitle, oldContent, newContent,
                        originalReaderUserIdList, newReaderUserIdList, originalReaderUserNames, newReaderUserNameList, oldJsonContent, newJsonContent,
                        oldFileAttIds, string.Join(",", handoverDetail.FileAttIds)
                        , Enum.GetName(ActionTypeEnum.Update));

                    // Save changes to the database
                    _dbContext.SaveChanges();

                    // If all deletions were successful, commit the transaction
                    transaction.Commit();
                    return newJsonContent;
                }
                catch (Exception ex)
                {
                    // Handle exceptions and log if necessary
                    Console.WriteLine($"Error update handover: {ex.Message}");

                    // Rollback the transaction in case of an exception
                    transaction.Rollback();
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("parse json content有誤,ex:{message}", ex.Message);
                return null;
            }
        }

        public List<HandoverDetail> SearchHandoverDetails(int? mainSheetId, DateTime? startDate, DateTime? endDate, PaginationCondition pageCondition, string? searchString)
        {
            IQueryable<HandoverDetail> query = _dbContext.HandoverDetails;

            // 根据提供的条件进行过滤
            if (startDate != null)
            {
                query = query.Where(h => h.UpdatedTime >= startDate);
            }
            if (endDate != null)
            {
                query = query.Where(h => h.UpdatedTime <= endDate);
            }
            if (mainSheetId != null)
            {
                query = query.Where(h => h.MainSheetId == mainSheetId);
            }
            if (!string.IsNullOrEmpty(searchString))
            {

                // 根据 Content 或 Title 进行模糊匹配
                query = query.Where(h => h.Content.ToLower().Contains(searchString.ToLower())
                || h.Title.ToLower().Contains(searchString.ToLower())
                || h.CreatorName.ToLower().Contains(searchString.ToLower()));
            }


            if (pageCondition.IsDescOrderBy)
            {
                // 根据提供的 orderBy 字段进行排序，默认按照 CreatedTime 降序排序
                query = pageCondition.OrderByField switch
                {
                    "mainSheetId" => query.OrderBy(h => h.MainSheetId),
                    "nandoverDetailId" => query.OrderBy(h => h.HandoverDetailId),
                    "creatorName" => query.OrderBy(h => h.CreatorName),
                    "creatorId" => query.OrderBy(h => h.CreatorId),
                    "updatedTime" => query.OrderBy(h => h.UpdatedTime),
                    _ => query.OrderByDescending(h => h.CreatedTime),
                };
            }
            else
            {
                query = pageCondition.OrderByField switch
                {
                    "mainSheetId" => query.OrderBy(h => h.MainSheetId),
                    "nandoverDetailId" => query.OrderBy(h => h.HandoverDetailId),
                    "creatorName" => query.OrderBy(h => h.CreatorName),
                    "creatorId" => query.OrderBy(h => h.CreatorId),
                    "updatedTime" => query.OrderBy(h => h.UpdatedTime),
                    _ => query.OrderBy(h => h.CreatedTime),
                };
            }

            // 分页
            query = query.Skip((pageCondition.Page - 1) * pageCondition.PageSize).Take(pageCondition.PageSize);

            return query.ToList();
        }

        public List<HandoverDetailReader> GetHandoverDetailReadersByDetailId(string handoverDetailId)
        {
            return _dbContext.HandoverDetailReaders.Where(r => r.HandoverDetailId == handoverDetailId).ToList();
        }
        public List<HandoverDetailReader> GetHandoverDetailReadersByUserId(string userId)
        {
            return _dbContext.HandoverDetailReaders.Where(r => r.UserId == userId).ToList();
        }

        public void AddHandoverDetailHistory(HandoverDetail newHandoverDetail, string oldTitle, string newTitle, string oldContent, string newContent,
            List<string> oldReaderUserIdList, List<string> newReaderUserIdList, List<string> oldReaderUserNameList, List<string> newReaderUserNameList,
            string oldJsonContent, string newJsonContent, string? oldFileAttids, string? newFileAttIds, string action)
        {

            if (action == Enum.GetName(ActionTypeEnum.Update))
            {
                HandoverDetailHistory handoverDetailHistory = new()
                {
                    HandoverDetailId = newHandoverDetail.HandoverDetailId,
                    MainSheetId = newHandoverDetail.MainSheetId,
                    CreatorId = newHandoverDetail.CreatorId,
                    CreatorName = newHandoverDetail.CreatorName,

                    OldTitle = oldTitle,
                    OldJsonContent = oldJsonContent,
                    OldContent = oldContent,
                    OldReaderUserIds = string.Join(",", oldReaderUserIdList),
                    OldFileAttIds = oldFileAttids,

                    NewTitle = newTitle,
                    NewContent = newContent,
                    NewJsonContent = newJsonContent,
                    NewReaderUserIds = string.Join(",", newReaderUserIdList),
                    NewReaderUserNames = string.Join(",", newReaderUserNameList),
                    NewFileAttIds = newFileAttIds,
                    Action = action
                };
                _dbContext.HandoverDetailHistories.Add(handoverDetailHistory);
            }
            else if (action == Enum.GetName(ActionTypeEnum.Create))
            {
                HandoverDetailHistory handoverDetailHistory = new()
                {
                    HandoverDetailId = newHandoverDetail.HandoverDetailId,
                    MainSheetId = newHandoverDetail.MainSheetId,
                    CreatorId = newHandoverDetail.CreatorId,
                    CreatorName = newHandoverDetail.CreatorName,

                    NewTitle = newTitle,
                    NewContent = newContent,
                    NewJsonContent = newJsonContent,
                    NewReaderUserIds = string.Join(",", newReaderUserIdList),
                    NewReaderUserNames = string.Join(",", newReaderUserNameList),
                    NewFileAttIds = newFileAttIds,
                    Action = action
                };
                _dbContext.HandoverDetailHistories.Add(handoverDetailHistory);
            }
        }


        public void DeleteHandoverDetailReader(string handoverDetailId, List<string> toBeRemovedReaderUserIdList)
        {
            string inUserId = string.Join(",", toBeRemovedReaderUserIdList);

            var deleteSql = $@"
            Delete From handover_detail_readers
            WHERE UserId IN ('{inUserId}') and handoverDetailId='{handoverDetailId}'";

            // Execute the raw SQL update statement
            _dbContext.Database.ExecuteSqlRaw(deleteSql);
        }

        public HandoverDetail? GetHandoverDetailByDetailId(string handoverDetailId)
        {
            return _dbContext.HandoverDetails.Where(d => d.HandoverDetailId == handoverDetailId).FirstOrDefault();
        }

        public List<HandoverDetail> GetHandoverDetailByDetailIds(List<string> handoverDetailIdList)
        {
            return _dbContext.HandoverDetails.Where(d => handoverDetailIdList.Contains(d.HandoverDetailId)).ToList();
        }

        public bool ReadHandoverDetail(string handoverDetailId, string userId)
        {
            var handoverReader = _dbContext.HandoverDetailReaders.Where(dr => dr.HandoverDetailId == handoverDetailId && dr.UserId == userId).FirstOrDefault();
            if (handoverReader != null)
            {
                handoverReader.IsRead = true;
                handoverReader.ReadTime = DateTime.Now;
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public List<MyHandoverDetailDto> GetMyHandoverDetailDtoList(string userId)
        {
            List<HandoverDetailReader> handoverDetailReaders = _dbContext.HandoverDetailReaders.Where(rd => rd.UserId == userId).ToList();

            if (handoverDetailReaders.Count != 0)
            {
                List<MyHandoverDetailDto> myHandoverDetailDtoList = new();
                List<string> allDistinctHandoverDetailIdList = handoverDetailReaders.Select(r => r.HandoverDetailId).Distinct().ToList();
                List<HandoverDetail> handoverDetails = GetHandoverDetailByDetailIds(allDistinctHandoverDetailIdList);
                handoverDetailReaders.ForEach(dr =>
                {
                    var matchedDetail = handoverDetails.Find(d => d.HandoverDetailId == dr.HandoverDetailId);
                    if (matchedDetail != null)
                    {
                        MyHandoverDetailDto myHandoverDetailDto = _mapper.Map<MyHandoverDetailDto>(matchedDetail);
                        myHandoverDetailDto.IsRead = dr.IsRead;
                        myHandoverDetailDto.ReadTime = dr.ReadTime;
                        myHandoverDetailDtoList.Add(myHandoverDetailDto);
                    }
                });
                return myHandoverDetailDtoList;
            }
            return new();
        }

        public List<HandoverDetailHistory> GetHandoverDetailHistories(string handoverDetailId)
        {
            return _dbContext.HandoverDetailHistories.Where(h => h.HandoverDetailId == handoverDetailId).ToList();
        }

        public List<HandoverDetail> GetUnreadHandoverDetails(Member reader)
        {
            List<HandoverDetailReader> unreadDetailReaders = _dbContext.HandoverDetailReaders.Where(dr => dr.UserId == reader.UserId && dr.IsRead != true).ToList();
            List<string> handoverDetailIdList = unreadDetailReaders.Select(udr => udr.HandoverDetailId).ToList();
            List<HandoverDetail> unreadDetail = GetHandoverDetailByDetailIds(handoverDetailIdList);
            return unreadDetail.Where(ud => ud.IsActive == true).ToList();
        }

        public List<FileDetailInfo> GetFileDetailInfos(List<string> fileAttIds)
        {
            return _dbContext.FileDetailInfos.Where(fi => fileAttIds.Contains(fi.AttId)).ToList();
        }
    }
}
