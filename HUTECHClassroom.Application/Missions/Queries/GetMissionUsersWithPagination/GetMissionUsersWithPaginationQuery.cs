﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Collections;
using EntityFrameworkCore.Repository.Extensions;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using HUTECHClassroom.Application.Common.DTOs;
using HUTECHClassroom.Application.Common.Exceptions;
using HUTECHClassroom.Application.Common.Models;
using HUTECHClassroom.Application.Common.Requests;
using HUTECHClassroom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HUTECHClassroom.Application.Missions.Queries.GetMissionUsersWithPagination
{
    public record GetMissionUsersWithPaginationQuery(Guid Id, PaginationParams Params) : GetWithPaginationQuery<MemberDTO>(Params);
    public class GetMissionUsersWithPaginationQueryHandler : GetWithPaginationQueryHandler<ApplicationUser, GetMissionUsersWithPaginationQuery, MemberDTO>
    {
        public GetMissionUsersWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
        protected override Expression<Func<ApplicationUser, bool>> FilterPredicate(GetMissionUsersWithPaginationQuery query)
        {
            return x => x.MissionUsers.Any(x => x.MissionId == query.Id);
        }
        protected override Expression<Func<ApplicationUser, bool>> SearchStringPredicate(string searchString)
        {
            var toLowerSearchString = searchString.ToLower();
            return x => x.UserName.ToLower().Contains(toLowerSearchString) || x.Email.ToLower().Contains(toLowerSearchString);
        }
        protected override Expression<Func<ApplicationUser, object>> OrderByKeySelector()
        {
            return x => x.UserName;
        }
    }
}
