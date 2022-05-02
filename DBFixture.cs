using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

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


[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DBFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}