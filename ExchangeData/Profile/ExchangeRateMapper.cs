using MapperProfile = AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeData.Dtos;
using ExchangeData.Entities;
using System.Globalization;

namespace ExchangeData.Profile
{
    public class ExchangeRateMapper : MapperProfile.Profile
    {
        public ExchangeRateMapper() 
        {
            CreateMap<ExchangeRateDto, ExchangeRate>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.RateDate, opt => opt.MapFrom(src => ParseIsoDate(src.RateDate)))
                .ForMember(dest => dest.ISO, opt => opt.MapFrom(src => src.ISO));

            CreateMap<ExchangeRate, ExchangeRateDto>()
                .ForMember(dest => dest.RateDate, opt => opt.MapFrom(src => src.RateDate.ToString("yyyy-mm-dd")))
                .ForMember(dest => dest.ISO, opt => opt.MapFrom(src => src.ISO));

        }
        private DateTime ParseIsoDate(string date)
        {
            DateTimeOffset inputDateTimeOffset = DateTimeOffset.Parse(date);

            DateTime resultDateTime = inputDateTimeOffset.DateTime;

            return resultDateTime;
        }
    }
}
