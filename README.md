# helloAPI.test

this is just a simple test i made for the controller that handles the products category for the helloAPI project, this controller was just scaffolded initially and just updated
to match what i need. 

### Things I learned while doing this
* unit testing vs integrated testing
* using Moq and FakeItEasy <sup>(although I scrapped it for this test as it's too hard to mock EF)</sup>
* use of Class Fixtures in testing
* Using in-memorydb is not really "Unit Testing" per se <sup>(BUT a LOT easier than setting up MOCKS/Fakes)</sup>
* //Arrange, //Act, //Assert
* how to run unit test
* how to link a unit test to an existing project

___________

#### How to run this test locally against the helloAPI Project

1. Clone this repo
```
git clone https://github.com/marcotitoL/helloAPI.test.git
```
2. edit **helloAPI.Test.csproj** file, update the value of Include in &lt;ItemGroup>&lt;ProjectReference>
   
   **__OR__** type via CLI `dotnet add /path/to/this/proj/helloAP.Test.csproj reference /path/to/helloAPI/helloAPI.csproj`


3. Finally run the test via CLI
```
dotnet test -v quiet --nologo -l:"console;verbosity=normal"
```
   If everything went fine, you should be able to see an output like this:
   ![image](https://user-images.githubusercontent.com/103156908/165854706-05dd0443-8691-4441-b94c-88569b694c39.png)


________

> **NOTE** that this is not a complete unit test for the helloAPI, but only for the productsCategory Controller
