﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using stock_api.Controllers.Validator;
using Microsoft.IdentityModel.Tokens;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplyNewProductController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly ApplyProductService _applyProductService;
        private readonly GroupService _groupService;
        private readonly IValidator<CreateApplyProductMainRequest> _createApplyProductValidator;
        private readonly IValidator<ListApplyNewProductMainRequest> _listApplyNewProductMainRequestValidator;
        private readonly IValidator<AnswerFlowRequest> _answerFlowRequest;

        public ApplyNewProductController(IMapper mapper, AuthHelpers authHelpers, ApplyProductFlowSettingService applyProductFlowSettingService, MemberService memberService, CompanyService companyService, ApplyProductService applyProductService, GroupService groupService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _applyProductFlowSettingService = applyProductFlowSettingService;
            _memberService = memberService;
            _companyService = companyService;
            _applyProductService = applyProductService;
            _groupService = groupService;
            _createApplyProductValidator = new CreateApplyProductValidator(groupService);
            _listApplyNewProductMainRequestValidator = new ListApplyNewProductValidator(groupService);
            _answerFlowRequest = new AnswerApplyNewProductFlowValidator();
        }


        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateApplyProductMain(CreateApplyProductMainRequest createRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            createRequest.CompId = compId;

            var validationResult = _createApplyProductValidator.Validate(createRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            List<ApplyProductFlowSettingVo> applyProductFlowSettingList = _applyProductFlowSettingService.GetAllApplyProductFlowSettingsByCompId(createRequest.CompId).ToList();
            if (applyProductFlowSettingList.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "尚未建立審核流程關卡"
                });
            }

            var group = _groupService.GetGroupByGroupId(createRequest.ProductGroupId);

            var newPurchaseMain = new ApplyNewProductMain()
            {
                CompId = createRequest.CompId,
                ApplyId = Guid.NewGuid().ToString(),
                ApplyReason = createRequest.ApplyReason,
                ApplyRemarks = createRequest.ApplyRemarks, 
                ApplyProductName = createRequest.ApplyProductName,
                ApplyProductSpec = createRequest.ApplyProductSpec,
                ApplyQuantity = createRequest.ApplyQuantity,
                ProductGroupId = createRequest.ProductGroupId,
                ProductGroupName = group.GroupName,
                UserId = memberAndPermissionSetting.Member.UserId
            };
            var (result,msg) = _applyProductService.CreateApplyProductMain(newPurchaseMain, applyProductFlowSettingList);

            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Message = msg
            };
            return Ok(response);
        }

        [HttpPost("list")]
        [Authorize]
        public IActionResult ListApplyNewProductMain(ListApplyNewProductMainRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            if (request.CompId == null) request.CompId = compId;
            
            var validationResult = _listApplyNewProductMainRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var (data,total) = _applyProductService.ListApplyNewProductMain(request,true);
            

            var response = new CommonResponse<List<ApplyNewProductMainWithFlowVo>>
            {
                Result = true,
                Data = data,
                TotalPages = total
            };
            return Ok(response);
        }

        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult ListApplyNewProductMainForOwner()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            ListApplyNewProductMainRequest request = new() { CurrentStatus = CommonConstants.PurchaseFlowAnswer.AGREE };

            var (data, total) = _applyProductService.ListApplyNewProductMain(request,false);
            
            

            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
                TotalPages = total
            };
            return Ok(response);
        }

        [HttpPost("flow/answer")]
        [Authorize]
        public IActionResult FlowSign(AnswerFlowRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var verifier = memberAndPermissionSetting.Member;
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var validationResult = _answerFlowRequest.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var applyNewProductFlow = _applyProductService.GetFlowsByFlowId(request.FlowId);

            if (applyNewProductFlow == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該審核流程不存在"
                });
            }


            if (applyNewProductFlow != null && memberAndPermissionSetting.CompanyWithUnit.Type == CommonConstants.CompanyType.OWNER && request.Answer == CommonConstants.AnswerPurchaseFlow.BACK)
            {
                // 金萬林退回
                var backResult = _applyProductService.AnswerFlow(applyNewProductFlow, request.Answer, request.Reason, true);
                var backResponse = new CommonResponse<dynamic>
                {
                    Result = backResult,
                    Data = null
                };
                return Ok(backResponse);

            }

            if (applyNewProductFlow != null && applyNewProductFlow.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (applyNewProductFlow != null && applyNewProductFlow.ReviewUserId != verifier.UserId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (applyNewProductFlow != null && !applyNewProductFlow.Answer.IsNullOrEmpty())
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "不能重複審核"
                });
            }

            var beforeFlows = _applyProductService.GetBeforeFlows(applyNewProductFlow);
            if (beforeFlows.Any(f => f.Answer == CommonConstants.PurchaseFlowAnswer.EMPTY))
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "之前的審核流程還在跑"
                });
            }

            var result = _applyProductService.AnswerFlow(applyNewProductFlow, request.Answer, request.Reason, false);


            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Data = null
            };
            return Ok(response);
        }
    }
}
