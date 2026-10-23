export VAULT_ADDR=http://0.0.0.0:$VAULT_PORT

echo "Enabling KV secrets engine at path 'AntSoftVault'"
vault secrets enable -path='AntSoftVault' -version=2 kv

echo "Writing antsoft keys"
vault kv put -mount=AntSoftVault antsoft public_key=""
vault kv put -mount=AntSoftVault antsoft public_key=""
vault kv patch -mount=AntSoftVault antsoft secret_key=""
vault kv patch -mount=AntSoftVault antsoft webhook_secret=""

exit 0