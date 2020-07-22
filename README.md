## Synopsis

This tool is written in C# and connects to the Microsoft Graph API and queries the presence of your user, then calls an API locally (NodeMCU, HUE light, etc).  In the event you cannot create an APP ID in Azure AD for your tenant, there is a workaround to access presence for a user in another domain.

## Motivation

KIDS! Working from home and having the family around all the time can pose some ackward moments when on meetings, presenting, video calls etc...  This process lights up an LED outside my office to give them real time indicators on if it's ok to enter.

## Pre-reqs
You need to generate a Client ID - Follow this tutorial to register an App in your tenant.  https://docs.microsoft.com/en-us/graph/auth-register-app-v2

If you do not have access to your company AAD tenant, follow the process and create the app in your own tenant and add the userID value of the user you want to monitor.
You can get your user ID here https://developer.microsoft.com/en-us/graph/graph-explorer 

## Installation

Edit the App.config file and enter the 4 values

clientID = "value from clientID after register app"<br />
userID = "*optional - add only if you register an app in a different tenant"<br />
apiPath = "path to local API that you will pass your status to via GET"<br />
timerSeconds = "how often to query the graph API"