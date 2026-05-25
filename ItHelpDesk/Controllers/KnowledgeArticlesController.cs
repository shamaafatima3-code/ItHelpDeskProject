using ItHelpDesk.Data;
using ItHelpDesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ItHelpDesk.Controllers
{
    public class KnowledgeArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KnowledgeArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KnowledgeArticles
        public async Task<IActionResult> Index(string searchString)
        {
            var articles = from a in _context.KnowledgeArticles
                           select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(a =>
                    a.Title.Contains(searchString) ||
                    a.Category.Contains(searchString) ||
                    a.Content.Contains(searchString));
            }

            return View(await articles.ToListAsync());
        }

        // GET: KnowledgeArticles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knowledgeArticle = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(m => m.Id == id);

            if (knowledgeArticle == null)
            {
                return NotFound();
            }

            return View(knowledgeArticle);
        }

        // GET: KnowledgeArticles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KnowledgeArticles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Category,CreatedBy,CreatedDate")] KnowledgeArticle knowledgeArticle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(knowledgeArticle);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(knowledgeArticle);
        }

        // GET: KnowledgeArticles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knowledgeArticle = await _context.KnowledgeArticles.FindAsync(id);

            if (knowledgeArticle == null)
            {
                return NotFound();
            }

            return View(knowledgeArticle);
        }

        // POST: KnowledgeArticles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Content,Category,CreatedBy,CreatedDate")] KnowledgeArticle knowledgeArticle)
        {
            if (id != knowledgeArticle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(knowledgeArticle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KnowledgeArticleExists(knowledgeArticle.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(knowledgeArticle);
        }

        // GET: KnowledgeArticles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knowledgeArticle = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(m => m.Id == id);

            if (knowledgeArticle == null)
            {
                return NotFound();
            }

            return View(knowledgeArticle);
        }

        // POST: KnowledgeArticles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var knowledgeArticle = await _context.KnowledgeArticles.FindAsync(id);

            if (knowledgeArticle != null)
            {
                _context.KnowledgeArticles.Remove(knowledgeArticle);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KnowledgeArticleExists(int? id)
        {
            return _context.KnowledgeArticles.Any(e => e.Id == id);
        }
    }
}