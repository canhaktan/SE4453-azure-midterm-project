#!/bin/bash

set -e

echo "Starting SSH..."
/usr/sbin/sshd

echo "Starting .NET application..."
exec dotnet AzureMidtermProject.dll