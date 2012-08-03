function Install-PsGet(){
	param([parameter(mandatory=$true)]$modulePath)
	import-module PsGet -ErrorVariable err -ErrorAction SilentlyContinue
	if($err)
	{
		new-item ($modulePath + "\PsGet\") -ItemType Directory -Force | out-null
	    Write-Host Downloading PsGet from https://github.com/chaliy/psget/raw/master/PsGet/PsGet.psm1
	    (new-object Net.WebClient).DownloadFile("https://github.com/chaliy/psget/raw/master/PsGet/PsGet.psm1", $modulePath + "\PsGet\PsGet.psm1")
	    Write-Host "PsGet is installed and ready to use" -Foreground Green
	    Write-Host "USAGE:"
	    Write-Host "    import-module PsGet"
	    Write-Host "    install-module https://github.com/chaliy/psurl/raw/master/PsUrl/PsUrl.psm1"
	    Write-Host ""
	    Write-Host "For more details:"
	    Write-Host "    get-help install-module"
	    Write-Host "Or visit http://psget.net"
	    import-module PsGet
	}

}

$vendor_path = (Convert-Path "./modules/vendor")
$Env:PSModulePath = $vendor_path + ";" + $Env:PsmodulePath

function Setup-Bundled-Modules(){
	param( $modes = @("bootstrap", "install"))
	if ($modes){
		$modes | %{
			switch( $_ ){
				"bootstrap" { 
					Install-PsGet "$vendor_path"
					break
				}
				"install" {
					install-module psake
				}
			}
		}
	}
}

function Include-Relative(){
	param([string]$path)
	$parent = (Split-Path ((Get-Variable MyInvocation -Scope 0).Value.ScriptName) -Parent )
	$path = $parent + ".\$path"
	. $path
}


