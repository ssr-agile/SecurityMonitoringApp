#!/bin/bash

# Start OpenSearch
/usr/share/wazuh-indexer/bin/opensearch &

# Wait for OpenSearch to be ready
while ! curl -k https://localhost:9200 -u admin:$OPENSEARCH_INITIAL_ADMIN_PASSWORD >/dev/null 2>&1; do
  sleep 30
done

# Initialize security indices (critical fix!)
/usr/share/wazuh-indexer/bin/opensearch-security-admin.sh \
  -cd /usr/share/wazuh-indexer/plugins/opensearch-security/securityconfig/ \
  -icl -nhnv \
  -cacert /etc/ssl/root-ca.pem \
  -cert /etc/ssl/admin.pem \
  -key /etc/ssl/admin-key.pem \
  -h 127.0.0.1

# Keep container running
tail -f /dev/null