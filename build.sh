#!/bin/bash

dotnet tool restore
dotnet paket install
npm install
npm run build
