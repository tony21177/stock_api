using AutoMapper;
using FluentValidation;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using MaiBackend.PublicApi.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnoucementController : Controller
    {
        private readonly AuthHelpers _authHelpers;
        private readonly MemberService _memberService;
        private readonly AnnouncementService _announcementService;
        private readonly IMapper _mapper;
        private readonly ILogger<AnnoucementController> _logger;
        private readonly IValidator<CreateAnnoucementRequest> _createAnnoucementRequestValidator;
        private readonly IValidator<CreateAnnoucementRequest> _updateAnnoucementRequestValidator;
        private readonly IValidator<ListAnnoucementRequest> _listAnnoucementRequestValidator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly FileUploadService _fileUploadService;

        public AnnoucementController(AuthHelpers authHelpers, MemberService memberService, AnnouncementService announcementService, IMapper mapper, ILogger<AnnoucementController> logger, IWebHostEnvironment webHostEnvironment, FileUploadService fileUploadService)
        {
            _authHelpers = authHelpers;
            _memberService = memberService;
            _announcementService = announcementService;
            _mapper = mapper;
            _logger = logger;
            _createAnnoucementRequestValidator = new CreateOrUpdateAnnouncementValidator(ActionTypeEnum.Create, memberService);
            _updateAnnoucementRequestValidator = new CreateOrUpdateAnnouncementValidator(ActionTypeEnum.Create, memberService);
            _listAnnoucementRequestValidator = new ListAnnouncementRequestValidator();
            _webHostEnvironment = webHostEnvironment;
            _fileUploadService = fileUploadService;
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult Create(CreateAnnoucementRequest createAnnoucementRequest)
        {
            var response = new CommonResponse<Announcement>
            {
                Result = true,
                Message = "",
                Data = null
            };
            // 權限控管
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting == null) return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            var permisionSetting = memberAndPermissionSetting.PermissionSetting;
            if (permisionSetting.IsCreateAnnouce == false)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            // 參數驗證
            var validationResult = _createAnnoucementRequestValidator.Validate(createAnnoucementRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            // 業務邏輯
            var newAnnouncement = _mapper.Map<Announcement>(createAnnoucementRequest);

            if (!createAnnoucementRequest.ReaderUserIdList.Contains(memberAndPermissionSetting.Member.UserId))
            {
                createAnnoucementRequest.ReaderUserIdList.Add(memberAndPermissionSetting.Member.UserId);
            }

            newAnnouncement = _announcementService.CreateAnnouncement(newAnnouncement, createAnnoucementRequest.ReaderUserIdList, memberAndPermissionSetting.Member, createAnnoucementRequest.AttIdList);
            if (newAnnouncement == null)
            {
                response.Result = false;
                response.Message = "創建公告失敗";
                return Ok(response);
            }
            response.Result = true;
            response.Data = newAnnouncement;

            return Ok(response);
        }

        [HttpDelete("attachment/{attid}")]
        [AuthorizeRoles("1", "3", "5")]
        public IActionResult DeleteAnnouceAttachment(string attid)
        {
            _announcementService.DeleteAttachmentByAttIds(new List<string> { attid });
            return Ok(new CommonResponse<dynamic>()
            {
                Result = true,
            });
        }


        [HttpPost("list")]
        [Authorize]
        public IActionResult ListAnnouncements(ListAnnoucementRequest listAnnoucementRequest)
        {
            //if (User.Identity == null || !User.Identity.IsAuthenticated)
            //{
            //    return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            //}
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;


            // 參數驗證
            var validationResult = _listAnnoucementRequestValidator.Validate(listAnnoucementRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var announcements = _announcementService.GetFilteredAnnouncements(listAnnoucementRequest);
            // Extract unique AnnounceIds from the announcements
            var uniqueAnnounceIds = announcements.Select(a => a.AnnounceId).Distinct().ToList();

            // Retrieve attachments based on the unique AnnounceIds
            var attachments = _announcementService.GetAttachmentsByAnnounceIds(uniqueAnnounceIds);

            var announceReaderList = _announcementService.GetAnnouceReadersByUserIds(new List<string> { userId });
            // Map attachments to corresponding announcements
            var result = announcements.Select(announcement =>
            {
                var announceAttachments = attachments
                    .Where(a => a.AnnounceId == announcement.AnnounceId)
                    .ToList();
                var matchedAnnouce = announceReaderList.Find(announceReader => announcement.AnnounceId == announceReader.AnnounceId);

                return new AnnouncementWithAttachments
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    BeginPublishTime = announcement.BeginPublishTime,
                    EndPublishTime = announcement.EndPublishTime,
                    BeginViewTime = announcement.BeginViewTime,
                    EndViewTime = announcement.EndViewTime,
                    IsActive = announcement.IsActive ?? true,
                    AnnounceId = announcement.AnnounceId,
                    CreatorId = announcement.CreatorId,
                    CreatorName = announcement.CreatorName,
                    CreatedTime = announcement.CreatedTime,
                    UpdatedTime = announcement.UpdatedTime,
                    AnnounceAttachments = announceAttachments,
                    IsRead = matchedAnnouce == null || matchedAnnouce.IsRead, // 表示此篇對於查詢者(現在登入的member)是否為已讀或未讀,若不在在收件人中給true
                };
            }).ToList();

            return Ok(new CommonResponse<List<AnnouncementWithAttachments>>
            {
                Result = true,
                Data = result
            });
        }

        [HttpGet("detail/{announceId}")]
        [Authorize]
        public IActionResult GetAnnouncementDetail(string announceId)
        {
            //if (User.Identity == null || !User.Identity.IsAuthenticated)
            //{
            //    return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            //}
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;


            var announcement = _announcementService.GetAnnouncementByAnnounceId(announceId);
            if (announcement == null)
            {
                return Ok(new CommonResponse<AnnouncementWithAttachments>
                {
                    Result = true
                });
            }

            // Retrieve attachments based on the unique AnnounceIds
            var attachments = _announcementService.GetAttachmentsByAnnounceIds(new List<string> { announceId });

            List<AnnouceReader> annouceReaders = _announcementService.GetAnnouceReaderByAnnouncementId(announceId);
            var readerUserIdList = annouceReaders.Select(ar => ar.UserId).ToList();
            var readersMembeList = _memberService.GetActiveMembersByUserIds(readerUserIdList);
            var readersMemberDtoList = GetAnnounceReaderMemberDtoList(announceId);
            // 更新成已讀
            var announceReaders = _announcementService.UpdateAnnounceReaderToRead(announceId, userId);
            var isRead = (announceReaders == null || announceReaders.IsRead);

            var result = new AnnouncementWithAttachmentsReaders
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                BeginPublishTime = announcement.BeginPublishTime,
                EndPublishTime = announcement.EndPublishTime,
                BeginViewTime = announcement.BeginViewTime,
                EndViewTime = announcement.EndViewTime,
                IsActive = announcement.IsActive ?? true,
                AnnounceId = announcement.AnnounceId,
                CreatorId = announcement.CreatorId,
                CreatorName = announcement.CreatorName,
                CreatedTime = announcement.CreatedTime,
                UpdatedTime = announcement.UpdatedTime,
                AnnounceAttachments = attachments,
                ReaderUserList = readersMemberDtoList,
                IsRead = isRead, // 表示此篇對於查詢者(現在登入的member)是否為已讀或未讀,若不在在收件人中給true
            };
            return Ok(new CommonResponse<AnnouncementWithAttachmentsReaders>
            {
                Result = true,
                Data = result
            });
        }
        [HttpGet("history/{announceId}")]
        [Authorize]
        public IActionResult GetAnnouncementHistory(string announceId)
        {

            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;


            var announcement = _announcementService.GetAnnouncementByAnnounceId(announceId);
            if (announcement == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此公告不存在"
                });
            }

            // Retrieve attachments based on the unique AnnounceIds
            var attachments = _announcementService.GetAttachmentsByAnnounceIds(new List<string> { announceId });

            List<AnnouceReader> annouceReaders = _announcementService.GetAnnouceReaderByAnnouncementId(announceId);
            List<string> readerUserDisplayNameList = _memberService.GetDisplayNameByUserIdList(annouceReaders.Select(r => r.UserId).ToList());

            List<AnnouncementHistory> announcementHistories = _announcementService.GetAnnouncementHistoriesByAnnounceId(announceId);
            List<AnnouncementHistoryDetail> announcementHistoryDetails = new List<AnnouncementHistoryDetail>();
            announcementHistories.ForEach(history =>
            {
                List<string> oldAttIdList = history.OldAttId.Split(",").ToList();
                List<AnnounceAttachment> oldAttachments = _announcementService.GetAnnounceAttachmentsByAttIds(oldAttIdList);

                List<string> newAttIdList = history.NewAttId.Split(",").ToList();
                List<AnnounceAttachment> newAttachments = _announcementService.GetAnnounceAttachmentsByAttIds(newAttIdList);

                List<string> oldReaderUserIdList = history.OldReaderUserIdList.Split(",").ToList();
                List<string> oldReaderDisplayNameList = _memberService.GetDisplayNameByUserIdList(oldReaderUserIdList);

                List<string> newReaderUserIdList = history.NewReaderUserIdList.Split(",").ToList();
                List<string> newReaderDisplayNameList = _memberService.GetDisplayNameByUserIdList(newReaderUserIdList);

                AnnouncementHistoryDetail historyDeatil = _mapper.Map<AnnouncementHistoryDetail>(history);
                historyDeatil.OldAttachmentList = oldAttachments;
                historyDeatil.NewAttachmentList = newAttachments;
                historyDeatil.OldReaderNames = string.Join(",", oldReaderDisplayNameList);
                historyDeatil.NewReaderNames = string.Join(",", newReaderDisplayNameList);
                announcementHistoryDetails.Add(historyDeatil);
            });
            return Ok(new CommonResponse<List<AnnouncementHistoryDetail>>
            {
                Result = true,
                Data = announcementHistoryDetails
            });
        }

        private List<AnnouceReaderMemberDto> GetAnnounceReaderMemberDtoList(String announceId)
        {
            List<AnnouceReader> annouceReaders = _announcementService.GetAnnouceReaderByAnnouncementId(announceId);
            var readerUserIdList = annouceReaders.Select(ar => ar.UserId).ToList();
            var readersMembeList = _memberService.GetActiveMembersByUserIds(readerUserIdList);
            var announceReadersMemberDtoList = _mapper.Map<List<AnnouceReaderMemberDto>>(readersMembeList);
            announceReadersMemberDtoList.ForEach(dto =>
            {
                var matchedAnnounceReader = annouceReaders.Find(annouceReader => annouceReader.UserId == dto.UserId);
                dto.IsRead = matchedAnnounceReader.IsRead;
                dto.ReadTime = matchedAnnounceReader.ReadTime;
            });
            return announceReadersMemberDtoList;
        }

        [HttpPost("update/{announceId}")]
        [Authorize]
        public IActionResult UpdateAnnouncement(UpdateAnnouncementRequest updateAnnouncementRequest, [FromRoute] string announceId)
        {
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;
            if (!loginMemberAndPermission.PermissionSetting.IsUpdateAnnouce)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var originalAnnouncement = _announcementService.GetAnnouncementByAnnounceId(announceId);
            if (originalAnnouncement == null)
            {
                return BadRequest(new CommonResponse<AnnouncementWithAttachments>
                {
                    Result = false,
                    Message = "此公告不存在"
                });
            }
            var announceReaders = _announcementService.GetAnnouceReaderByAnnouncementId(announceId);
            var attachments = _announcementService.GetAttachmentsByAnnounceIds(new List<string> { announceId });
            var myAnnouncements = _announcementService.GetMyAnnouncements(announceId);
            var newAnnouncement = _mapper.Map<Announcement>(updateAnnouncementRequest);
            var result = _announcementService.UpdateAnnouncement(announceId, newAnnouncement, originalAnnouncement, announceReaders, updateAnnouncementRequest, attachments, myAnnouncements);
            if (result == false)
            {
                return StatusCode(500, new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "內部錯誤"
                });
            }

            attachments = _announcementService.GetAttachmentsByAnnounceIds(new List<string> { announceId });

            //var result = new AnnouncementWithAttachments
            //{
            //    Id = announcement.Id,
            //    Title = announcement.Title,
            //    Content = announcement.Content,
            //    BeginPublishTime = announcement.BeginPublishTime,
            //    EndPublishTime = announcement.EndPublishTime,
            //    BeginViewTime = announcement.BeginViewTime,
            //    EndViewTime = announcement.EndViewTime,
            //    IsActive = announcement.IsActive ?? false,
            //    AnnounceId = announcement.AnnounceId,
            //    CreatorId = announcement.CreatorId,
            //    CreatorName = announcement.CreatorName,
            //    CreatedTime = announcement.CreatedTime,
            //    UpdatedTime = announcement.UpdatedTime,
            //    AnnounceAttachments = attachments,
            //};
            return Ok(new CommonResponse<AnnouncementWithAttachments>
            {
                Result = true,
                Data = null
            });
        }

        [HttpDelete("{announceId}")]
        [Authorize]
        public IActionResult DeleteAnnouncement(string announceId)
        {
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;
            if (!loginMemberAndPermission.PermissionSetting.IsDeleteAnnouce)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var announcement = _announcementService.GetAnnouncementByAnnounceId(announceId);
            if (announcement == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "公告不存在"
                });
            }
            var result = _announcementService.InActiveByAnnounceId(announceId);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

        [HttpGet("myAnnouncement")]
        [Authorize]
        public IActionResult GetMyAnnouncement()
        {
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;
            var myAnnouncements = _announcementService.GetMyAnnouncementsByUserId(userId);
            List<MyAnnouncementWithAttachmentsDto> myAnnouncementsWithAttachList = new List<MyAnnouncementWithAttachmentsDto>();
            myAnnouncements.ForEach(myAnnouncement =>
            {
                var myAnnouncementsWithAttachDto = _mapper.Map<MyAnnouncementWithAttachmentsDto>(myAnnouncement);
                // Retrieve attachments based on the unique AnnounceIds
                var attachments = _announcementService.GetAttachmentsByAnnounceIds(new List<string> { myAnnouncement.AnnounceId });
                myAnnouncementsWithAttachDto.AnnounceAttachments = attachments;
                // creator對於目前這篇公告是已讀還是未讀
                var creatorId = myAnnouncement.CreatorId;
                var creator = _memberService.GetMemberByUserId(creatorId);
                myAnnouncementsWithAttachDto.CreatorName = creator?.DisplayName;

                var isRead = _announcementService.IsUserReadAnnouncement(myAnnouncement.AnnounceId, loginMemberAndPermission.Member.UserId);
                myAnnouncementsWithAttachDto.IsRead = isRead;
                myAnnouncementsWithAttachList.Add(myAnnouncementsWithAttachDto);
            });

            return Ok(new CommonResponse<List<MyAnnouncementWithAttachmentsDto>>
            {
                Result = true,
                Data = myAnnouncementsWithAttachList
            });
        }

        [HttpPost("myAnnouncement/update/{announceId}")]
        [Authorize]
        public IActionResult UpdateMyAnnouncement(UpdateMyAnnouncementRequest request, string announceId)
        {
            var loginMemberAndPermission = _authHelpers.GetMemberAndPermissionSetting(User);
            var userId = loginMemberAndPermission!.Member.UserId;
            var myAnnouncement = _announcementService.GetMyAnnouncements(announceId, userId);
            if (myAnnouncement == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "不存在"
                });
            }
            var result = _announcementService.UpdateMyAnnouncements(myAnnouncement.Id, request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result
            });
        }

        [HttpPost("Attachment/upload")]
        [AuthorizeRoles("1", "3", "5")]
        public async Task<IActionResult> UploadAttatchment([FromForm] UploadFilesRequest uploadAnnouncementAttachmentRequest)
        {
            var fileDetails = await _fileUploadService.PostFilesAsync(uploadAnnouncementAttachmentRequest.Files, new List<string> { "announcement" });
            List<AnnounceAttachment> announceAttachments = new List<AnnounceAttachment>();
            if (fileDetails.Count > 0)
            {
                announceAttachments = _announcementService.AnnounceAttachments(fileDetails);
            }

            return Ok(new CommonResponse<List<AnnounceAttachment>>
            {
                Result = true,
                Data = announceAttachments
            });
        }

        [HttpGet("Attachment/download/{attId}")]
        [Authorize]
        public IActionResult DownloadAttatchment(string attId)
        {
            var attachment = _announcementService.GetAttachment(attId);
            if (attachment == null)
            {
                return NotFound();
            }
            FileDetail fileDetail = new()
            { 
                AttId =attachment.AttId,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                FileSizeText = attachment.FileSizeText,
                FileType = attachment.FileType,
            };
            var fileStream = _fileUploadService.Download(fileDetail);
            return fileStream;
        }

    }
}
