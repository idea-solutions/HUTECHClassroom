﻿using HUTECHClassroom.Application.Common.DTOs;

namespace HUTECHClassroom.Application.Classrooms.DTOs
{
    public record ClassroomDTO : BaseEntityDTO
    {
        public string Title { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}
