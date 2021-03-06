param($installPath, $toolsPath, $package, $project)
set-location ([System.IO.Path]::GetDirectoryName($project.FullName))
get-childitem -filter "*.edmx" -recurse | foreach-object {
	$projectItemPath = ([System.IO.Path]::ChangeExtension($_.FullName, ".tt"));
	$projectItem = $project.DTE.Solution.FindProjectItem($projectItemPath);
	if ($projectItem) {
		foreach($childItem in $projectItem.ProjectItems) {
  			if (test-path $childItem.FileNames(1)) { remove-item $childItem.FileNames(1); }
		}
		$projectItem.Remove();
		if (test-path $projectItemPath) { remove-item $projectItemPath; }
	}
}