#!/bin/bash

export PATH="/c/Program Files/7-Zip/:$PATH"

SEVENZIP="7z"

set -ex

(
cd source
dotnet build --configuration Release --no-incremental -p:OutputPath=../ "$@"
)

(
cd ..

CCZIP="CustomComponents/dist/CustomComponents.zip"
INCLUDES="-i!CustomComponents/CustomComponents.dll -i!CustomComponents/LICENSE -i!CustomComponents/mod.json -i!CustomComponents/mod.minimal.json -i!CustomComponents/README.md -ir!CustomComponents/data"

rm -f "$CCZIP"
"$SEVENZIP" a -tzip -mx9 "$CCZIP" $INCLUDES
)
