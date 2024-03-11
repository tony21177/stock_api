using AutoMapper;
using stock_api.Common;
using stock_api.Controllers.Dto;
using stock_api.Models;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly HandoverService _handoverService;
        private readonly MemberService _memberService;
        private readonly AnnouncementService _announcementService;

        public DashboardController(IMapper mapper, ILogger<DashboardController> logger, AuthHelpers authHelpers, HandoverService handoverService, MemberService memberService, AnnouncementService announcementService)
        {
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _handoverService = handoverService;
            _memberService = memberService;
            _announcementService = announcementService;
        }

        [HttpGet("unread/my")]
        [Authorize]
        public IActionResult GetMyHandoverDetail()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            Member reader = memberAndPermissionSetting.Member;
            var handoverDetailDtoList = _handoverService.GetMyHandoverDetailDtoList(reader.UserId);
            List<HandoverDetail> unreadHandoverDetails = _handoverService.GetUnreadHandoverDetails(reader);
            List<Announcement> unreadAnnoucements = _announcementService.GetUnreadAnnouncement(reader);

            UnreadDto unreadDto = new UnreadDto
            {
                UnreadAnnouncementCount = unreadAnnoucements.Count,
                UnreadHandoverCount = unreadHandoverDetails.Count,
                UnreadAnnouncements = unreadAnnoucements,
                UnreadHandoverDetails = unreadHandoverDetails,
            };

            return Ok(new CommonResponse<UnreadDto>
            {
                Result = true,
                Data = unreadDto
            });
        }
    }
}
