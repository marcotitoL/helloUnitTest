using Xunit;
using helloAPI.Controllers;

using helloAPI.DTO;
using System.Collections.Generic;

using System.Linq;
using Newtonsoft.Json;

using System;
using Microsoft.AspNetCore.Mvc;

using System.IO;

//using FakeItEasy;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using helloAPI.Services;

using Microsoft.EntityFrameworkCore;

namespace helloAPI.Test;

[Collection("Database collection")]
public class UsersControllerTest /*: IClassFixture<DBFixture> */
{
    private readonly ApplicationDbContext applicationDbContext;

    public UsersControllerTest(DBFixture dBFixture){
        applicationDbContext = dBFixture.applicationDbContext;
    }

    [Fact]
    public async void GET_Returns_All_users(){
        //arrange
var userManagerMock = new Mock<UserManager<IdentityUser>>(
    /* IUserStore<TUser> store */Mock.Of<IUserStore<IdentityUser>>(),
    /* IOptions<IdentityOptions> optionsAccessor */null,
    /* IPasswordHasher<TUser> passwordHasher */null,
    /* IEnumerable<IUserValidator<TUser>> userValidators */null,
    /* IEnumerable<IPasswordValidator<TUser>> passwordValidators */null,
    /* ILookupNormalizer keyNormalizer */null,
    /* IdentityErrorDescriber errors */null,
    /* IServiceProvider services */null,
    /* ILogger<UserManager<TUser>> logger */null);

var signInManagerMock = new Mock<SignInManager<IdentityUser>>(
    userManagerMock.Object,
    /* IHttpContextAccessor contextAccessor */Mock.Of<IHttpContextAccessor>(),
    /* IUserClaimsPrincipalFactory<TUser> claimsFactory */Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
    /* IOptions<IdentityOptions> optionsAccessor */null,
    /* ILogger<SignInManager<TUser>> logger */null,
    /* IAuthenticationSchemeProvider schemes */null,
    /* IUserConfirmation<TUser> confirmation */null);

        //act
        var usersController = new UsersController(userManagerMock.Object,
        signInManagerMock.Object,
        applicationDbContext,
        Mock.Of<IConfiguration>(),
        Mock.Of<IHostEnvironment>(),
        Mock.Of<SendGridService>(),
        Mock.Of<TwilioService>()
                );

        usersController.ControllerContext = new ControllerContext();
        usersController.ControllerContext.HttpContext = new DefaultHttpContext();
        usersController.ControllerContext.HttpContext.Request.Scheme = "https";

        
        var actionResult = await usersController.Get();
        var allUsersFromEndpoint = actionResult.Value as IEnumerable<UsersDTO>;

        var allUsersFromInMemoryDB = await applicationDbContext.Users
                .Join( applicationDbContext.UserDetails,
                        a => a.Id,
                        b => b.AspNetUserId,
                        (a, b) => new UsersDTO{
                            Id = a.Id,
                            Email = a.Email,
                            Firstname = b.Firstname,
                            Lastname = b.Lastname,
                            Birthdate = b.Birthdate.ToString("yyyy-MM-dd"),
                            ProfileImage =  b.ProfileImage == null ? "" : $"https://" + "localhost:7016" + "/uploads/" + b.ProfileImage,
                            Balance = b.Balance
                        }
                     ).ToListAsync();
        


        //assert
        Assert.Equal(JsonConvert.SerializeObject( allUsersFromInMemoryDB ),JsonConvert.SerializeObject( allUsersFromEndpoint ));
        
    }

    [Fact]
    public async void Register_Returns_User_ID_SuccessMessage(){

        var registrationEmail = "janesmith@email.com";
        var registrationFName = "Jane";
        var registrationLName = "Smith";

        var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),null,null,null,null,null,null,null,null);

        var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object,Mock.Of<IHttpContextAccessor>(),Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),null,null,null,null);

        /* HERE LIES A Battle Ground of trying to MOCK IConfiguration */
        /* === 100 lines of code to no avail used to lie here === */

         var dummyIConfiguration = new ConfigurationBuilder()
        .AddJsonFile( Directory.GetCurrentDirectory() + "/../../../appsettings.mock.json")
        .Build();


        userManagerMock.Setup(m=>m.CreateAsync(It.IsAny<IdentityUser>(),It.IsAny<string>()) ).ReturnsAsync(IdentityResult.Success).Verifiable();
        userManagerMock.Setup(m=>m.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>())).ReturnsAsync("mycustomtoken");
        userManagerMock.Setup(m=>m.GenerateChangePhoneNumberTokenAsync(It.IsAny<IdentityUser>(),It.IsAny<string>() )).ReturnsAsync("CODE2FA");

        /* var SendGridServiceMock = new Mock<SendGridService>();
        SendGridServiceMock.Setup(m=>m.Send(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(It.IsAny<SendGrid.Response>()); */

        var UrlHelper = new Mock<IUrlHelper>();
        UrlHelper.SetReturnsDefault("http://google.com"); 

        //act
        var usersController = new UsersController(userManagerMock.Object,
        signInManagerMock.Object,
        applicationDbContext,
        dummyIConfiguration,
        Mock.Of<IHostEnvironment>(),
        Mock.Of<SendGridService>(),
        Mock.Of<TwilioService>()
                );
        usersController.ControllerContext = new ControllerContext();
        usersController.ControllerContext.HttpContext = new DefaultHttpContext();
        usersController.ControllerContext.HttpContext.Request.Scheme = "https";
        usersController.Url = UrlHelper.Object;

        var actionResult = await usersController.registerUser(new RegisterUser{ Email=registrationEmail, Phone="+15879211199",Firstname=registrationFName,Lastname=registrationLName,Password="Pa$$w0rd.",Birthdate=new DateTime(1985,04,30) });
        var actionResultValue = actionResult as OkObjectResult;

        var user = actionResultValue?.Value?.GetType()?.GetProperty("User")?.GetValue(actionResultValue.Value);
        var userid = user?.GetType().GetProperty("Id")?.GetValue(user);

        var message = actionResultValue?.Value?.GetType()?.GetProperty("message")?.GetValue(actionResultValue.Value);

        var userFound = await applicationDbContext.UserDetails.CountAsync( u => u.Firstname == registrationFName && u.Lastname == registrationLName);
        
        //assert
        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotEqual(0,userFound);
        Assert.True(Guid.TryParse( userid?.ToString(), out _) );
        Assert.Equal("you have successfully registered. please confirm your email and your phone number",message?.ToString());
    }
}