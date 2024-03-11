using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Service.ValueObject;
using System.Globalization;
using Member = stock_api.Models.Member;

namespace stock_api.Common.AutoMapper
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            CreateMap<CreateAuthlayerRequest, Authlayer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateAuthlayerRequest, Authlayer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CreateOrUpdateMemberRequest, Member>()
            //.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => string.Join(",", src.PhotoUrls)))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrls))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Member, MemberDto>()
            //.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
            //    src.PhotoUrl != null ? src.PhotoUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() : null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Member, AnnouceReaderMemberDto>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Member, Member>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CreateAnnoucementRequest, Announcement>()
            .ForMember(dest => dest.BeginPublishTime, opt => opt.MapFrom(src => ParseDateString(src.BeginPublishTime)))
            .ForMember(dest => dest.EndPublishTime, opt => opt.MapFrom(src => ParseDateString(src.EndPublishTime)))
            .ForMember(dest => dest.BeginViewTime, opt => opt.MapFrom(src => ParseDateString(src.BeginViewTime)))
            .ForMember(dest => dest.EndViewTime, opt => opt.MapFrom(src => ParseDateString(src.EndViewTime)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Announcement, Announcement>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateAnnouncementRequest, Announcement>()
            .ForMember(dest => dest.BeginPublishTime, opt => opt.MapFrom(src => ParseDateString(src.BeginPublishTime)))
            .ForMember(dest => dest.EndPublishTime, opt => opt.MapFrom(src => ParseDateString(src.EndPublishTime)))
            .ForMember(dest => dest.BeginViewTime, opt => opt.MapFrom(src => ParseDateString(src.BeginViewTime)))
            .ForMember(dest => dest.EndViewTime, opt => opt.MapFrom(src => ParseDateString(src.EndViewTime)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MyAnnouncement, MyAnnouncementWithAttachmentsDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<HandoverSheetMain, SheetSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverSheetGroup, HandoverSheetGroupDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // 目標為null才複製過去
            CreateMap<HandoverSheetMain, HandoverSheetMain>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember, context) => srcMember != null && destMember == null));
            CreateMap<HandoverSheetGroup, HandoverSheetGroup>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember, context) => srcMember != null && destMember == null));
            CreateMap<HandoverSheetRow, HandoverSheetRow>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember, context) => srcMember != null && destMember == null));


            CreateMap<CreateOrUpdateSheetSettingMainRequest, HandoverSheetMain>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CreateOrUpdateSheetSettingGroupRequest, HandoverSheetGroup>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CreateOrUpdateSheetSettingRowRequest, HandoverSheetRow>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<FileDetail, FileDetailInfo>()
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => ParseDateTimeFromUnixTime(src.CreatedTime)))
                .ForMember(dest => dest.UpdatedTime, opt => opt.MapFrom(src => ParseDateTimeFromUnixTime(src.UpdatedTime)))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FileDetailInfo, FileDetail>()
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime != null ? ConvertToUnixTimestamp(src.CreatedTime) : (long?)null))
                .ForMember(dest => dest.UpdatedTime, opt => opt.MapFrom(src => src.UpdatedTime != null ? ConvertToUnixTimestamp(src.UpdatedTime) : (long?)null))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AnnouncementHistory, AnnouncementHistoryDetail>()
                .ForMember(dest => dest.NewBeginPublishTime, opt => opt.MapFrom(src => FormatDateString(src.NewBeginPublishTime)))
                .ForMember(dest => dest.OldBeginPublishTime, opt => opt.MapFrom(src => FormatDateString(src.OldBeginPublishTime)))
                .ForMember(dest => dest.NewEndPublishTime, opt => opt.MapFrom(src => FormatDateString(src.NewEndPublishTime)))
                .ForMember(dest => dest.OldEndPublishTime, opt => opt.MapFrom(src => FormatDateString(src.OldEndPublishTime)))
                .ForMember(dest => dest.NewBeginViewTime, opt => opt.MapFrom(src => FormatDateString(src.NewBeginViewTime)))
                .ForMember(dest => dest.OldBeginViewTime, opt => opt.MapFrom(src => FormatDateString(src.OldBeginViewTime)))
                .ForMember(dest => dest.NewEndViewTime, opt => opt.MapFrom(src => FormatDateString(src.NewEndViewTime)))
                .ForMember(dest => dest.OldEndViewTime, opt => opt.MapFrom(src => FormatDateString(src.OldEndViewTime)))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            //交班內容
            CreateMap<HandoverSheetMain, HandoverSheetRowDetailAndSettings>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverSheetGroup, GroupSetting>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverSheetRow, RowSettingAndDetail>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember, context) => srcMember != null && destMember == null));

            CreateMap<HandoverDetail, MyHandoverDetailDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverDetail, HandoverDetailWithReaders>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverDetail, HandoverDetailDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverDetail, HandoverDetailWithReadDto>()
                //.ForMember(dest => dest.FileAttIds,
                //           opt => opt.MapFrom(src => src.FileAttIds != null ? src.FileAttIds.Split(',', StringSplitOptions.None).ToList() : null))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverDetailReader, HandoverDetailReaderDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }

        //public Dictionary<string, object>? MapSchema(ColumnDefinition src)
        //{
        //    var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
        //    if (src.Schema != null)
        //    {
        //        return JsonSerializer.Deserialize<Dictionary<string, object>>(
        //            src.Schema.ToJson(jsonWriterSettings, null, null),
        //            new JsonSerializerOptions
        //            {
        //                WriteIndented = false
        //            });
        //    }
        //    return null;
        //}

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
