using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reddit.Dtos;
using Reddit.Mapper;
using Reddit.Models;
using System.Linq;
using System.Linq.Expressions;


namespace Reddit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        IQueryable<Community> community;
        IQueryable<Post> post;

        public CommunityController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            community = _context.Communities.AsQueryable();
            post = _context.Posts.AsQueryable();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Community>>> GetCommunities(int pageNumber, int pageSize, string? searchTerm, string? SortTerm, bool? isAscending = true)
        {
            if (isAscending == false)
            {
                community = community.OrderByDescending(GetSortExpression(SortTerm));
            }
            else
            {
                community = community.OrderBy(GetSortExpression(SortTerm));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                community = community.Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm);
            }
            return await _context.Communities.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Community>> GetCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);

            if(community == null)
            {
                return NotFound();
            }

            return community;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommunity(CreateCommunityDto communityDto)
        {
                var community = _mapper.toCommunity(communityDto);

                await _context.Communities.AddAsync(community);
                await _context.SaveChangesAsync();
                return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);
            if (community == null)
            {
                return NotFound();
            }

            _context.Communities.Remove(community);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommunity (int id, Community community)
        {
            if (!CommunityExists(id))
            {
                return NotFound();
            }

            _context.Entry(community).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool CommunityExists(int id) => _context.Communities.Any(e => e.Id == id);

        private Expression<Func<Community,object>> GetSortExpression(string? sortTerm)
        {
            sortTerm = sortTerm?.ToLower();
            return sortTerm switch
            {
                "createdAt" => community => community.CreateAt,
                "PostsCount" => community => post.Count(c => c.CommunityId == community.Id),
                "subscribersCount" => community => community.Subscribers.Count(),
                "id" => community => community.Id
            };
        }
    }
}
