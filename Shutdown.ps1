echo "Authenticating to playfab rest API..."
try{
	$headers = @{
		"X-SecretKey"="4A58IERIMM8XEG5DXRK7FAQZ4XRBAYNJ69TIQA6NIBDTKDAPB1"
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

echo "Sending request to delete matchmaking queue..."
try{
	$headers = @{
		"X-EntityToken"=$authenticationResponse.data.EntityToken
	}
	$body = @{
		"QueueName"="DefaultQueue"
	}
	
	Invoke-RestMethod -Method "Post" -Uri "https://B76E5.playfabapi.com/Match/RemoveMatchmakingQueue" -Headers $headers -Body ($body|ConvertTo-Json) -ContentType "application/json"
}
catch{
	echo "Failed to send request to delete matchmaking queue."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully sent request to delete matchmaking queue."
echo "`n"

echo "Connecting to playfab multiplayer API..."
try{
	Set-PfTitle -TitleID "B76E5" -SecretKey "4A58IERIMM8XEG5DXRK7FAQZ4XRBAYNJ69TIQA6NIBDTKDAPB1"
	Enable-PfMultiplayerServer
}
catch{
	echo "Failed to connect to playfab multiplayer API."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}
echo "Successfully connected to playfab multiplayer API."
echo "`n"

echo "Removing running playtest build..."
try{
	$buildsResponse = Get-PfBuild
	$foundMatch = $false
	
	foreach($buildSummary in $buildsResponse.data.BuildSummaries){
		if($buildSummary.BuildName -eq "Playtest"){
			$foundMatch = $true
			Remove-PfBuild -BuildId $buildSummary.BuildId
			break
		}
	}
}
catch{
	echo "Either failed to collect builds or could not remove the playtest one."
	
	Read-Host -Prompt "Press enter to exit"
	exit
}

if($foundMatch){
	echo "Successfully removed running playtest build."
}
else{
	echo "No playtest build was found."
}
echo "`n"

Read-Host -Prompt "Done. Press enter to exit"
exit