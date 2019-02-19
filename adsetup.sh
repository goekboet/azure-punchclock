#! /bin/bash

Appname="punchclock-api"

az ad app create --display-name $Appname --identifier-uris "https://$Appname"

