﻿using EntityFrameworkCore.QueryBuilder.Interfaces;
using HUTECHClassroom.Application.Classrooms.DTOs;
using HUTECHClassroom.Application.Common.Models;
using HUTECHClassroom.Application.Common.Requests;
using HUTECHClassroom.Domain.Interfaces;
using System.Linq.Expressions;

namespace HUTECHClassroom.Application.Users.Queries.GetUserClassroomsWithPagination;

public record GetUserClassroomsWithPaginationQuery(PaginationParams Params) : GetWithPaginationQuery<ClassroomDTO, PaginationParams>(Params);
public class GetUserClassroomsWithPaginationQueryHandler : GetWithPaginationQueryHandler<Classroom, GetUserClassroomsWithPaginationQuery, ClassroomDTO, PaginationParams>
{
    private readonly IUserAccessor _userAccessor;

    public GetUserClassroomsWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUserAccessor userAccessor) : base(unitOfWork, mapper)
    {
        _userAccessor = userAccessor;
    }
    protected override IQuery<Classroom> Order(IMultipleResultQuery<Classroom> query) => query.OrderBy(x => x.CreateDate);
    protected override Expression<Func<Classroom, bool>> FilterPredicate(GetUserClassroomsWithPaginationQuery query)
        => x => x.ClassroomUsers.Any(y => y.UserId == _userAccessor.Id) || x.LecturerId == _userAccessor.Id;
}
