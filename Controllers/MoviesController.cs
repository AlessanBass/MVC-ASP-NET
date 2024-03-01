using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;

        public MoviesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Movies
        [Authorize(Roles = "admin")]      
        public async Task<IActionResult> Index()
        {
            var mvcMovieContext = _context.Movie.Include(m => m.Studio);

            return View(await mvcMovieContext.ToListAsync());
        }

        // GET: Movies/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Studio)
                .Include(m => m.Artists)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["StudioId"] = new SelectList(_context.Set<Studio>(), "StudioId", "Name");
            ViewData["Artists"] = new SelectList(_context.Set<Artist>(), "ArtistId", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,StudioId")] Movie movie, string[] Artists)
        {
            if (ModelState.IsValid)
            {
                var studio = await _context.Studio.FirstOrDefaultAsync(m => m.StudioId == movie.StudioId);
                if (studio != null)
                {
                    // Inicialize a lista de artistas do filme
                    movie.Artists = new List<Artist>();

                    // Verifique se existem artistas selecionados
                    if (Artists != null)
                    {
                        // Para cada ID de artista selecionado
                        foreach (var artistId in Artists)
                        {
                            // Tente converter o ID de string para int
                            if (int.TryParse(artistId, out int parsedArtistId))
                            {
                                // Busque o artista no banco de dados
                                var artist = await _context.Artist.FindAsync(parsedArtistId);
                                if (artist != null)
                                {
                                    // Adicione o artista à lista de artistas do filme
                                    movie.Artists.Add(artist);
                                }
                            }
                        }
                    }

                    // Adicione o filme ao contexto e salve as alterações
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    // Percamence na mesma pagina
                    return RedirectToAction(nameof(Details), new { id = movie.Id });
                }
                return RedirectToAction(nameof(Index));
            }

            // Se houver erros de validação, retorne a view com os dados preenchidos
            ViewData["StudioId"] = new SelectList(_context.Set<Studio>(), "StudioId", "Name", movie.StudioId);
            ViewData["Artists"] = new SelectList(_context.Set<Artist>(), "ArtistId", "Name");
            return View(movie);
        }


        // GET: Movies/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.Include(m => m.Artists).FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            // Obtenha todos os artistas
            var allArtists = await _context.Artist.ToListAsync();

            // Obtenha os IDs dos artistas associados a este filme
            var movieArtistIds = await _context.Movie.Include(ma => ma.Artists).Where(ma => ma.Id == id).SelectMany(ma => ma.Artists.Select(a => a.ArtistId)).ToListAsync();

            // Passe os artistas e os IDs associados ao filme para a view
            ViewBag.AllArtists = allArtists;
            ViewBag.MovieArtistIds = movieArtistIds;
            ViewData["StudioId"] = new SelectList(_context.Set<Studio>(), "StudioId", "Name", movie.StudioId);
            ViewData["Artists"] = new SelectList(_context.Set<Artist>(), "ArtistId", "Name");

            return View(movie);
        }


        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,StudioId")] Movie movie, string[] Artists)
        {
            var originalMovie = _context.Movie.Include(m => m.Artists).FirstOrDefault(m => m.Id == id);
            if (id != movie.Id)
            {
                return NotFound();
            }
            if (originalMovie == null)
            {
                return NotFound();
            }
            if (Artists == null)
            {
                return NotFound();

            }

            if (ModelState.IsValid)
            {
                // Para cada ID de artista selecionado
                foreach (var artistId in Artists)
                {
                    // Tente converter o ID de string para int
                    if (int.TryParse(artistId, out int parsedArtistId))
                    {
                        // Busque o artista no banco de dados
                        var artist = await _context.Artist.FindAsync(parsedArtistId);
                        if (artist != null)
                        {
                            // Adicione o artista à lista de artistas do filme
                            originalMovie.Artists.Add(artist);
                        }
                    }
                }
                try
                {
                    _context.Update(originalMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(originalMovie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudioId"] = new SelectList(_context.Set<Studio>(), "StudioId", "Name", movie.StudioId);
            ViewData["Artists"] = new SelectList(_context.Set<Artist>(), "ArtistId", "Name");

            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Studio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movie == null)
            {
                return Problem("Entity set 'MvcMovieContext.Movie'  is null.");
            }
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return (_context.Movie?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
