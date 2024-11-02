using AutoMapper;
using GHCW_BE.Helpers;
using GHCW_BE.Models;

namespace GHCW_BE.Services
{
    public class ScheduleService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private Helper _helper;
        private IMapper _mapper;

        public ScheduleService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }


    }
}
