﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Service;
using stock_api.Utils;
using stock_api.Service.ValueObject;
using stock_api.Models;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockInController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly GroupService _groupService;
        private readonly StockInService _stockInService;
        private readonly IValidator<SearchPurchaseAcceptItemRequest> _searchPurchaseAcceptItemValidator;

        public StockInController(IMapper mapper, AuthHelpers authHelpers,GroupService groupService, StockInService stockInService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _groupService = groupService;
            _stockInService = stockInService;
            _searchPurchaseAcceptItemValidator = new SearchPurchaseAcceptItemValidator(groupService);
        }

        [HttpPost("purchaseAndAcceptItems/list")]
        [Authorize]
        public IActionResult ListPurchases(SearchPurchaseAcceptItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            request.CompId = compId;
            
            var validationResult = _searchPurchaseAcceptItemValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemsViewList = _stockInService.SearchPurchaseAcceptanceItems(request);
            Dictionary<string, List<PurchaseAcceptanceItemsView>> purchaseMainIdAndAcceptionItemListMap = new Dictionary<string, List<PurchaseAcceptanceItemsView>>();
            purchaseAcceptanceItemsViewList.ForEach(item =>
            {
                if (!purchaseMainIdAndAcceptionItemListMap.ContainsKey(item.PurchaseMainId))
                {
                    purchaseMainIdAndAcceptionItemListMap[item.PurchaseMainId] = new List<PurchaseAcceptanceItemsView>();
                };
                purchaseMainIdAndAcceptionItemListMap[item.PurchaseMainId].Add(item);
            });

            List<PurchaseAcceptItemsVo> data = new List<PurchaseAcceptItemsVo>();

            foreach(var keyValuePair in purchaseMainIdAndAcceptionItemListMap)
            {
                List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemViewList = keyValuePair.Value;
                PurchaseAcceptItemsVo purchaseAcceptItemsVo = _mapper.Map<PurchaseAcceptItemsVo>(purchaseAcceptanceItemViewList[0]);
                List<AcceptItem> acceptItems = _mapper.Map<List<AcceptItem>>(purchaseAcceptanceItemViewList);
                purchaseAcceptItemsVo.AcceptItems = acceptItems;
                data.Add(purchaseAcceptItemsVo);
            }
            data = data.OrderByDescending(item => item.ApplyDate).ToList();

            var response = new CommonResponse<List<PurchaseAcceptItemsVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }
    }
}
