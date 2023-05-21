﻿using HUTECHClassroom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HUTECHClassroom.Infrastructure.Persistence.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(x => x.Users)
            .WithOne(x => x.Faculty)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Classrooms)
            .WithOne(x => x.Faculty)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
