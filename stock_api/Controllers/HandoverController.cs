using AutoMapper;
using FluentValidation;
using stock_api.Common;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using MaiBackend.Common.AutoMapper;
using MaiBackend.PublicApi.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HandoverController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<HandoverController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly HandoverService _handoverService;
        private readonly MemberService _memberService;
        private readonly FileUploadService _fileUploadService;
        private readonly CreateHandoverDetailRequestValidator _createHandoverDetailRequestValidator;
        private readonly UpdateHandoverDetailRequestValidator _updateHandoverDetailRequestValidator;

        public HandoverController(IMapper mapper, ILogger<HandoverController> logger, AuthHelpers authHelpers, HandoverService handoverService, MemberService memberService, FileUploadService fileUploadService)
        {
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _handoverService = handoverService;
            _memberService = memberService;
            _createHandoverDetailRequestValidator = new CreateHandoverDetailRequestValidator(ActionTypeEnum.Create, _memberService);
            _updateHandoverDetailRequestValidator = new UpdateHandoverDetailRequestValidator(_memberService);
            _fileUploadService = fileUploadService;
        }


        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateHandover(CreateHandoverDetailRequest createHandoverDetailRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            Member creatorMember = memberAndPermissionSetting.Member;
            if (permissionSetting == null || !permissionSetting.IsCreateHandover)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            // 參數驗證
            var validationResult = _createHandoverDetailRequestValidator.Validate(createHandoverDetailRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            // 驗證是否同屬一個main
            var sheetRowIdList = createHandoverDetailRequest.RowDetails.Select(rd => rd.SheetRowId).ToList();
            var matchedSheetMainSettings = _handoverService.GetSheetMainListBySheetRowIdList(sheetRowIdList);
            var matchedSheetMainIdList = matchedSheetMainSettings.Select(main => main.SheetId).ToList();
            bool isTheSameMainId = matchedSheetMainSettings.Distinct().Count() == 1;
            if (!isTheSameMainId)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "不可跨交班main setting"
                });
            }
            if (matchedSheetMainSettings[0].IsActive == null || matchedSheetMainSettings[0].IsActive == false)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此交班表設定為失效狀態"
                });
            }

            List<HandoverSheetRowWithGroup> neededSheetRowWithGroup = _handoverService.GetSheetRowsByMainSheetId(matchedSheetMainIdList[0]).Where(row => row.IsActive == true && row.IsGroupActive == true).ToList();
            var neededSheetRowCount = neededSheetRowWithGroup.Count;
            if (neededSheetRowCount != createHandoverDetailRequest.RowDetails.Count)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"交班單的row筆數:{createHandoverDetailRequest.RowDetails.Count}不對,需要{neededSheetRowCount}筆"
                });
            }

            List<Member> readerMemberList = _memberService.GetMembersByUserIdList(createHandoverDetailRequest.ReaderUserIds);

            if (readerMemberList.Find(m => m.UserId == creatorMember.UserId) == null)
            {
                readerMemberList.Add(creatorMember);
            }

            var createdJsonContent = _handoverService.CreateHandOverDetail(matchedSheetMainIdList[0], createHandoverDetailRequest.RowDetails, createHandoverDetailRequest.Title, createHandoverDetailRequest.Content, readerMemberList, creatorMember, createHandoverDetailRequest.FileAttIds);

            return Ok(new CommonResponse<string?>
            {
                Result = createdJsonContent != null,
                Data = createdJsonContent,
            });
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateHandover(UpdateHandoverDetailRequest updateHandoverDetailRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            Member creatorMember = memberAndPermissionSetting.Member;
            if (permissionSetting == null || !permissionSetting.IsUpdateHandover)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            // 參數驗證
            var validationResult = _updateHandoverDetailRequestValidator.Validate(updateHandoverDetailRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var handoverDetail = _handoverService.GetHandoverDetail(updateHandoverDetailRequest.HandoverDetailId);
            if (handoverDetail == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此交班表不存在",
                });
            }

            List<Member> readerMemberList = _memberService.GetMembersByUserIdList(updateHandoverDetailRequest.ReaderUserIds);

            var updatedJsonContent = _handoverService.UpdateHandover(handoverDetail, updateHandoverDetailRequest.RowDetails, updateHandoverDetailRequest.Title,
                updateHandoverDetailRequest.Content, readerMemberList, updateHandoverDetailRequest.FileAttIds);

            return Ok(new CommonResponse<string?>
            {
                Result = updatedJsonContent != null,
                Data = updatedJsonContent,
            });
        }

        [HttpDelete("{handoverDetailId}")]
        [Authorize]
        public IActionResult InactiveHandoverDetail(string handoverDetailId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            if (permissionSetting == null || !permissionSetting.IsUpdateHandover)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }


            var handoverDetail = _handoverService.GetHandoverDetail(handoverDetailId);
            if (handoverDetail == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此交班表不存在",
                });
            }

            var result = _handoverService.InActiveHandoverDetail(handoverDetail);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }


        [HttpPost("search")]
        [Authorize]
        public IActionResult SearchHandoverDetails(SearchHandoverDetailRequest searchHandoverDetailRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            Member loginMember = memberAndPermissionSetting.Member;

            if ((searchHandoverDetailRequest.StartDate != null && !Regex.IsMatch(searchHandoverDetailRequest.StartDate, @"^\d{3}/\d{2}/\d{2}$"))
                || (searchHandoverDetailRequest.EndDate != null && !Regex.IsMatch(searchHandoverDetailRequest.EndDate, @"^\d{3}/\d{2}/\d{2}$")))
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "時間格式必需為yyy/mm/dd",
                });
            }
            var startDate = searchHandoverDetailRequest.StartDate != null ? APIMappingProfile.ParseDateString(searchHandoverDetailRequest.StartDate) : null;
            var endDate = searchHandoverDetailRequest.EndDate != null ? APIMappingProfile.ParseDateString(searchHandoverDetailRequest.EndDate) : null;
            endDate = endDate?.AddDays(1);
            List<HandoverDetail> handoverDetailList = _handoverService.SearchHandoverDetails(searchHandoverDetailRequest.MainSheetId, startDate, endDate,
                searchHandoverDetailRequest.PaginationCondition, searchHandoverDetailRequest.SearchString);

            List<HandoverDetailWithReadDto> handoverDetailWithReadDtoList = _mapper.Map<List<HandoverDetailWithReadDto>>(handoverDetailList);

            var handoverReaderList = _handoverService.GetHandoverDetailReadersByUserId(loginMember.UserId);

            List<string> fileAttIdsList = handoverDetailList.Select(hd => hd.FileAttIds).ToList();
            HashSet<string> allDistinctfileAttIds = new();
            fileAttIdsList.ForEach(fileAttIdsString =>
            {
                var fileAttIdsList = fileAttIdsString.Split(",");
                foreach (var fileAttId in fileAttIdsList)
                {
                    allDistinctfileAttIds.Add(fileAttId);
                }
            });
            List<FileDetailInfo> fileDetailInfos = _handoverService.GetFileDetailInfos(allDistinctfileAttIds.ToList());



            handoverDetailWithReadDtoList.ForEach(dto =>
            {
                var matchedReader = handoverReaderList.Find(reader => reader.HandoverDetailId == dto.HandoverDetailId);
                if (matchedReader != null)
                {
                    dto.IsRead = matchedReader.IsRead;
                }
                else
                {
                    // 如果不在 readUser 名單內，視為已讀
                    dto.IsRead = true;
                }
                var fileAttIdList = dto.FileAttIds.Split(",");
                List<FileDetailInfo> matchedFiles = fileDetailInfos.Where(fdi => fileAttIdList.Contains(fdi.AttId)).ToList();
                dto.Files = matchedFiles;
            });


            return Ok(new CommonResponse<List<HandoverDetailWithReadDto>>
            {
                Result = true,
                Data = handoverDetailWithReadDtoList
            });
        }

        [HttpGet("detail/{handoverDetailId}")]
        [Authorize]
        public IActionResult ReadHandoverDetail(string handoverDetailId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            Member reader = memberAndPermissionSetting.Member;
            var handoverDetail = _handoverService.GetHandoverDetailByDetailId(handoverDetailId);
            if (handoverDetail == null)
            {
                return BadRequest(new CommonResponse<HandoverDetail>
                {
                    Result = false,
                    Message = "此交班表不存在",
                });
            }
            HandoverDetailWithReaders handoverDetailWithReaders = _mapper.Map<HandoverDetailWithReaders>(handoverDetail);

            if (!string.IsNullOrEmpty(handoverDetail.FileAttIds))
            {
                List<string> fileAttIds = handoverDetail.FileAttIds.Split(",").ToList();
                List<FileDetailInfo> fileDetailInfos = _handoverService.GetFileDetailInfos(fileAttIds);
                handoverDetailWithReaders.Files = fileDetailInfos;
            }

            var result = _handoverService.ReadHandoverDetail(handoverDetailId, reader.UserId);

            var handoverReaders = _handoverService.GetHandoverDetailReadersByDetailId(handoverDetailId);
            var handoverReaderDtoList = _mapper.Map<List<HandoverDetailReaderDto>>(handoverReaders);
            var readersMemberInto = _memberService.GetActiveMembersByUserIds(handoverReaders.Select(hr => hr.UserId).ToList());
            handoverReaderDtoList.ForEach(dto =>
            {
                var matchedReaderMember = readersMemberInto.Find(m => m.UserId == dto.UserId);
                dto.PhotoUrl = matchedReaderMember?.PhotoUrl;
            });

            handoverDetailWithReaders.HandoverDetailReader = handoverReaderDtoList;


            return Ok(new CommonResponse<HandoverDetailWithReaders>
            {
                Result = result,
                Data = handoverDetailWithReaders
            });
        }

        [HttpGet("detail/my")]
        [Authorize]
        public IActionResult GetMyHandoverDetail()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            Member reader = memberAndPermissionSetting.Member;
            var handoverDetailDtoList = _handoverService.GetMyHandoverDetailDtoList(reader.UserId);
            foreach (var handoverDetailDto in handoverDetailDtoList)
            {
                if (!string.IsNullOrEmpty(handoverDetailDto.FileAttIds))
                {
                    List<string> fileAttIds = handoverDetailDto.FileAttIds.Split(",").ToList();
                    List<FileDetailInfo> fileDetailInfos = _handoverService.GetFileDetailInfos(fileAttIds);
                    handoverDetailDto.Files = fileDetailInfos;
                }
            }
            return Ok(new CommonResponse<List<MyHandoverDetailDto>>
            {
                Result = true,
                Data = handoverDetailDtoList
            });
        }

        [HttpGet("histories/{handoverDetailId}")]
        [Authorize]
        public IActionResult GetHandoverDetailHistories(string handoverDetailId)
        {

            var handoverDetailHistories = _handoverService.GetHandoverDetailHistories(handoverDetailId);

            return Ok(new CommonResponse<List<HandoverDetailHistory>>
            {
                Result = true,
                Data = handoverDetailHistories
            });
        }

        [HttpPost("Files/upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile([FromForm] UploadFilesRequest uploadFilesRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;


            var fileDetails = await _fileUploadService.PostFilesAsync(uploadFilesRequest.Files, new List<string> { "handover" });
            var fileDetailInfos = _mapper.Map<List<FileDetailInfo>>(fileDetails);
            bool result = _fileUploadService.AddFileDetailInfo(fileDetailInfos);

            return Ok(new CommonResponse<List<FileDetailInfo>>
            {
                Result = result,
                Message = result ? "" : "上傳失敗",
                Data = fileDetailInfos
            });
        }
        [HttpGet("Files/{attid}")]
        public async Task<IActionResult> DownloadFile(string attid)
        {
            var fileDetail = _fileUploadService.GetFileDetail(attid);
            if (fileDetail == null)
            {
                return NotFound();
            }
            var fileStream = _fileUploadService.Download(fileDetail);
            return fileStream;
        }
    }
}
