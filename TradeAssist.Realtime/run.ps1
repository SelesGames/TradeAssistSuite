#param([string]$QuestionArgs="Is this a square container?")

# Docker image name for the application
$ImageName="tradeassist-realtime"

function Invoke-Docker-Run ([string]$DockerImage) {
	#echo "Asking $Question"
	Invoke-Expression "docker run -d --rm -p 1942:1942 $ImageName"
}

Invoke-Docker-Run -DockerImage $ImageName