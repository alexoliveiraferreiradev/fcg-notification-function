#!/usr/bin/env bash
# Cria os tópicos (nomes fixados com SetEntityName nas APIs) e a subscription
# "notification" usada pelos ServiceBusTrigger da function.
# Uso: ./create-servicebus-subscriptions.sh <resource-group> <namespace>
set -euo pipefail

RESOURCE_GROUP="$1"
NAMESPACE="$2"
SUBSCRIPTION="notification"

TOPICS=(user-created user-republished payment-approved purchase-rejected delivery-failed)

for topic in "${TOPICS[@]}"; do
  az servicebus topic create -g "$RESOURCE_GROUP" --namespace-name "$NAMESPACE" -n "$topic" --output none
  az servicebus topic subscription create -g "$RESOURCE_GROUP" --namespace-name "$NAMESPACE" \
    --topic-name "$topic" -n "$SUBSCRIPTION" --max-delivery-count 10 --output none
  echo "ok: $topic/$SUBSCRIPTION"
done
