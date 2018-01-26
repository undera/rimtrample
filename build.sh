#! /usr/bin/sudo /bin/sh
docker build -t rimmod . && docker run -v /tmp/bzt-artifacts/docker:/tmp/artifacts --entrypoint xbuild -it rimmod 
