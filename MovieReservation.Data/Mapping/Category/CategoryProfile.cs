using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Mapping.Category
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<Entities.Category, Dtos.CategoryDTO>().ReverseMap();
        }
    }
}
