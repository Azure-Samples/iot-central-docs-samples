# The cluster name can contain only lowercase letters and numbers.
# It must contain from 4 to 22 characters.
CLUSTER_NAME="<A unique name for your cluster>"

CENTRAL_URL_PREFIX="<The URL prefix of your IoT Central application>"

DATABASE_NAME="phonedata"
LOCATION="eastus"
RESOURCE_GROUP="IoTCentralExportData_rg"

# Login to your subscription - this is necessary to create
# a bearer token to use to authenticate REST API calls
az login

ACCESS_TOKEN=$(az account get-access-token \
    --resource https://${CLUSTER_NAME}.${LOCATION}.kusto.windows.net \
    --query accessToken -o tsv) 

az extension add -n kusto

# Create a resource group for the Azure Data Explorer cluster
az group create --location $LOCATION \
    --name $RESOURCE_GROUP

# Create the Azure Data Explorer cluster
# This command takes at least 10 minutes to run
az kusto cluster create --cluster-name $CLUSTER_NAME \
    --sku name="Standard_D11_v2"  tier="Standard" \
    --enable-streaming-ingest=true \
    --enable-auto-stop=true \
    --resource-group $RESOURCE_GROUP --location $LOCATION

# Create a database in the cluster
az kusto database create --cluster-name $CLUSTER_NAME \
    --database-name $DATABASE_NAME \
    --read-write-database location=$LOCATION soft-delete-period=P365D hot-cache-period=P31D \
    --resource-group $RESOURCE_GROUP

# Create and assign a managed identity to use
# when authenticating from IoT Central.
# This assumes your IoT Central was created in the default
# IOTC resource group.
MI_JSON=$(az iot central app identity assign --name $CENTRAL_URL_PREFIX \
    --resource-group IOTC --system-assigned)

## Assign the managed identity permissions to use the database.
az kusto database-principal-assignment create \
    --cluster-name $CLUSTER_NAME \
    --database-name $DATABASE_NAME \
    --principal-id $(jq -r .principalId <<< $MI_JSON) \
    --principal-assignment-name $CENTRAL_URL_PREFIX \
    --resource-group $RESOURCE_GROUP \
    --principal-type App \
    --tenant-id $(jq -r .tenantId <<< $MI_JSON) \
    --role Admin

# Create the acceleration table
az rest -m post -u https://${CLUSTER_NAME}.${LOCATION}.kusto.windows.net/v1/rest/mgmt \
  --headers Authorization="Bearer $ACCESS_TOKEN" --body \
'{
  "db": "'"$DATABASE_NAME"'",
  "csl": ".create table acceleration (EnqueuedTime:datetime, Device:string, X:real, Y:real, Z:real);"
}'

# Enable streaming ingestion for the table
az rest -m post -u https://${CLUSTER_NAME}.${LOCATION}.kusto.windows.net/v1/rest/mgmt \
  --headers Authorization="Bearer $ACCESS_TOKEN" --body \
'{
  "db": "'"$DATABASE_NAME"'",
  "csl": ".alter table acceleration policy streamingingestion enable"
}'

echo "Azure Data Explorer URL: $(az kusto cluster show --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP --query uri -o tsv)"
