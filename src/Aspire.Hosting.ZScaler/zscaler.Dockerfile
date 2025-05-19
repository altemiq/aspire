RUN apt-get update && \
    apt-get install -y ca-certificates && \
    # get the certificates, and then split them into separate files as `update-ca-certificates` does not like files with more than one certificate in it
    openssl s_client -showcerts -verify 5 -connect zscaler.com:443 < /dev/null 2>/dev/null | awk 'BEGIN {c=0;} /BEGIN CERT/{c++} { print > "/tmp/zscaler." c ".crt"}' && \
    # this one does not contain a certificate, just the start of the file before the first certificate
    rm /tmp/zscaler.0.crt && \
    # strip out any extra lines, and ensure that the files are in PEM format
    for file in /tmp/zscaler.*.crt; do cat $file | openssl x509 -outform PEM > /usr/local/share/ca-certificates/$file.crt; done && \
    # remove the intermediate files
    rm /tmp/zscaler.*.crt && \
    # update the certificates
    update-ca-certificates