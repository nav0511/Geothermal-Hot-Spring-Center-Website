using AutoMapper;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class TicketDetailService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private Helper _helper;
        private IMapper _mapper;

        public TicketDetailService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }

        public IQueryable<TicketDetail> GetListBookingDetails()
        {
            return _context.TicketDetails.AsQueryable();
        }

        public async Task<List<TicketDetail>> GetBookingDetails(int id)
        {
            return await _context.TicketDetails
                .Include(t => t.Ticket) 
                .Where(t => t.TicketId == id)
                .ToListAsync(); 
        }
    }
}
