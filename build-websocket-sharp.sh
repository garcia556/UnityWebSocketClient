#!/bin/bash

# requires Mono installed and msbuild in PATH

PROJ="websocket-sharp"
DIR_BIN="UnitySrc/Assets/Plugins/WebSocket"

git submodule update
msbuild /p:Configuration=Release ${PROJ}/${PROJ}
cp "${PROJ}/${PROJ}/bin/Release/${PROJ}.dll" ${DIR_BIN}

