using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGestao2223226.Data;

namespace CinemaGestao2223226.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Catalogo - Public movie catalog
        public async Task<IActionResult> Index()
        {
            var filmes = await _context.Filmes.ToListAsync();
            return View(filmes);
        }

        // GET: Catalogo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filme = await _context.Filmes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filme == null)
            {
                return NotFound();
            }

            return View(filme);
        }

        // GET: Catalogo/Sessoes/5 - Show sessions for a specific movie
        public async Task<IActionResult> Sessoes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null)
            {
                return NotFound();
            }

            var sessoes = await _context.Sessoes
                .Where(s => s.FilmeId == id && s.DataHora > System.DateTime.Now)
                .OrderBy(s => s.DataHora)
                .ToListAsync();

            ViewBag.Filme = filme;
            return View(sessoes);
        }
    }
}