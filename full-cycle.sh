killall RimWorldLinux.x86_64
./build.sh && ./copy.sh && ( steam steam://rungameid/294100 ; tail -F /tmp/rimworld_log )