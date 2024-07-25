using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Controllers.Dto;
using stock_api.Service.ValueObject;
using System.Globalization;
using stock_api.Common.Utils;

namespace stock_api.Common.AutoMapper
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            // member
            CreateMap<CreateAuthlayerRequest, WarehouseAuthlayer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateAuthlayerRequest, WarehouseAuthlayer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CreateOrUpdateMemberRequest, WarehouseMember>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src => string.Join(",", src.GroupIds)))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrls))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseMember, MemberDto>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src =>
                src.GroupIds != null ? src.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseMember, WarehouseMember>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // company
            CreateMap<CreateCompanyRequest, Company>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateCompanyRequest, Company>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //CreateMap<Company, Company>()
            //    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // group
            CreateMap<CreateGroupRequest, WarehouseGroup>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateGroupRequest, WarehouseGroup>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // manufacturer
            CreateMap<CreateManufacturerRequest, Manufacturer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateManufacturerRequest, Manufacturer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // supplier
            CreateMap<CreateSupplierRequest, Supplier>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateSupplierRequest, Supplier>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // purchaseFlowSetting
            CreateMap<CreateOrUpdatePurchaseFlowSettingRequest, PurchaseFlowSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PurchaseFlowSetting, PurchaseFlowSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PurchaseFlowSetting, PurchaseFlowSettingVo>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ApplyProductFlowSetting
            CreateMap<ApplyProductFlowSettingRequest, ApplyProductFlowSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ApplyProductFlowSetting, ApplyProductFlowSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ApplyProductFlowSetting, ApplyProductFlowSettingVo>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ApplyNewProductMain
            CreateMap<ApplyNewProductMain, ApplyNewProductMainWithFlowVo>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // purchase
            CreateMap<PurchaseMainSheet, PurchaseMainAndSubItemVo>()
           .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src =>
               src.GroupIds != null ? src.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null));
            CreateMap<PurchaseSubItem, PurchaseSubItemVo>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src =>
                src.GroupIds != null ? src.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null))
            .ForMember(dest => dest.GroupNames, opt => opt.MapFrom(src =>
                src.GroupNames != null ? src.GroupNames.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null));
            CreateMap<PurchaseSubItem, UnDonePurchaseSubItem>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            // product
            CreateMap<UpdateProductRequest, WarehouseProduct>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src => string.Join(",", src.GroupIds)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AdminUpdateProductRequest, WarehouseProduct>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src => src.GroupIds != null ? string.Join(",", src.GroupIds):null))
            .ForMember(dest => dest.LastAbleDate, opt => opt.MapFrom(src =>
        src.LastAbleDate != null ? DateTimeHelper.ParseDateString(src.LastAbleDate) : (DateTime?)null))
            .ForMember(dest => dest.LastOutStockDate, opt => opt.MapFrom(src =>
        src.LastOutStockDate != null ? DateTimeHelper.ParseDateString(src.LastOutStockDate) : (DateTime?)null))
    .ForMember(dest => dest.OriginalDeadline, opt => opt.MapFrom(src =>
        src.OriginalDeadline != null ? DateTimeHelper.ParseDateString(src.OriginalDeadline) : (DateTime?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<WarehouseProduct, WarehouseProduct>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseProduct, WarehouseProductVo>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseProduct, NotEnoughQuantityProduct>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseProduct, NearExpiredProductVo>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // stockin & accept
            CreateMap<PurchaseAcceptanceItemsView, AcceptanceItem>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.AcceptCreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.AcceptUpdatedAt))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PurchaseAcceptanceItemsView, AcceptItem>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PurchaseAcceptanceItemsView, PurchaseAcceptItemsVo>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<InStockItemRecord, InStockRecordForPrint>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // OutStockRecord
            CreateMap<OutStockRecord, OutStockRecordVo>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // SupplierTrace
            CreateMap<ManualCreateSupplierTraceLogRequest, SupplierTraceLog>()
                .ForMember(dest => dest.AbnormalDate, opt => opt.MapFrom(src =>
                    src.AbnormalDate != null ? DateTimeHelper.ParseDateString(src.AbnormalDate) : (DateTime?)null));
            CreateMap<ManualUpdateSupplierTraceLogRequest, SupplierTraceLog>()
                .ForMember(dest => dest.AbnormalDate, opt => opt.MapFrom(src =>
                    src.AbnormalDate != null ? DateTimeHelper.ParseDateString(src.AbnormalDate) : (DateTime?)null));
            CreateMap<SupplierTraceLog, SupplierTraceLog>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SupplierTraceLog, SupplierTraceLogWithInStock>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


        }
        
    }
}
