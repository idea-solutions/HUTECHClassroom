﻿using HUTECHClassroom.Domain.Entities;
using HUTECHClassroom.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HUTECHClassroom.Web.Controllers;

public class ClassroomsController : BaseEntityController<Classroom>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ClassroomsController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    // GET: Classrooms
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = DbContext.Classrooms.Include(c => c.Faculty).Include(c => c.Lecturer);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Classrooms/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null || DbContext.Classrooms == null)
        {
            return NotFound();
        }

        var classroom = await DbContext.Classrooms
            .Include(c => c.Faculty)
            .Include(c => c.Lecturer)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (classroom == null)
        {
            return NotFound();
        }

        return View(classroom);
    }

    // GET: Classrooms/Add-User
    public IActionResult AddUser()
    {
        return View();
    }

    // POST: Classrooms/Add-User
    [HttpPost]
    public async Task<IActionResult> AddUser(ImportUsersToClassroomViewModel viewModel)
    {
        if (viewModel.File == null || viewModel.File.Length == 0)
        {
            ViewBag.Error = "Please select a file to upload.";
            return View(viewModel);
        }

        if (!Path.GetExtension(viewModel.File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Please select an Excel file (.xlsx).";
            return View(viewModel);
        }

        var users = ExcelService.ReadExcelFileWithColumnNames<ApplicationUser>(viewModel.File.OpenReadStream(), null);
        Console.WriteLine(users.Count);
        users.ForEach(x => Console.WriteLine(x.UserName + " " + x.Email + " " + x.FirstName + " " + x.LastName + " " + x.FacultyId));
        // Do something with the imported people data, such as saving to a database
        var results = new List<IdentityResult>();
        foreach (var user in users)
        {
            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FacultyId = user.FacultyId
            };
            results.Add(await _userManager.CreateAsync(newUser, user.UserName));
        }


        ViewBag.Success = $"Successfully imported {results.Count(x => x.Succeeded)} rows.";
        return RedirectToAction("Index");
    }

    // GET: Classrooms/Create
    public IActionResult Create()
    {
        ViewData["FacultyId"] = new SelectList(DbContext.Faculties, "Id", "Name");
        ViewData["LecturerId"] = new SelectList(DbContext.Users, "Id", "UserName");
        return View();
    }

    // POST: Classrooms/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Topic,Room,Description,LecturerId,FacultyId,Id,CreateDate")] Classroom classroom)
    {
        if (ModelState.IsValid)
        {
            classroom.Id = Guid.NewGuid();
            DbContext.Add(classroom);
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["FacultyId"] = new SelectList(DbContext.Faculties, "Id", "Name", classroom.FacultyId);
        ViewData["LecturerId"] = new SelectList(DbContext.Users, "Id", "UserName", classroom.LecturerId);
        return View(classroom);
    }

    // GET: Classrooms/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null || DbContext.Classrooms == null)
        {
            return NotFound();
        }

        var classroom = await DbContext.Classrooms.FindAsync(id);
        if (classroom == null)
        {
            return NotFound();
        }
        ViewData["FacultyId"] = new SelectList(DbContext.Faculties, "Id", "Name", classroom.FacultyId);
        ViewData["LecturerId"] = new SelectList(DbContext.Users, "Id", "UserName", classroom.LecturerId);
        return View(classroom);
    }

    // POST: Classrooms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Title,Topic,Room,Description,LecturerId,FacultyId,Id,CreateDate")] Classroom classroom)
    {
        if (id != classroom.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                DbContext.Update(classroom);
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassroomExists(classroom.Id))
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
        ViewData["FacultyId"] = new SelectList(DbContext.Faculties, "Id", "Name", classroom.FacultyId);
        ViewData["LecturerId"] = new SelectList(DbContext.Users, "Id", "UserName", classroom.LecturerId);
        return View(classroom);
    }

    // GET: Classrooms/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null || DbContext.Classrooms == null)
        {
            return NotFound();
        }

        var classroom = await DbContext.Classrooms
            .Include(c => c.Faculty)
            .Include(c => c.Lecturer)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (classroom == null)
        {
            return NotFound();
        }

        return View(classroom);
    }

    // POST: Classrooms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (DbContext.Classrooms == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Classrooms'  is null.");
        }
        var classroom = await DbContext.Classrooms.FindAsync(id);
        if (classroom != null)
        {
            DbContext.Classrooms.Remove(classroom);
        }

        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClassroomExists(Guid id)
    {
        return DbContext.Classrooms.Any(e => e.Id == id);
    }
}
