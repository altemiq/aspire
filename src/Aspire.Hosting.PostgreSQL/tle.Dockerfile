ARG TLE_BRANCH=main

RUN apt-get update && \
    apt-get install -y -qq postgresql-server-dev-$PG_MAJOR && \
    apt-get install -y -qq \
    git \
    gcc \
    make \
    flex \
    libsasl2-modules-gssapi-mit \
    libkrb5-dev && \
    git -c advice.detachedHead=false clone https://github.com/aws/pg_tle.git /pg_tle --branch ${TLE_BRANCH} && \
    chown -R postgres /pg_tle && \
    PG_CONFIG=$(which pg_config) && \
    cd /pg_tle && \
    make && \
    make install