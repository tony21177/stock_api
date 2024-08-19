using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseRequest>
    {
        private readonly WarehouseProductService _warehouseProductService;
        private readonly GroupService _groupService;
        private readonly PurchaseService _purchaseService;

        public CreatePurchaseValidator(WarehouseProductService warehouseProductService, GroupService groupService,PurchaseService purchaseService)
        {
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;
            _purchaseService = purchaseService;

            ClassLevelCascadeMode = CascadeMode.Stop;
            //RuleFor(x => x.GroupIds)
            //    .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
            //    .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
            
            RuleFor(x => x.DemandDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.PurchaseSubItems).NotEmpty().WithMessage("purchaseSubItems不可為空");

            RuleFor(x => x)
                .Must((request, x, context) => BeValidProductList(request, context))
                .WithMessage("以下 productId 為無效的 product: {InvalidProdcutIds}");

            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.PurchaseType.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.Type)) // Only validate when Type is not empty
                    .WithMessage($"type必須為{string.Join(",", CommonConstants.PurchaseType.GetAllValues())}");

            RuleForEach(request => request.PurchaseSubItems).SetValidator(new SubItemValidator(_purchaseService));

        }


        private bool BeValidGroupList(List<string> groupIds, ValidationContext<CreatePurchaseRequest> context)
        {
            if (groupIds == null || groupIds.Count == 0)
            {
                return true; // 允許空的 groupIds
            }

            var groupList = _groupService.GetGroupsByIdList(groupIds);
            var activeGroupList = groupList.Where(g => g.IsActive == true).ToList();

            var notExistGroupIds = groupIds.Except(activeGroupList.Select(m => m.GroupId)).ToList();

            if (notExistGroupIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistGroupIds)}";
                context.MessageFormatter.AppendArgument("InvalidGroupIds", errorMessage);
                return false;
            }
            return true;
        }

        private bool BeValidProductList(CreatePurchaseRequest request, ValidationContext<CreatePurchaseRequest> context)
        {
           var productIs = request.PurchaseSubItems.Select(x => x.ProductId).ToList();

            var productList = _warehouseProductService.GetProductsByProductIdsAndCompId(productIs, request.CompId);
            var activeProductList = productList.Where(p => p.IsActive == true).ToList();

            var notExistProductIds = productIs.Except(activeProductList.Select(m => m.ProductId)).ToList();

            if (notExistProductIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistProductIds)}";
                context.MessageFormatter.AppendArgument("InvalidProdcutIds", errorMessage);
                return false;
            }
            return true;
        }

        private static bool BeValidDate(string? date, ValidationContext<CreatePurchaseRequest> context)
        {
            if (date == null)
            {
                return true; 
            }

            if(DateTimeHelper.ParseDateString(date)!=null) return true;
            return false;
        }
    }

    public class SubItemValidator : AbstractValidator<SubItem>
    {
        private readonly PurchaseService _purchaseService;

        public SubItemValidator(PurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
            RuleFor(x=>x)
                .Must((subItem, x, context) => BeValidWithSubItem(subItem, context))
                .When(subItem => subItem.WithPurchaseMainId!=null&&subItem.WithItemId!=null)
                .WithMessage("對應的withSubItem(待拆單採購品項)不存在");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("採購數量必須大於0");

        }

        private bool BeValidWithSubItem(SubItem subItem, ValidationContext<SubItem> context)
        {
            return (_purchaseService.GetPurchaseSubItemByItemId(subItem.WithItemId)!=null);
        }
    }
}
