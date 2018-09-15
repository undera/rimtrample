#! /bin/sh -xe

ROOT=/media/bigdisk/games/steamapps/common/RimWorld/Mods
DEST=$ROOT/rimtrample

rm -rf $DEST
mkdir -p $DEST
cp -r ./About $DEST/
rm -f ./Assemblies/*.pdb
cp -r ./Assemblies $DEST/
#cp -r ./Defs $DEST/
#cp -r ./Textures $DEST/
#cp -r ./Languages $DEST/
