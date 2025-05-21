COPY --from=tle-build /usr/share/postgresql/${PG_MAJOR}/extension /usr/share/postgresql/${PG_MAJOR}/extension
COPY --from=tle-build /usr/lib/postgresql/${PG_MAJOR}/lib /usr/lib/postgresql/${PG_MAJOR}/lib
COPY --from=tle-build --chown=postgres:postgres /pg_tle /pg_tle

# get make to be able to install the examples
RUN apt-get update && \
    apt-get install -y -qq make && \
    rm -rf /var/lib/apt/lists/*

# enable making the examples by linking /bin/sh to /bin/bash
RUN mv /bin/sh /bin/sh.original && ln -s /bin/bash /bin/sh