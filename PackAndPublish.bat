set path=%path%;C:\Programs\NuGet
nuget pack KendoGridCrudController.nuspec
nuget push KendoGridCrudController.1.0.0.0.nupkg -s http://falafel-svn:82/ Fa1afe1!