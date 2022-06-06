$zipDstPath = ".\KAG.DarkRift\KAG.DarkRift.Server.Console.zip"

if (Test-Path $zipDstPath -PathType Leaf) {
	Remove-Item $zipDstPath
}

Get-ChildItem -Path ".\KAG.DarkRift\*" -Force | Compress-Archive -DestinationPath $zipDstPath -Force

$secretKey = Get-Content -Path "$($env:APPDATA)\KAG\SecretKey.txt"

echo "Connecting to playfab multiplayer API..."
try{
	Set-PfTitle -TitleID "B76E5" -SecretKey $secretKey
	Enable-PfMultiplayerServer
}
catch{
	echo "Failed to connect to playfab multiplayer API."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully connected to playfab multiplayer API."
echo "`n"

echo "Uploading latest server build..."
try{
	Remove-PfAsset -FileName "DarkRift.Server.Console.zip"
	New-PfAsset -FilePath "D:\Work\Git\KAG\KAG.DarkRift\DarkRift.Server.Console.zip" -AssetName "DarkRift.Server.Console.zip"
}
catch{
	echo "Either failed to remove existing build or could not upload the latest one."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully uploaded latest server build."
echo "`n"

echo "Creating a new build..."
try{
	$vmSize = "Standard_D2as_v4"
	$regions = @(
		@{
			StandbyServers=2;
			MaxServers=4;
			Region ="NorthEurope";
			ScheduledStandbySettings=$NULL 
		}
	)
	$ports = @(
		@{
			Name="KAG_tcp";
			Num=4296;
			Protocol="TCP"
		},
		@{
			Name="KAG_udp";
			Num=4296;
			Protocol="UDP" 
		}
	)
	$gameAssets = @(
		@{
			FileName="DarkRift.Server.Console.zip"; 
			MountPath="C:\Assets" 
		}
	)
	
	$buildResponse = New-PfBuild -BuildName "Playtest" -ContainerFlavor ManagedWindowsServerCorePreview -StartMultiplayerServerCommand "C:\Assets\DarkRift.Server.Console.exe" -GameAssetReferences $gameAssets -VMSize $vmSize -MultiplayerServerCountPerVM 1 -Ports $ports -RegionConfigurations $regions	
}
catch{
	echo "Failed to create a new build."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully created a new build."
echo "`n"

echo "Authenticating to playfab rest API..."
try{
	$headers = @{
		"X-SecretKey"=$secretKey
	}
	
	$authenticationResponse = Invoke-RestMethod -Method "Post" -Uri "https://B76E5.playfabapi.com/Authentication/GetEntityToken" -Headers $headers -ContentType "application/json"
}
catch{
	echo "Failed to authenticate to playfab rest API."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully authenticated to playfab rest API."
echo "`n"

echo "Creating matchmaking queue..."
try{
	$headers = @{
		"X-EntityToken"=$authenticationResponse.data.EntityToken
	}
	$body = @{
		"MatchMakingQueue"=@{
			"BuildId"=$buildResponse.data.BuildId;
			"ServerAllocationEnabled"="true";
			"Name"="DefaultQueue";
			"MinMatchSize"=3;
			"MaxMatchSize"=3;
			"RegionSelectionRule"=@{
				"Name"="region";
				"Path"="Latencies";
				"MaxLatency"=500;
				"Weight"=1
			}
		}
	}
	
	Invoke-RestMethod -Method "Post" -Uri "https://B76E5.playfabapi.com/Match/SetMatchmakingQueue" -Headers $headers -Body ($body|ConvertTo-Json) -ContentType "application/json"
}
catch{
	echo "Failed to create matchmaking queue."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully created matchmaking queue."
echo "`n"

Read-Host -Prompt "Done. Press enter to exit"
exit
