#! /usr/bin/sudo /bin/sh
MNGDIR=/media/bigdisk/games/steamapps/common/RimWorld/RimWorldLinux_Data/Managed

docker build -t rimmod . && docker run -v `pwd`:/root -v $MNGDIR:/root/Source/Lib:ro --workdir /root/Source -it rimmod 
