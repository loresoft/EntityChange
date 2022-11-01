using System;
using System.Collections.Generic;

using EntityChange.Extensions;

using FluentAssertions;

using Xunit;

namespace EntityChange.Tests;

public class DeltaCompareTests
{
    [Fact]
    public void CompareInt()
    {
        var existing = new List<int> { 1, 2, 3, 4, 5 };
        var current = new List<int> { 1, 3, 5, 7 };

        var delta = existing.DeltaCompare(current);
        delta.Should().NotBeNull();

        delta.Created.Should().NotBeNullOrEmpty();
        delta.Created.Should().HaveCount(1);
        delta.Created.Should().IntersectWith(new[] { 7 });

        delta.Deleted.Should().NotBeNullOrEmpty();
        delta.Deleted.Should().HaveCount(2);
        delta.Deleted.Should().IntersectWith(new[] { 2, 4 });

        delta.Matched.Should().NotBeNullOrEmpty();
        delta.Matched.Should().HaveCount(3);
        delta.Matched.Should().IntersectWith(new[] { 1, 3, 5 });
    }

    [Fact]
    public void CompareUser()
    {
        var existing = new List<User>
        {
            new User {Id = new Guid("a956a1f2-f544-4c59-8c9d-8d1ce506d404"), Name = "Name 1"},
            new User {Id = new Guid("efd95a4f-0f66-4f34-99b4-94e32adfc1d2"), Name = "Name 2"},
            new User {Id = new Guid("b3fa9a1e-19bc-437d-9bbb-2a03d3f0156c"), Name = "Name 3"},
        };

        var current = new List<User>
        {
            new User {Id = new Guid("efd95a4f-0f66-4f34-99b4-94e32adfc1d2"), Name = "Name 2"},
            new User {Id = new Guid("b3fa9a1e-19bc-437d-9bbb-2a03d3f0156c"), Name = "Name 3"},
            new User {Id = new Guid("de7693fa-b090-41aa-bbeb-f68b9aed13c5"), Name = "Name 4"},
        };

        var comparer = KeyEqualityComparer<User, Guid>.Create(p => p.Id);
        var delta = existing.DeltaCompare(current, comparer);
        delta.Should().NotBeNull();

        delta.Created.Should().NotBeNullOrEmpty();
        delta.Created.Should().HaveCount(1);

        delta.Deleted.Should().NotBeNullOrEmpty();
        delta.Deleted.Should().HaveCount(1);

        delta.Matched.Should().NotBeNullOrEmpty();
        delta.Matched.Should().HaveCount(2);
    }

    [Fact]
    public void CompareUserRole()
    {
        var existing = new List<UserRole>
        {
            new UserRole {UserName = "Name1", RoleName = "Role1"},
            new UserRole {UserName = "Name1", RoleName = "Role2"},
            new UserRole {UserName = "Name1", RoleName = "Role3"},
        };

        var current = new List<UserRole>
        {
            new UserRole {UserName = "Name1", RoleName = "Role2"},
            new UserRole {UserName = "Name1", RoleName = "Role3"},
            new UserRole {UserName = "Name1", RoleName = "Role4"},
        };

        var comparer = KeyEqualityComparer<UserRole, Tuple<string, string>>.Create(p => new Tuple<string, string>(p.UserName, p.RoleName));
        var delta = existing.DeltaCompare(current, comparer);
        delta.Should().NotBeNull();

        delta.Created.Should().NotBeNullOrEmpty();
        delta.Created.Should().HaveCount(1);

        delta.Deleted.Should().NotBeNullOrEmpty();
        delta.Deleted.Should().HaveCount(1);

        delta.Matched.Should().NotBeNullOrEmpty();
        delta.Matched.Should().HaveCount(2);

    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class UserRole
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }

}
