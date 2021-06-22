#! /bin/bash

DOCKER_ORGANIZATION_NAME=$1
DOCKER_IMAGE_NAME=$2
DOCKER_CONTAINER_NAME=$2
DOCKER_IMAGE_AND_TAG=$3
CONTAINER_PORT=$4
INTERNAL_PORT=80


echo ======== variables and values ========;

echo DOCKER_ORGANIZATION_NAME=${DOCKER_ORGANIZATION_NAME}
echo DOCKER_IMAGE_NAME=${DOCKER_IMAGE_NAME}
echo DOCKER_CONTAINER_NAME=${DOCKER_CONTAINER_NAME}
echo DOCKER_IMAGE_AND_TAG=${DOCKER_IMAGE_AND_TAG}
echo CONTAINER_PORT=${CONTAINER_PORT}

echo ======== docker images ========;
docker images --format {{.Repository}}:{{.Tag}};

echo ======== docker containers ========;
docker ps --format {{.Names}};

echo ======== last docker container of this micro service ========;
docker ps -a -q --filter name=${DOCKER_CONTAINER_NAME}

echo ======== last docker images of this micro service ========;
docker images -q --filter reference=${DOCKER_ORGANIZATION_NAME}/${DOCKER_IMAGE_NAME};

echo ======== stop current container of this micro service ========;
docker ps -q --filter name=${DOCKER_CONTAINER_NAME} | grep -q -a . && docker stop ${DOCKER_CONTAINER_NAME} || echo Not Found Running Container With Name = ${DOCKER_CONTAINER_NAME} ;

echo ======== remove current container of this micro service ========;
docker ps -a -q --filter name=${DOCKER_CONTAINER_NAME} | grep -q -a . && docker rm -fv ${DOCKER_CONTAINER_NAME} || echo Not Found Stopped Container With Name = ${DOCKER_CONTAINER_NAME} ;

echo ======== remove last docker images of this micro service ========;
docker images -q --filter reference=${DOCKER_ORGANIZATION_NAME}/${DOCKER_IMAGE_NAME} | grep -q . && docker rmi $(docker images --format="{{.Repository}} {{.ID}}" |  grep "^${DOCKER_ORGANIZATION_NAME}/${DOCKER_IMAGE_NAME} " |  cut -d" " -f2 | tr "\r\n" " ") || echo Not Found Image With Name = ${DOCKER_IMAGE_NAME};

echo ======== pull docker image on server ========;
docker pull ${DOCKER_IMAGE_AND_TAG};

echo ======== run docker container ========;
docker run -p 46.227.254.20:${CONTAINER_PORT}:${INTERNAL_PORT} --name ${DOCKER_CONTAINER_NAME} --hostname ${DOCKER_CONTAINER_NAME} -d ${DOCKER_IMAGE_AND_TAG};
