# Copyright 2014-2024 Ellucian Company L.P. and its affiliates.
# Version 2.0.3 (Delivered with Colleague Web API 2.0.3)

# PURPOSE:
# Warm up the Colleague Web API by pre-loading the most frequently-used and shared API data
# repository caches to reduce initial load time.

# PARAMETERS:
# -webApiBaseUrl: Full URL of the Colleague Web API instance to be warmed-up.
# -userId: Colleague Web username to use when running the warm-up.
# -password: Password for the above Colleague Web username.
# -recycleAppPool: The name of the IIS application pool to be recycled prior to running
#    the warm-up script.
# -runEthosApi : parameter to run Ethos API

# EXAMPLE POWERSHELL COMMANDS:
# - Run warmups with no recycle and no ethos
#   PS C:\scripts> .\WarmUp.ps1 "http://serverAddress:1234/ColleagueApi" "loginID" "password"
# - Run warmups with recycle and no ethos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' "loginID" "password" "appPoolName"
# - Run warmups with no recycle and include ehtos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' "loginID" "password" -runEthosApi
# - Run warmups with recycle and include ethos
#   PS C:\scripts> .\WarmUp.ps1 "http://serverAddress:1234/ColleagueApi" "loginID" "password" "appPoolName" -runEthosApi

# RECOMMENDED USAGE:
# The data caches maintained by Colleague Web API are not retained when its application pool
# is recycled. Accordingly, there can be a noticeable delay in the responsiveness of applications
# that make use of the Colleague Web API. This may result in a poor user experience for whoever 
# happens to access the application first. To alleviate this, IT administrators should consider 
# running this script at least once every 24 hours, during off-peak time or just after daily
# Colleague backup activities. The script can be run more frequently as well.
#
# The script can be used with or without an option that performs a recycle of the Colleague Web
# API's IIS application pool. When using the -recycleAppPool option the traditional recycling
# settings within IIS can be set to not recycle on periodic bases (no regular time interval or 
# specific times) and to never time out (idle time-out) and a scheduled run of this script with 
# the -recycleAppPool can be used instead to ensure that the application pool recycle and 
# warm-up happen at the same time rather than trying to coordinate a scheduled task just after
# a periodic application pool recycle or worse not warming-up due to an idle time-out shutting-
# down the application pool. The suggested usage of the -recycleAppPool option is to create a scheduled task that runs 
# once a day, during off-peak time or just after Colleague backup activities are finished that
# uses the -recycleAppPool option and then, optionally if you wish to run the warm-up periodically
# throughout the day, create another scheduled task that does not use the -recycleAppPool so that
# the application pool is not being recycled during the normal hours of the day.
#
# You can find when and how IIS automatically recycles the Colleague Web API application pool
# by right-clicking on the application pool in IIS Manager and choosing 'Recycling.'
#
# This script could be scheduled using the Windows Task Scheduler as described in Knowledge
# Article 000006250:
#   https://ellucian.force.com/clients/s/article/9304-Colleague-Web-API-Automated-Warm-Up

# NOTES: 
# 1. This script, as delivered by Ellucian, pre-loads some caches that are most frequently used
#    and shared. It does not pre-load all caches.
# 2. The endpoints used in the delivered CSV files do not require special user permission/roles or
#    valid input parameters. Therefore, no modification is necessary prior to running this script.
# 3. You may add to this script more warm up requests for your own endpoints that use caching,
#    or for any other endpoints that you deem necessary to improve performance. The CSV files have 
#    the following headers:
#    	a. method - Set request method to post or get. The script will need to be adjusted to include
#          additional methods
#    	b. url - Set to path of request. It will be combined with the base url.
#    	c. body - Set to raw body request.
#    	d. accept - Set type of data that can be received
#    	e. headers - Set request headers. Can be enclosed in brackets or left without.
#    	f. message - Set to the message you wish to be displayed with sending the request
#    	g. expectFail - Set this value to true if the request is expected to comeback with a response
# 		   status other than 200
# 4. In order to use the -recycleAppPool option this script MUST be run from the Colleague Web API
#    host web server.

# CAUTIONS:
# 1. You should take special care to select a user ID that does not have broad access to the
#    system (i.e. a user that is assigned many self-service roles), in case the credentials are
#    somehow compromised.
# 2. When adding additional warm up requests to this script, you should ensure the endpoints used
#    and any associated parameters are protected against unauthorized access.
# 3. This file will be overwritten when you perform an upgrade installation. For that reason,
#    it is recommended that you use a separate copy of the script for your customization needs.
# 4. Use of the -recycleAppPool option should only be done during off-peak times, such as in the
#    middle of the night or just after Colleague backup activities are complete. If you run this
#    script periodically throughout the day you should not use this option during those runs.
###

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$webApiBaseUrl,
	
	[Parameter(Mandatory=$True)]
	[string]$userId,
	
	[Parameter(Mandatory=$True)]
	[string]$password,
	
	[Parameter(Mandatory=$false)]
	[string]$recycleAppPool,
	
	[Parameter(Mandatory=$false)]
	[switch]$runEthosApi
)

$symbols = [PSCustomObject] @{ CHECKMARK = "$([char]0x1b)[92m$([char]8730) $([char]0x1b)[0m"; XMARK = "$([char]0x1b)[91m× $([char]0x1b)[0m" }

# Read endpoints from csv file. File should be in same dirctory as this script
function GetEndpoints([string]$fileName) {
	$csvHeaders = @("method", "url", "body", "accept", "headers", "message", "expectFail")
	$endpoints = Import-Csv (Join-Path $PSScriptRoot $fileName) -Header $csvHeaders 

	return $endpoints
}

function UpdateEndpoint([ref]$endpoint) {
	# Convert string header to key, value array
	$endpoint.Value.headers = ConvertHeaderString $endpoint.Value.headers

	# Convert expected to fail value to a boolean
	if ($endpoint.Value.expectFail -and $endpoint.Value.expectFail.ToUpper() -eq "TRUE") {
		$endpoint.Value.expectFail = $true
	} else {
		$endpoint.Value.expectFail = $false
	}

	# Replace templated start date
	$startDate = Get-Date -Year ((Get-Date).Year - 1) -Month 1 -Day 1 -Hour 0 -Minute 0 -Second 0 -Millisecond 0
	if ($endpoint.Value.url -like "*{startDate}*")
	{
		$endpoint.Value.url = $endpoint.Value.url.replace("{startDate}", $startDate.ToString("yyyy-MM-dd hh:mm:ss"))
		$endpoint.Value.message = $endpoint.Value.message.replace("{startDate}", $startDate.ToString("yyyy-MM-dd hh:mm:ss"))
	}
}

function ConvertHeaderString([string]$headerString) {
	$headerHash = @{}
	
	if ($headerString) {
		$headerString = $headerString.Trim("{","[","]","}"," ")
		$splitStr = $headerString.Split(",")

		foreach ($str in $splitStr) {
			$keyVal = $str.Split(":")
			$headerHash[$keyVal[0].Trim()] = $keyVal[1].Trim()
		}
	}

	return $headerHash
}

# Send HTTP request to endpoint
function SendRequest([object]$endPoint, [string]$baseUrl, [string]$token) 
{
	try {
		if ($token) {$endPoint.headers["X-CustomCredentials"] = $token}
		if ($endPoint.accept) {$endPoint.headers["Accept"] = $endPoint.accept}
		$endPoint.headers["Content-Type"] = "application/json"
		$endPoint.url = $baseUrl + $endPoint.url
		
		switch ($endpoint.method.ToUpper()) {
			"POST" { Invoke-RestMethod -Method Post -Uri $endPoint.url -Headers $endPoint.headers -Body $endPoint.body -TimeoutSec 300 > $null}
			"GET" { Invoke-RestMethod -Method Get -Uri $endPoint.url -Headers $endPoint.headers -TimeoutSec 300 > $null}
		}		
	}
	catch {
		if ($_.Exception.Response.StatusCode.value__ -eq 401)
		{
			Write-Host " $($symbols.XMARK)$($_.Exception.Response.StatusCode.value__) $($_.Exception.Response.StatusCode)"
			Write-Host "Exiting program due to invalid credentials..."
			Exit
		}
		if (!$endpoint.expectFail) {
			Write-Host " $($symbols.XMARK)$($_.Exception.Response.StatusCode.value__) $($_.Exception.Response.StatusCode)"
			return
		}
	}
	Write-Host " $($symbols.CHECKMARK)"
}

# Execute all endpoints in list
function ExecuteApiCalls([array]$endpoints, [string]$baseUrl, [string]$token) {
	foreach ($endpoint in $endpoints) {
		try {
			UpdateEndpoint ([ref]$endpoint)
			Write-Host $endpoint.message -NoNewline
			SendRequest $endpoint $baseUrl $token
		}
		catch { 
			Write-Host $_.Exception.Message
		}
	}
}

# Recycle application pool if supplied
if (![System.String]::IsNullOrEmpty($recycleAppPool)) {
	Write-Host "Recycling application pool $recycleAppPool..." -NoNewline
	$commandPath = [Environment]::GetEnvironmentVariable("systemroot") + "\system32\inetsrv\appcmd.exe"
	if (Test-Path $commandPath) {
		$command = "'$commandPath' recycle apppool '$recycleAppPool'"
		Invoke-Expression "& $command" > $null

		Write-Host " $($symbols.CHECKMARK)"
	}
	else {
		Write-Host " $($symbols.XMARK)Cannot recycle because appcmd.exe cannot be found at $commandPath"
	}
}

$headers = @{"Content-Type" = "application/json"}

# Login
try {
	Write-Host "Logging in..." -NoNewline
	$token = Invoke-RestMethod -Method Post -Uri "$webApiBaseUrl/session/login" -Headers $headers -Body "{'UserId':'$userId','Password':'$password'}" -TimeoutSec 300
	Write-Host " $($symbols.CHECKMARK)"
}
catch {
	Write-Host " $($symbols.XMARK)$($_.Exception.Message)"
	Exit
}

# Perform API calls
$endpoints = GetEndpoints "WarmUpEndpoints.csv"
ExecuteApiCalls $endpoints $webApiBaseUrl $token

if ($runEthosApi) {
    Write-Host  "Ethos API Run Flag is set..."
    $endpoints = GetEndpoints "WarmUpEthosEndpoints.csv"
    ExecuteApiCalls $endpoints $webApiBaseUrl $token
}

#Logout
try {
	Write-Host "Logging out..." -NoNewline
	$headers["X-CustomCredentials"] = $token
	Invoke-RestMethod -Method Post -Uri "$webApiBaseUrl/session/logout" -Headers $headers -TimeoutSec 300 > $null
	Write-Host " $($symbols.CHECKMARK)"
	Write-Host "Done."
}
catch {
	Write-Host " $($symbols.XMARK)$($_.Exception.Message)"
}

