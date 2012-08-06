properties {
	$hedgehog_path = Convert-Path ".\tools\x86\HedgehogDevelopment\SitecoreProject\v9.0"
	$package_builder_exe = "$hedgehog_path\HedgehogDevelopment.SitecoreProject.PackageBuilder.exe"
	$solutionPath = Convert-Path "buckets.sln"
	$msbuild_configuration = "Debug"
	$build_nuget_package = "build/nuget.package"
	$zip_exe = Resolve-Path ".\tools\7-Zip\7z.exe"
	$nuget_exe = Resolve-Path ".\tools\Nuget\nuget.exe"
	$nuget_version_number = $(
	switch ($env:PACKAGE_VERSION){
		($null) { "0.0.0.1"}
		default {$env:PACKAGE_VERSION}
	})
	$nuget_api_key = "e7f0a09d-edfe-4dce-b50b-9f92a236a600"
}

function Zip-File {
	param(
	 [Parameter(Position=0,Mandatory=1)] [string]$target = $null,
	 [Parameter(Position=1,Mandatory=1)] [string]$source = $null,
	 [Parameter(Position=2,Mandatory=1)] [string]$logFile = $null)
	 if( Test-Path -path $target){
	 	ri $target -force
	 }
	 & $zip_exe a $target  $source 2>&1 | out-file $logFile
}

function RunMsBuild {
	param ([string]$task, $outputFolder=$false, $projectOrSolution=$solutionPath)
	Write-Host "$task the solution." -ForegroundColor YELLOW
	$msbuildCommand = "msbuild $projectOrSolution /t:""$task"" /p:Configuration=$msbuild_configuration /v:quiet /nologo"
	if($outputFolder){
		$msbuildCommand += " /p:OutputPath=$outputFolder"
	}
	$msbuildExpression = $msbuildCommand
	Write-Host "Executing $msbuildExpression" -ForegroundColor YELLOW
	Exec { Invoke-Expression $msbuildExpression }
}

function Create-Temp-Directory(){
   $tmpDir = [System.IO.Path]::GetTempPath()
   $tmpDir = [System.IO.Path]::Combine($tmpDir, [System.IO.Path]::GetRandomFileName())
   [System.IO.Directory]::CreateDirectory($tmpDir) | Out-Null
   return $tmpDir
}

function CreateSitecorePackage(){
	param( $humanName, $project, $target_directory, $configuration=$msbuild_configuration)
	Write-Host "Creating sitecore database packages for $humanName, project $project." -ForegroundColor YELLOW
	$tmpDir = Create-Temp-Directory
	
	$expression = "& '$package_builder_exe' $project $target_directory $configuration $tmpDir"
	$expression
	Exec { Invoke-Expression "$expression" }
}

function Copy-TemplatedFiles(){
	param($sourceDirectory, $targetdirectory, $tokens = @{}, $TemplateFilter = '*.template', $TokenPrefix='@@', $TokenSuffix = $TokenPrefix)
	
	$replace_args = @()
	$replace_args = @(
		$tokens.GetEnumerator() | %{ 
			$match = (@($TokenPrefix,$_.Name,$TokenSuffix) -join '') -replace '\$','\$'
			$value = $_.value
			( "-replace '$match','$value'") }
	) -join " "

	@(gci $sourceDirectory -Filter "$TemplateFilter" -Name) | %{ 
		$template = "$sourceDirectory" + "\" + $_
		$new_name = $_ -replace ".template$",""
		$target = $targetdirectory + "\" + $new_name
		$source_file = (Get-Content $template)
		$expr = "`$source_file $replace_args"
		$result = iex $expr
		Write-Output @{
			Script= $result
			Target= $target;
		}
	 } | %{ 
		sc $_.Target -Value $_.Script
	 }
}
task clean {
	if( Test-Path -path $build_nuget_package ){
		rd $build_nuget_package -recurse -force
	}
	RunMsBuild -task "Clean"
}
task compile -depends Clean {
	RunMsBuild -task "Build"
}

task setVersionInfo {
	$versionInfoFile = Convert-Path "src\VersionAssemblyInfo.cs"
	$versionInfo = 'using System.Reflection; [assembly: AssemblyFileVersion("' + $nuget_version_number + '")]';
	Set-Content $versionInfoFile $versionInfo
}

task emptyNugetTemplate {

	if( Test-Path -path "$build_nuget_package" ){
	ri $build_nuget_package -recurse -force
	}
	md $build_nuget_package
	md "$build_nuget_package/Sitecore.ItemBuckets/lib/net40"
	md "$build_nuget_package/Sitecore.ItemBuckets/config/App_Config/Include"
	md "$build_nuget_package/Sitecore.ItemBuckets/items"
	md "$build_nuget_package/Sitecore.ItemBuckets.UI/Content"
	md "$build_nuget_package/Sitecore.ItemBuckets.UI/lib/net40"
	md "$build_nuget_package/Sitecore.BigData/lib/net40"

}

task nugetTemplate -depends emptyNugetTemplate {

	"Core", "Master" | %{ 
		$scope = $_
		$project = Convert-Path "src/ItemBucket.$scope/Sitecore.ItemBucket.$scope.scproj"
		$target = Convert-Path "$build_nuget_package/Sitecore.ItemBuckets/items"
		CreateSitecorePackage -humanName "Item.Buckets.$scope" -project $project -target_directory $target
	}

	"BigData.Solr" | %{ 
		$scope = $_
		$project = Convert-Path "Sitecore.ItemBucket.$scope/Sitecore.ItemBucket.$scope.scproj"
		
		CreateSitecorePackage -humanName "Item.Buckets.$scope" -project $project -target_directory $target
	}
	
	
	$tmpDir = Create-Temp-Directory
	RunMsBuild -task "Build" -outputFolder $tmpDir -projectOrSolution "src/Sitecore.BigData/Sitecore.BigData.csproj"
	cp "$tmpDir/Sitecore.BigData.*" "$build_nuget_package/Sitecore.BigData/lib/net40"

	$tmpDir = Create-Temp-Directory
	RunMsBuild -task "Build" -outputFolder $tmpDir -projectOrSolution "Sitecore.ItemBuckets.BigData/Sitecore.ItemBuckets.BigData.csproj"
	cp "$tmpDir/Sitecore.ItemBuckets.BigData.*" "$build_nuget_package/Sitecore.BigData/lib/net40"

	$tmpDir = Create-Temp-Directory
	RunMsBuild -task "Build" -outputFolder $tmpDir -projectOrSolution "src/ItemBucket.Kernel/Sitecore.ItemBucket.Kernel.csproj"
	cp "$tmpDir/Sitecore.ItemBucket.Kernel.*" "$build_nuget_package/Sitecore.ItemBuckets/lib/net40"
	cp "$tmpDir/SolrNet*" "$build_nuget_package/Sitecore.ItemBuckets/lib/net40"
	cp "$tmpDir/Microsoft.Practices.ServiceLocation*" "$build_nuget_package/Sitecore.ItemBuckets/lib/net40"
	
	$tmpDir = Create-Temp-Directory
	RunMsBuild -task "Build" -outputFolder $tmpDir -projectOrSolution "Website/Sitecore.ItemBucket.UI.csproj"
	RoboCopy "$tmpDir/_PublishedWebsites/Sitecore.ItemBucket.UI" "$build_nuget_package/Sitecore.ItemBuckets.UI/Content" /S /XD bin /XD ItemGenerator

	cp "$tmpDir/_PublishedWebsites/Sitecore.ItemBucket.UI/bin/Sitecore.ItemBucket.UI.dll" "$build_nuget_package/Sitecore.ItemBuckets.UI/lib/net40" 
	
	cp "./Website/App_Config/Include/Sitecore.ItemBuckets*.config" "$build_nuget_package/Sitecore.ItemBuckets/config/App_Config/Include" 
	
	Copy-TemplatedFiles -sourceDirectory "templates" -targetDirectory "$build_nuget_package/Sitecore.ItemBuckets" -tokens @{ "version"=$nuget_version_number } -TemplateFilter "Sitecore.ItemBuckets.nuspec.template"
	Copy-TemplatedFiles -sourceDirectory "templates" -targetDirectory "$build_nuget_package/Sitecore.ItemBuckets.UI" -tokens @{ "version"=$nuget_version_number } -TemplateFilter "Sitecore.ItemBuckets.UI.nuspec.template"
	Copy-TemplatedFiles -sourceDirectory "templates" -targetDirectory "$build_nuget_package/Sitecore.BigData" -tokens @{ "version"=$nuget_version_number } -TemplateFilter "Sitecore.BigData.nuspec.template"
}
task nuget {
	$nuget_exe
}
task package -depends setVersionInfo, compile, nugetTemplate, nuget {
	Exec {
		. "$nuget_exe" pack $build_nuget_package/Sitecore.BigData/Sitecore.BigData.nuspec -OutputDirectory $build_nuget_package
	}
	Exec {
		. "$nuget_exe" pack $build_nuget_package/Sitecore.ItemBuckets.UI/Sitecore.ItemBuckets.UI.nuspec -OutputDirectory $build_nuget_package
	}
	Exec {
		. "$nuget_exe" pack $build_nuget_package/Sitecore.ItemBuckets/Sitecore.ItemBuckets.nuspec -OutputDirectory $build_nuget_package
	}

}

task publish -depends package {
	Exec {
    	. "$nuget_exe" push $build_nuget_package/Sitecore.BigData.$nuget_version_number.nupkg $nuget_api_key -CreateOnly
    }
	Exec {
		. "$nuget_exe" push $build_nuget_package/Sitecore.ItemBuckets.$nuget_version_number.nupkg $nuget_api_key -CreateOnly
	}
    Exec {
    	. "$nuget_exe" push $build_nuget_package/Sitecore.ItemBuckets.Client.$nuget_version_number.nupkg $nuget_api_key -CreateOnly
    }
}

task install -depends package {
	
}