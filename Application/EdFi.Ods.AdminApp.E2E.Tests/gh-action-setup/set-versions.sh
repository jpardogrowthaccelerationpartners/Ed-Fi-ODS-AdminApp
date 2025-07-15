#! /bin/bash

NUGET_URL="https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_packaging/EdFi/nuget/v3/index.json"

WEB="EdFi.Suite3.ODS.AdminApp.Web"
WEB_PACKAGE=$(nuget list $WEB -Source $NUGET_URL -PreRelease)
WEB_VERSION=$(echo $WEB_PACKAGE| cut -d' ' -f 2)
echo "Using $WEB Version: $WEB_VERSION"

DB="EdFi.Suite3.ODS.AdminApp.Database"
DB_PACKAGE=$(nuget list $DB -Source $NUGET_URL -PreRelease)
DB_VERSION=$(echo $DB_PACKAGE| cut -d' ' -f 2)
echo "Using $DB Version: $DB_VERSION"

# Check if files exist before trying to modify them
DB_DOCKERFILE="ods-docker/DB-Admin/Alpine/pgsql/Dockerfile"
WEB_DOCKERFILE="main/Docker/pgsql.Dockerfile"

if [ ! -f "$DB_DOCKERFILE" ]; then
    echo "Error: Database Dockerfile not found at $DB_DOCKERFILE"
    exit 1
fi

if [ ! -f "$WEB_DOCKERFILE" ]; then
    echo "Error: Web Dockerfile not found at $WEB_DOCKERFILE"
    exit 1
fi

# Clean up any existing changelog.txt
rm -f changelog.txt

# Update database version
sed -i -E "s/ENV ADMINAPP_DATABASE_VERSION=\"[0-9]+\.[0-9]+\.[0-9]+(-alpha\.[0-9]+\.[0-9]+)?\"/ENV ADMINAPP_DATABASE_VERSION=\"$DB_VERSION\"/" "$DB_DOCKERFILE"
DB_RESULT=$?

# Update web version
sed -i -E "s/ARG VERSION=latest/ARG VERSION=\"$WEB_VERSION\"/" "$WEB_DOCKERFILE"
WEB_RESULT=$?

if [ $DB_RESULT -eq 0 ] && [ $WEB_RESULT -eq 0 ]; then
    echo "Files updated successfully"
    echo "Database version updated to: $DB_VERSION"
    echo "Web version updated to: $WEB_VERSION"
    exit 0
else
    echo "Error updating files"
    echo "Database update result: $DB_RESULT"
    echo "Web update result: $WEB_RESULT"
    exit 1
fi
