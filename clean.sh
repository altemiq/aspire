find . -type d \( -name "bin" -or -name "obj" -or -name "TestResults" \) | xargs -I '{}' rm -rf {}
find . -maxdepth 1 -type d \( -name "nupkg" -or -name "versions" -or -name ".vs" \) | xargs -I '{}' rm -rf {}
find . -type f -name "*.binlog" | xargs -I '{}' rm -rf {}