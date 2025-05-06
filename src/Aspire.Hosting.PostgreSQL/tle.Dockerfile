ARG TLE_BRANCH=main

RUN apt-get update
RUN apt-get install -y -qq postgresql-server-dev-$PG_MAJOR
RUN apt-get install -y -qq \
    git \
    gcc \
    make \
    flex \
    libsasl2-modules-gssapi-mit \
    libkrb5-dev

RUN git clone https://github.com/aws/pg_tle.git /pg_tle --branch ${TLE_BRANCH} && \
    chown -R postgres /pg_tle

RUN PG_CONFIG=$(which pg_config) && \
    cd /pg_tle && \
    make && \
    make install