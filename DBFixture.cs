using Microsoft.EntityFrameworkCore;
using System;

namespace helloAPI.Test;

public class DBFixture : IDisposable
{
    public ApplicationDbContext applicationDbContext { get; private set; } = null!;
    public DBFixture()
    {
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDB")
        .Options;
        this.applicationDbContext = new ApplicationDbContext(options);
        this.applicationDbContext.Database.EnsureCreated();

        this.applicationDbContext.SaveChangesAsync();
    }

    public async void Dispose()
    {
        await this.applicationDbContext.Database.EnsureDeletedAsync();
    }

    
}