foreach($line in docker ps -f status=running --format "{{.ID}}-{{.Image}}"){
	$split = $line -split('-')
	if ($split[1] -like 'mcr.microsoft.com/playfab/multiplayer:*') {
		docker rm --force $split[0]
		break
	}
}

exit