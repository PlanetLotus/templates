#!/bin/bash

projectName=$1

mkdir "$projectName"
cd "$projectName"
git init
dotnet new postgresmvc
npm install
psql -h localhost -U postgres --command="CREATE DATABASE \"$projectName\";"
psql -h localhost -U postgres -d $projectName --file="Data/schema.sql"
cp ../.env.default .env
sed -i "s/Database=;/Database=$projectName;/" .env
