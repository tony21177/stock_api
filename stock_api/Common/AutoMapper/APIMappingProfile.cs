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

            // purchase
            CreateMap<PurchaseMainSheet, PurchaseMainAndSubItemVo>()
           .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src =>
               src.GroupIds != null ? src.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null));
            CreateMap<PurchaseSubItem, PurchaseSubItemVo>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src =>
                src.GroupIds != null ? src.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null))
            .ForMember(dest => dest.GroupNames, opt => opt.MapFrom(src =>
                src.GroupNames != null ? src.GroupNames.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null));

            // product
            CreateMap<UpdateProductRequest, WarehouseProduct>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src => string.Join(",", src.GroupIds)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AdminUpdateProductRequest, WarehouseProduct>()
            .ForMember(dest => dest.GroupIds, opt => opt.MapFrom(src => string.Join(",", src.GroupIds)))
            .ForMember(dest => dest.LastAbleDate, opt => opt.MapFrom(src =>DateOnly.FromDateTime(DateTimeHelper.ParseDateString(src.LastAbleDate).Value)))
            .ForMember(dest => dest.LastOutStockDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTimeHelper.ParseDateString(src.LastOutStockDate).Value)))
            .ForMember(dest => dest.OriginalDeadline, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTimeHelper.ParseDateString(src.OriginalDeadline).Value)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<WarehouseProduct, WarehouseProduct>()
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
        }
        
    }
}
