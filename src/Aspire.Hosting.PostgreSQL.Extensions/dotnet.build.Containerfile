FROM postgres:15.1-bullseye AS pldotnet-build

ARG PL_DOTNET_BRANCH

RUN apt update && apt install -y git && git -c advice.detachedHead=false clone https://github.com/Brick-Abode/pldotnet /app/pldotnet --depth 1 --branch ${PL_DOTNET_BRANCH}
RUN cd /app/pldotnet && git submodule set-url dotnet_src/npgsql https://github.com/Brick-Abode/npgsql.git && git submodule update --init --recursive /app/pldotnet
WORKDIR /app/pldotnet

# Do all the things
RUN apt update && apt install -y make && make -f /app/pldotnet/docker/scripts/Makefile-builder deb_build

RUN make pldotnet-build-debian-packages