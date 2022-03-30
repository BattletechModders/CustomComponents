#!/bin/bash

export PATH="/c/Program Files/7-Zip/:$PATH"

SEVENZIP="7z"

set -ex

CCZIP="CustomComponents/dist/CustomComponents.zip"
rm -f "$CCZIP"

cd ..

(
cd CustomComponents/source
#git describe --exact-match
dotnet build --configuration Release --no-incremental -p:OutputPath=../ "$@"
)

(
INCLUDES="-i!CustomComponents/CustomComponents.dll -i!CustomComponents/LICENSE -i!CustomComponents/mod.json -i!CustomComponents/mod.minimal.json -i!CustomComponents/README.md -ir!CustomComponents/data"

"$SEVENZIP" a -tzip -mx9 "$CCZIP" $INCLUDES
)
