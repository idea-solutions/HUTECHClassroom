﻿using HUTECHClassroom.Application.Account.DTOs;
using HUTECHClassroom.Application.Common.Exceptions;
using HUTECHClassroom.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace HUTECHClassroom.Application.Account.Commands.Register;

public record RegisterCommand : IRequest<AccountDTO>
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Guid FacultyId { get; set; }
}
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AccountDTO>
{
    private readonly UserManager<ApplicationUser> _userManger;
    private readonly ITokenService _tokenService;
    private readonly IRepository<Faculty> _facultyRepository;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManger, IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _userManger = userManger;
        _tokenService = tokenService;
        _facultyRepository = unitOfWork.Repository<Faculty>();
    }
    public async Task<AccountDTO> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var facultyQuery = _facultyRepository
            .SingleResultQuery()
            .AndFilter(x => x.Id == request.FacultyId);

        var faculty = await _facultyRepository.SingleOrDefaultAsync(facultyQuery);

        if (faculty == null) throw new NotFoundException(nameof(Faculty), request.FacultyId);

        user.Faculty = faculty;

        var result = await _userManger.CreateAsync(user, request.Password);

        if (result.Succeeded) return AccountDTO.Create(user, _tokenService.CreateToken(user));

        throw new InvalidOperationException("Failed to register");
    }
}
