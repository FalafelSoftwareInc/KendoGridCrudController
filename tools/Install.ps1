param($installPath, $toolsPath, $package, $project)
set-location ([System.IO.Path]::GetDirectoryName($project.FullName))
get-childitem -filter "*.edmx" -recurse | foreach-object {
	$projectItemPath = ([System.IO.Path]::ChangeExtension($_.FullName, ".tt"));
	copy-item "KendoGridCrudController\Templates\ViewModels.tt" $projectItemPath;
	$project.ProjectItems.AddFromFile($projectItemPath);
}