#! /usr/bin/sudo /bin/sh
MNGDIR=/media/bigdisk/games/steamapps/common/RimWorld/RimWorldLinux_Data/Managed

docker build -t rimmod . && docker run -v $MNGDIR:/tmp/artifacts:ro --entrypoint xbuild -it rimmod 
