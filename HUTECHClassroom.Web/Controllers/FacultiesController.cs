﻿using AutoMapper.QueryableExtensions;
using HUTECHClassroom.Domain.Constants;
using HUTECHClassroom.Domain.Entities;
using HUTECHClassroom.Web.ViewModels.ApplicationUsers;
using HUTECHClassroom.Web.ViewModels.Faculties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace HUTECHClassroom.Web.Controllers;

[Authorize(DeanOrTrainingOfficePolicy)]
public class FacultiesController : BaseEntityController<Faculty>
{
    // GET: Faculties
    public IActionResult Index(int? page, int? size)
    {
        int pageIndex = page ?? 1;
        int pageSize = size ?? 5;
        return View(DbContext.Faculties
            .OrderByDescending(x => x.CreateDate)
            .ToPagedList(pageIndex, pageSize));
    }

    // GET: Faculties/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null || DbContext.Faculties == null)
        {
            return NotFound();
        }

        var faculty = await DbContext.Faculties
            .FirstOrDefaultAsync(m => m.Id == id);
        if (faculty == null)
        {
            return NotFound();
        }

        return View(faculty);
    }

    public async Task<IActionResult> ImportFacultyUsers(Guid? id)
    {
        if (id == null)
            return View("Index");
        if (id == null || DbContext.Faculties == null)
        {
            return NotFound();
        }
        var classroom = await DbContext.Faculties.FindAsync(id);
        if (classroom == null)
        {
            return NotFound();
        }
        var viewModel = new ImportUsersToFacultyViewModel
        {
            FacultyId = classroom.Id,
            FacultyName = classroom.Name
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFacultyUsers(ImportUsersToFacultyViewModel viewModel)
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

        var userViewModels = ExcelService.ReadExcelFileWithColumnNames<ImportedUserViewModel>(viewModel.File.OpenReadStream(), null);
        // Do something with the imported people data, such as saving to a database

        var existingUsers = await DbContext.Users
            .Where(u => userViewModels.Select(x => x.UserName).Contains(u.UserName))
            .ToListAsync();

        var newUsers = userViewModels
            .Where(vm => !existingUsers.Select(x => x.UserName).Contains(vm.UserName))
            .AsQueryable()
            .ProjectTo<ApplicationUser>(Mapper.ConfigurationProvider)
            .ToList();

        foreach (var user in existingUsers)
        {
            user.FacultyId = viewModel.FacultyId;
            DbContext.Entry(user).State = EntityState.Modified;
            DbContext.Update(user);
        }

        foreach (var user in newUsers)
        {
            user.FacultyId = viewModel.FacultyId;
            await UserManager.CreateAsync(user, user.UserName).ConfigureAwait(false);
            await UserManager.AddToRoleAsync(user, RoleConstants.STUDENT).ConfigureAwait(false);
        }


        int count = await DbContext.SaveChangesAsync().ConfigureAwait(false);

        ViewBag.Success = $"Successfully imported and updated {count} rows.";
        return RedirectToAction("Index");
    }

    // GET: Faculties/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Faculties/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Id,CreateDate")] Faculty faculty)
    {
        if (ModelState.IsValid)
        {
            faculty.Id = Guid.NewGuid();
            DbContext.Add(faculty);
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(faculty);
    }

    // GET: Faculties/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null || DbContext.Faculties == null)
        {
            return NotFound();
        }

        var faculty = await DbContext.Faculties.FindAsync(id);
        if (faculty == null)
        {
            return NotFound();
        }
        return View(faculty);
    }

    // POST: Faculties/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Name,Id,CreateDate")] Faculty faculty)
    {
        if (id != faculty.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                DbContext.Update(faculty);
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FacultyExists(faculty.Id))
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
        return View(faculty);
    }

    // GET: Faculties/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null || DbContext.Faculties == null)
        {
            return NotFound();
        }

        var faculty = await DbContext.Faculties
            .FirstOrDefaultAsync(m => m.Id == id);
        if (faculty == null)
        {
            return NotFound();
        }

        return View(faculty);
    }

    // POST: Faculties/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (DbContext.Faculties == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Faculties'  is null.");
        }
        var faculty = await DbContext.Faculties.FindAsync(id);
        if (faculty != null)
        {
            DbContext.Faculties.Remove(faculty);
        }

        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FacultyExists(Guid id)
    {
        return DbContext.Faculties.Any(e => e.Id == id);
    }
}
