# modified from https://github.com/tcdi/plrust/blob/main/Dockerfile.try

ARG PL_RUST_BRANCH=main

# Install just enough to set up the official Postgres debian repository,
# then install everything else needed for Rust and plrust
RUN echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections && \
    apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        gnupg \
        lsb-release \
        wget \
        software-properties-common && \
    sh -c 'echo "deb https://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list' && \
    wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor | tee /etc/apt/trusted.gpg.d/apt.postgresql.org.gpg >/dev/null && \
    wget https://apt.llvm.org/llvm.sh && chmod +x llvm.sh && ./llvm.sh && rm ./llvm.sh && \
    apt-get update -y -qq --fix-missing && \
    apt-get install -y --no-install-recommends \
        build-essential \
        gcc \
        git \
        jq \
        libssl-dev \
        make \
        ruby \
        postgresql-server-dev-$PG_MAJOR \
        pkg-config && \
    rm -rf /var/lib/apt/lists/*

# Set up permissions so that the postgres user can install the plrust plugin
RUN chmod a+rwx `$(which pg_config) --pkglibdir` `$(which pg_config) --sharedir`/extension

# Install fpm for the creation of the .deb file,
# and install toml so TOML files can be parsed later
RUN gem install --no-document fpm toml

RUN --mount=type=bind,source=0001-fix-version.patch,target=/tmp/0001-fix-version.patch \
    git clone -c advice.detachedHead=false https://github.com/tcdi/plrust.git /plrust --depth 1 --branch ${PL_RUST_BRANCH} && \
    chown -R postgres /plrust && \
    cd /plrust && git apply /tmp/0001-fix-version.patch

# The 'postgres' user is the default user that the official postgres image sets up
USER postgres
ENV USER=postgres

# Copy in plrust source
WORKDIR /plrust

# Obtain the toolchain version from rust-toolchain.toml and store that into a file.
RUN ruby <<EOF
require 'toml'
toml=TOML.load_file('/plrust/rust-toolchain.toml')
if ver=toml['toolchain']['channel']
  File.open('/tmp/.toolchain-ver', 'w') { |file| file.write(ver) }
else
  raise 'Could not determine toolchain channel version. Is rust-toolchain.toml missing or malformed?'
end
EOF

## Install Rust
RUN TOOLCHAIN_VER=`cat /tmp/.toolchain-ver` && wget -qO- https://sh.rustup.rs | sh -s -- -y --profile minimal --default-toolchain=$TOOLCHAIN_VER    
ENV PATH="/var/lib/postgresql/.cargo/bin:${PATH}"

RUN PGRX_VERSION=$(cargo metadata --format-version 1 | jq -r '.packages[]|select(.name=="pgrx")|.version') && \
    cargo install cargo-pgrx --locked --force --version "$PGRX_VERSION" && \
    rustup component add llvm-tools-preview rustc-dev && \
    cd /plrust/plrustc && ./build.sh && cp ../build/bin/plrustc ~/.cargo/bin && \
    export PG${PG_MAJOR}_PG_CONFIG=$(which pg_config) && cargo pgrx init && \
    cd /plrust/plrust && STD_TARGETS="$(uname -m)-postgres-linux-gnu" ./build && \
    cargo pgrx install --release --features trusted -c $(which pg_config) && \
    cd /plrust && find . -type d -name target | xargs rm -r && \
    rustup component remove llvm-tools-preview rustc-dev

# Reset the permissions of files/directories that was created or touched by the postgres user.
# Switching to the root user here temporarily is easier than installing and setting up sudo
USER root

RUN chmod 0755 `$(which pg_config) --pkglibdir` && \
    cd `$(which pg_config) --pkglibdir` && \
    chown root:root *.so && \
    chmod 0644 *.so && \
    chmod 755 `$(which pg_config) --sharedir`/extension && \
    cd `$(which pg_config) --sharedir`/extension && \
    chown root:root *

USER postgres

WORKDIR /