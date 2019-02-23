#! /bin/bash

BASEURL=https://localhost:5001/api/punchclock
TOKEN=""

function bearerHeader {
    if [ -z "$TOKEN" ]
    then
        echo "$TOKEN"
    else
        echo "Bearer $TOKEN"
    fi 
}

function signout {
    TOKEN=""
}

function signin {
    if [ $# -ne 2 ]
    then
        echo "useage: signin <username> <password>"
    else
        TOKEN=$(dotnet fetchToken.dll $1 $2)
    fi
}

function list {
    curl -k -i -H "Authorization:$(bearerHeader)" -X GET $BASEURL 
}

function get {
    curl -k -i -H "Authorization:$(bearerHeader)" -X GET ${BASEURL}/${1}
}

function new {
    curl -k -i -H "Authorization:$(bearerHeader)" -X POST $BASEURL \
    -H "Content-Length: 0"
}

function punch {
    curl -k -i -H "Authorization:$(bearerHeader)" -X POST ${BASEURL}/${1} \
    -H "Content-Length: 0"
}

function remove {
    curl -k -i -H "Authorization:$(bearerHeader)" -X DELETE ${BASEURL}/${1} \
    -H "Content-Length: 0"
}