﻿using HUTECHClassroom.Application.Common.Validators;
using HUTECHClassroom.Application.Missions.DTOs;

namespace HUTECHClassroom.Application.Projects.Queries.GetProjectMissionsWithPagination;

public class GetProjectMissionsWithPaginationQueryValidator : GetWithPaginationQueryValidator<GetProjectMissionsWithPaginationQuery, MissionDTO, ProjectPaginationParams>
{ }
