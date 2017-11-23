#!/bin/bash

RUN="node-docker-spinup.sh"
URL="https://gist.github.com/f63b8e006a1bbed1a090c8e4240d1f91.git"
TMP="tmp"

if [ ! -f ${RUN} ]; then
	git clone "${URL}" ${TMP}
	mv ${TMP}/${RUN} .
	chmod +x ${RUN}
	rm -rf ${TMP}
fi

./${RUN} i ws
./${RUN} r

