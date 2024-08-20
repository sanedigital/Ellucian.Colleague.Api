# Copyright 2014-2024 Ellucian Company L.P. and its affiliates.
# Version 2.0.3 (Delivered with Colleague Web API 2.0.3)

# PURPOSE:
# Warm up the Colleague Web API by pre-loading the most frequently-used and shared API data
# repository caches to reduce initial load time.

# PARAMETERS:
# -webApiBaseUrl: Full URL of the Colleague Web API instance to be warmed-up.
# -userId: Colleague Web username to use when running the warm-up.
# -password: Password for the above Colleague Web username.
# -recycleAppPool: If windows, this should be the name of the IIS application pool to be recycled prior to running
#    the warm-up script. If on linux, app pools do not exist so this should have some value to indicate that the 
#    kestrel server should be restarted. For either OS, leaving this value blank indicates that there should be no recycle/restart.
# -runEthosApi : parameter to run Ethos API

# EXAMPLE PYTHON COMMANDS:
# - Run warmups with no recycle and no ethos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' 'loginID' 'password'
# - Run warmups with recycle and no ethos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' 'loginID' 'password' 'appPoolName'
# - Run warmups with no recycle and include ehtos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' 'loginID' 'password' -runEthosApi
# - Run warmups with recycle and include ethos
#   PS C:\scripts> python .\WarmUp.py 'http://serverAddress:1234/ColleagueApi' 'loginID' 'password' 'appPoolName' -runEthosApi

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
# 2. The endpoints used in the delivered script do not require special user permission/roles or
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
# 		   status other than 200# 4. In order to use the -recycleAppPool option this script MUST be run from the Colleague Web API
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

import argparse
import csv
import datetime
import os
import requests
import sys
import time

symbols = {
    "CHECKMARK": "\033[92m {}\033[00m" .format(u'\u2713'),
    "XMARK": "\033[91m {}\033[00m" .format(u'\u2717'),
}

# Read endpoints from csv file. File should be in same dirctory as this script
def getEndpoints(fileName:str):
    startDate = datetime.datetime(datetime.datetime.now().year - 1, 1, 1)
    endpoints = []
    filePath = os.path.join(sys.path[0], fileName)
    with open(filePath, newline='') as csvfile:
        reader = csv.reader(csvfile, delimiter=',')
        for row in reader:
            if '{startDate}' in row[1]:
                row[1] = row[1].replace('{startDate}', str(startDate))
                row[5] = row[5].replace('{startDate}', str(startDate))
            
            # Convert string header to key, value array
            headers = convertStringToDictionary(row[4])

            # Convert expected to fail value to a boolean
            expectFail = row[6].upper() == 'TRUE' if True else False
            
            endpoints.append({'method': row[0], 'url': row[1], 'body': row[2], 'accept': row[3], 'headers': headers, 'message': row[5], 'expectFail': expectFail})

    return endpoints

def convertStringToDictionary(headerString:str):
    headerDict = dict()

    if(headerString):
        splitString = headerString.replace('[','').replace(']','').replace('{','').replace('}','').split(',')

        for str in splitString:
            keyVal = str.split(':')
            headerDict[keyVal[0].strip()] = keyVal[1].strip()

    return headerDict

# Send HTTP request to endpoint
def sendRequest(endpoint:dict, baseUrl:str, token:str):
    try:
        if token:
            endpoint['headers']['X-CustomCredentials'] = token

        if endpoint['accept']:
            endpoint['headers']['Accept'] = endpoint['accept']

        endpoint['headers']['Content-Type'] = 'application/json'
        endpoint['url'] = baseUrl + endpoint['url']

        response = requests.Response()

        if endpoint['method'].upper() == "POST":
            response = requests.post(endpoint['url'], headers=endpoint['headers'], data=endpoint['body'], timeout=300)
        elif endpoint['method'].upper() == "GET":
            response = requests.get(endpoint['url'], headers=endpoint['headers'], timeout=300)

        if response.status_code == 401:
            print(symbols['XMARK'], response.status_code, response.reason)
            print('Exiting program due to invalid credentials...')
            exit()

        if response.status_code != 200 and not endpoint['expectFail']:
            print(symbols['XMARK'], response.status_code, response.reason)
            return

    except Exception as e:
        print(symbols['XMARK'], e)
        return

    print(symbols['CHECKMARK'])
    
# Execute all endpoints in list
def executeApiCalls(endpointList:list, baseUrl:str, token:str):
    for endpoint in endpointList:
        try:
            print(endpoint['message'], end=' ')
            sendRequest(endpoint, baseUrl, token)
        except Exception as e:
            print(e)

# Set and parse arguments
argParser = argparse.ArgumentParser(description='Warm up the Colleague Web API by pre-loading the most frequently-used and shared API data repository caches to reduce initial load time.')
argParser.add_argument('webApiBaseUrl', help='Full URL of the Colleague Web API instance to be warmed-up.')
argParser.add_argument('userId', help='Colleague Web username to use when running the warm-up.')
argParser.add_argument('password', help='Password for the corresponding Colleague Web username.')
argParser.add_argument('recycleAppPool', nargs='?', default='', help='The name of the IIS application pool to be recycled prior to running the warm-up script.')
argParser.add_argument('-runEthosApi', action='store_true', help='Parameter to run Ethos API')

webApiBaseUrl = str(argParser.parse_args().webApiBaseUrl)
userId = str(argParser.parse_args().userId)
password = str(argParser.parse_args().password)
recycleAppPool = str(argParser.parse_args().recycleAppPool)
runEthosApi = bool(argParser.parse_args().runEthosApi)

# Recycle application pool if supplied
if recycleAppPool:
    platform = sys.platform
    if platform == 'win32':
        print('Recycling application pool', recycleAppPool, '...', end=' ')
        # Add code to recycle the application pool using appropriate commands for your environment
        commandPath = '{}\\system32\\inetsrv\\appcmd.exe'.format(os.environ['systemroot'])

        if(os.path.isfile(commandPath)):
            command = '{} recycle apppool {}'.format(commandPath, recycleAppPool)
            os.system(command)
            print(symbols['CHECKMARK'])
        else:
            print('{} Cannot recycle because appcmd.exe cannot be found at {}'.format(symbols['XMARK'], commandPath))          
    elif platform in ['linux', 'linux2']:
        os.system('systemctl restart ellucian-collapi.service')

        # perform health/about api call to check if service is available
        isUp = False
        upCheckCount  = 0

        while(not isUp):
            if(upCheckCount  > 90): 
                print("ellucian-collapi service is not running... Skipping warmup run")
                exit()
            try:
                response = requests.get(url='{}/health'.format(webApiBaseUrl))
                if ((response.status_code == 200 and response.json()['Status']=="available")) or (response.status_code == 204):
                    print("ellucian-collapi service is up") 
                    isUp = True
                else:
                    time.sleep(2)
            except Exception as e:
                upCheckCount += 1
                print("ellucian-collapi service is restarting...")
                time.sleep(2)
                continue

# Login
try:
    print('Logging in...', end=' ')
    response = requests.post(url='{}/session/login'.format(webApiBaseUrl), json={'UserId': userId, 'Password': password})
    if response.status_code == 200:
        token = response.text.strip()
        print(print(symbols['CHECKMARK']))
    else:
        print(symbols['XMARK'], response.text)
        exit()   
except requests.exceptions.RequestException as e:
    print(symbols['XMARK'], e.response)
    exit()

# Perform API calls
endpoints = getEndpoints('WarmUpEndpoints.csv')
executeApiCalls(endpoints, webApiBaseUrl, token)

if runEthosApi:
    print('Ethos API Run Flag is set...')
    endpoints = getEndpoints('WarmUpEthosEndpoints.csv')
    executeApiCalls(endpoints, webApiBaseUrl, token)
    
#Logout
try:
    print('Logging out...', end=' ')
    requests.post('{}/session/logout'.format(webApiBaseUrl), None, token)
    print(symbols['CHECKMARK'])
    print('Done.')
except requests.exceptions.RequestException as e:
    print(symbols['XMARK'], e.response)
