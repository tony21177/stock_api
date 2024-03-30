using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Controllers.Dto;
using stock_api.Service.ValueObject;
using System.Globalization;

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
            //.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => string.Join(",", src.PhotoUrls)))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrls))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WarehouseMember, MemberDto>()
            //.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
            //    src.PhotoUrl != null ? src.PhotoUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null))
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

        }


        public static DateTime? ParseDateString(string? dateString)
        {
            CultureInfo culture = new("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (DateTime.TryParseExact(dateString, "yyy/M/dd", culture, DateTimeStyles.None, out DateTime result)
        || DateTime.TryParseExact(dateString, "yyy/MM/dd", culture, DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }

        public static DateTime? ParseDateTimeFromUnixTime(long? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(dateTime.Value).UtcDateTime;
            }

            return null;
        }

        public static long ConvertToUnixTimestamp(DateTime? dateTime)
        {
            if (dateTime == null) return 0;
            DateTimeOffset dateTimeOffset = new DateTimeOffset((DateTime)dateTime);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        public static string? FormatDateString(DateTime? dateTime)
        {
            CultureInfo culture = new("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (dateTime.HasValue)
            {
                return dateTime.Value.ToString("yyy/MM/dd", culture);
            }
            return null;
        }
    }
}
