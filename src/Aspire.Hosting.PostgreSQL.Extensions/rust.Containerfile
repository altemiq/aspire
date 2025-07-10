COPY --from=plrust-build /usr/share/postgresql/${PG_MAJOR}/extension /usr/share/postgresql/${PG_MAJOR}/extension
COPY --from=plrust-build /usr/lib/postgresql/${PG_MAJOR}/lib /usr/lib/postgresql/${PG_MAJOR}/lib
COPY --from=plrust-build --chown=postgres:postgres /plrust /plrust
COPY --from=plrust-build --chown=postgres:postgres /var/lib/postgresql/.cargo/bin /var/lib/postgresql/.cargo/bin
COPY --from=plrust-build --chown=postgres:postgres /var/lib/postgresql/.rustup/toolchains /var/lib/postgresql/.rustup/toolchains
COPY --from=plrust-build --chown=postgres:postgres /var/lib/postgresql/.rustup/settings.toml /var/lib/postgresql/.rustup/settings.toml

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
      build-essential \
      gcc \
      clang \
      ca-certificates \
      postgresql-server-dev-$PG_MAJOR && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="/var/lib/postgresql/.cargo/bin:${PATH}"