using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaGestao.Models;
using CinemaGestao2223226.Data;
using Microsoft.AspNetCore.Authorization;

namespace CinemaGestao2223226.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservas - Admin sees all, Client sees only their own
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Administrador");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservas = isAdmin
                ? await _context.Reservas.Include(r => r.Sessao).ThenInclude(s => s.Filme).ToListAsync()
                : await _context.Reservas
                    .Include(r => r.Sessao)
                    .ThenInclude(s => s.Filme)
                    .Where(r => r.UtilizadorId == userId)
                    .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Sessao)
                .ThenInclude(s => s.Filme)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Check if user owns this reservation or is admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrador") && reserva.UtilizadorId != userId)
            {
                return Forbid();
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        [Authorize(Roles = "Cliente")]
        public IActionResult Create(int? sessaoId)
        {
            if (sessaoId == null)
            {
                ViewData["SessaoId"] = new SelectList(_context.Sessoes.Include(s => s.Filme), "Id", "Filme.Titulo");
            }
            else
            {
                ViewData["SessaoId"] = sessaoId;
                var sessao = _context.Sessoes.Include(s => s.Filme).FirstOrDefault(s => s.Id == sessaoId);
                ViewBag.Sessao = sessao;
            }

            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create([Bind("SessaoId,NumeroBilhetes")] Reserva reserva)
        {
            var sessao = await _context.Sessoes.FindAsync(reserva.SessaoId);

            if (sessao == null)
            {
                ModelState.AddModelError("", "Sessão não encontrada.");
                return View(reserva);
            }

            if (reserva.NumeroBilhetes > sessao.LugaresDisponiveis)
            {
                ModelState.AddModelError("NumeroBilhetes", $"Apenas {sessao.LugaresDisponiveis} lugares disponíveis.");
                ViewData["SessaoId"] = reserva.SessaoId;
                ViewBag.Sessao = await _context.Sessoes.Include(s => s.Filme).FirstOrDefaultAsync(s => s.Id == reserva.SessaoId);
                return View(reserva);
            }

            reserva.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reserva.DataReserva = DateTime.Now;

            sessao.LugaresDisponiveis -= reserva.NumeroBilhetes;

            _context.Add(reserva);
            _context.Update(sessao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reserva criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Sessao)
                .ThenInclude(s => s.Filme)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Check if user owns this reservation or is admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrador") && reserva.UtilizadorId != userId)
            {
                return Forbid();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.Include(r => r.Sessao).FirstOrDefaultAsync(r => r.Id == id);
            
            if (reserva == null)
            {
                return NotFound();
            }

            // Check if user owns this reservation or is admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrador") && reserva.UtilizadorId != userId)
            {
                return Forbid();
            }

            // Return seats to session
            reserva.Sessao.LugaresDisponiveis += reserva.NumeroBilhetes;
            _context.Update(reserva.Sessao);

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reserva cancelada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}