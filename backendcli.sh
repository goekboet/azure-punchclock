#! /bin/bash

BASEURL=https://localhost:5001/api/punchclock

function list {
    curl -k -v -X GET $BASEURL 
}

function get {
    curl -k -v -X GET ${BASEURL}/${1}
}

function new {
    curl -k -v -X POST $BASEURL \
    -H "Content-Length: 0"
}

function punch {
    curl -k -v -X POST ${BASEURL}/${1} \
    -H "Content-Length: 0"
}

function remove {
    curl -k -v -X DELETE ${BASEURL}/${1} \
    -H "Content-Length: 0"
}