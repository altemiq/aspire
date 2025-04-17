ARG IMAGE=postgres
ARG TAG=17

FROM ${IMAGE}:${TAG}

ARG POSTGRES_VERSION=-17
ARG TLE_BRANCH=main

RUN apt-get update
RUN apt-get install -y -qq postgresql-server-dev${POSTGRES_VERSION}
RUN apt-get install -y -qq git gcc make flex libsasl2-modules-gssapi-mit libkrb5-dev

RUN git clone https://github.com/aws/pg_tle.git pg_tle --branch ${TLE_BRANCH}

RUN cd pg_tle && PG_CONFIG=/usr/bin/pg_config make
RUN cd pg_tle && PG_CONFIG=/usr/bin/pg_config make install