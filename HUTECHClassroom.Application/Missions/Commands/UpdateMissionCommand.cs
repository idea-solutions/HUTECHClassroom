﻿using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using HUTECHClassroom.Application.Common.Requests;
using HUTECHClassroom.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace HUTECHClassroom.Application.Missions.Commands
{
    public record UpdateMissionCommand(Guid Id) : UpdateCommand(Id)
    {
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        public bool IsDone { get; set; }
    }
    public class UpdateMissionCommandHandler : UpdateCommandHandler<Mission, UpdateMissionCommand>
    {
        public UpdateMissionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
    }
}
