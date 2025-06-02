# avoids tzdata prompt
ARG DEBIAN_FRONTEND=noninteractive

# Install PostgreSQL and other dependencies
RUN apt update && \
    apt install -y \
    wget \
    libglib2.0-dev \
    lsb-release

# Install Dotnet
RUN wget -q \
    https://packages.microsoft.com/config/$(lsb_release -i | cut -f2 -d'	' | sed 's/.*/\L&/')/$(lsb_release -r | cut -f2 -d'	')/packages-microsoft-prod.deb \
	-O packages-microsoft-prod.deb && \
	dpkg -i packages-microsoft-prod.deb && \
	rm packages-microsoft-prod.deb

RUN apt update && \
    apt install -y \
    dotnet-sdk-6.0 \
    dotnet-runtime-6.0 \
    dotnet-hostfxr-6.0

COPY --from=pldotnet-build /app/pldotnet/debian/packages /pldotnet/packages

RUN dpkg -i /pldotnet/packages/postgresql-${PG_MAJOR}-pldotnet_*_amd64.deb

RUN apt-get clean

COPY --from=pldotnet-build /app/pldotnet/docker/scripts/install_pldotnet_deb.sh /docker-entrypoint-initdb.d/install_pldotnet_deb.sh