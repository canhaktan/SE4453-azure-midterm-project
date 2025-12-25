# Stage 1: Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY AzureMidtermProject.csproj ./
RUN dotnet restore "AzureMidtermProject.csproj"

COPY . .
RUN dotnet publish "AzureMidtermProject.csproj" -c Release -o /app/publish

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

# SSH setup
RUN apt-get update \
    && apt-get install -y --no-install-recommends openssh-server \
    && mkdir /var/run/sshd \
    && echo "root:Azure123!" | chpasswd \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# SSH config
COPY sshd_config /etc/ssh/sshd_config

# Startup script
COPY start.sh /app/start.sh
RUN chmod +x /app/start.sh

EXPOSE 8080 2222

ENTRYPOINT ["/app/start.sh"]