using AutoMapper;
using BLL.Models;
using DAL.EF.Tables;

namespace BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserModel>().ReverseMap();
        CreateMap<Booking, BookingModel>().ReverseMap();
        CreateMap<Layout, LayoutModel>().ReverseMap();
        CreateMap<Seat, SeatModel>().ReverseMap();
        

    }
}