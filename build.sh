#! /usr/bin/sudo /bin/sh
docker build -t rimmod . \
  && docker run -v `pwd`:/root -v /media:/media:ro --workdir /root/Source -it rimmod xbuild /p:Configuration=Release \
  && chown -R undera:undera .
