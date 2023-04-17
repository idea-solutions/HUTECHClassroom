﻿using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using HUTECHClassroom.Application.Common.Requests;
using HUTECHClassroom.Application.Groups.DTOs;
using HUTECHClassroom.Domain.Entities;

namespace HUTECHClassroom.Application.Groups.Commands.UpdateGroup
{
    public record UpdateGroupCommand(Guid Id) : UpdateCommand(Id)
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class UpdateGroupCommandHandler : UpdateCommandHandler<Group, UpdateGroupCommand>
    {
        public UpdateGroupCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
    }
}
