using Xunit;
using helloAPI.Controllers;

using helloAPI.DTO;
using System.Collections.Generic;

using System.Linq;
using Newtonsoft.Json;

using System;
using Microsoft.AspNetCore.Mvc;

//using FakeItEasy;
//using Moq;

namespace helloAPI.Test;

public class ProductsCategoryTest : IClassFixture<DBFixture>
{
    private readonly ApplicationDbContext applicationDbContext;

    public ProductsCategoryTest(DBFixture dBFixture){
        applicationDbContext = dBFixture.applicationDbContext;
    }

    [Fact]
    public async void Get_Returns_All_Products()
    {
        //arrange
        var productsCategoryController = new ProductsCategoryController(applicationDbContext);
        
        //act
        var actionResult = await productsCategoryController.GetProductsCategory();

        var allProductsFromEndpoint = actionResult.Value as IEnumerable<ProductCategoryDTO>;

        var allProductsFromInMemoryDb = applicationDbContext.ProductsCategory.Select( cat => new ProductCategoryDTO{
            Id = cat.Guid,
            CategoryName = cat.CategoryName,
            ProductCount = applicationDbContext.Products.Where(p=>p.CategoryId==cat.Id).Count()
        }).ToList();

        //assert
        
        Assert.Equal( JsonConvert.SerializeObject( allProductsFromEndpoint ), JsonConvert.SerializeObject( allProductsFromInMemoryDb ) );

    }


    [Fact]
    public async void GetID_Returns_Correct_Product(){

        //arrange
        var productsCategoryController = new ProductsCategoryController(applicationDbContext);

        //act
        var randomProduct = applicationDbContext.ProductsCategory.Select( cat => new ProductCategoryDTO{
            Id = cat.Guid,
            CategoryName = cat.CategoryName,
            ProductCount = applicationDbContext.Products.Where(p=>p.CategoryId==cat.Id).Count()
            
        } ).OrderBy( o => Guid.NewGuid() ).FirstOrDefault();

        var actionResult = await productsCategoryController.GetProductsCategory( randomProduct?.Id );

        var productFromEndpoint = actionResult.Value as ProductCategoryDTO;

        //assert
        Assert.Equal( JsonConvert.SerializeObject(randomProduct), JsonConvert.SerializeObject(productFromEndpoint));

    }

    [Fact]
    public async void GetID_Does_NOT_Return_Unexisting_Product(){

        //arrange
        var productsCategoryController = new ProductsCategoryController( applicationDbContext );

        //act
        var actionResult = await productsCategoryController.GetProductsCategory( "NON-EXISTING-PRODUCT-ID" );

        //asset
        Assert.Null( actionResult.Value );

    }

    [Fact]
    public async void PutProductsCategory_Updates_Product(){

        //arrange
        var productsCategoryController = new ProductsCategoryController( applicationDbContext );

        //act
        var randomProduct = applicationDbContext.ProductsCategory.Select( cat => new ProductCategoryDTO{
            Id = cat.Guid,
            CategoryName = cat.CategoryName,
            ProductCount = applicationDbContext.Products.Where(p=>p.CategoryId==cat.Id).Count()
            
        } ).OrderBy( o => Guid.NewGuid() ).FirstOrDefault();

        UpdateProductsCategoryDTO updateProductsCategoryDTO = new UpdateProductsCategoryDTO(){
            CategoryName = randomProduct?.CategoryName + "-updated"
        };

        var actionResult = await productsCategoryController.PutProductsCategory(randomProduct?.Id, updateProductsCategoryDTO);

        var actionResult2 = await productsCategoryController.GetProductsCategory(randomProduct?.Id);
        var updatedProduct = actionResult2.Value as ProductCategoryDTO;

        //assert
        Assert.IsType<NoContentResult>(actionResult);

        Assert.Equal( randomProduct?.CategoryName + "-updated", updatedProduct?.CategoryName );
    }

    [Fact]
    public async void Delete_Removes_Product(){
        //arrange
        var productsCategoryController = new ProductsCategoryController( applicationDbContext );

        //act
        var actionResult = await productsCategoryController.DeleteProductsCategory("e2cfd51c-47cc-4b86-b0de-216ede287fe5");

        var actionResult2 = await productsCategoryController.GetProductsCategory("e2cfd51c-47cc-4b86-b0de-216ede287fe5");

        //assert
        Assert.IsType<NoContentResult>(actionResult);

        Assert.Null(actionResult2.Value);
    }

    [Fact]
    public async void Post_Creates_New_Product_Successfully(){
        //arrange
        var randomGen = new Random();
        string randomCategoryName = "Category - " + randomGen.Next(1,100).ToString();
        var productsCategoryController = new ProductsCategoryController( applicationDbContext );

        //act
        var actionResult = await productsCategoryController.PostProductsCategory( new UpdateProductsCategoryDTO(){ CategoryName = randomCategoryName });
        var createdProductResult = actionResult.Result as CreatedAtActionResult;
        var createdProduct = createdProductResult?.Value as ProductCategoryDTO;

        var actionResult2 = await productsCategoryController.GetProductsCategory(createdProduct?.Id);

        //assert
        Assert.IsType<ActionResult<ProductCategoryDTO>>(actionResult);
        Assert.Equal( randomCategoryName ,actionResult2?.Value?.CategoryName);
        
    }

}