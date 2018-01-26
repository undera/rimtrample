FROM ubuntu:16.04


RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb http://download.mono-project.com/repo/debian wheezy/snapshots/3.12.0 main" | tee /etc/apt/sources.list.d/mono-xamarin.list \
    && apt-get update \
    && apt-get -y install build-essential cli-common libgtk2.0-cil-dev libglade2.0-cil-dev libgnome2.0-cil-dev libgconf2.0-cil-dev 

RUN apt-get remove mono-complete mono-devel monodevelop \
    && apt-get -y install -t wheezy/snapshots/3.12.0 mono-complete \
    && mono --version