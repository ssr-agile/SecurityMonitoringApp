#!/bin/bash
curl -k -s -o /dev/null -w "%{http_code}" https://localhost:9200 -u admin:IndexerP@ss123! | grep -q 200