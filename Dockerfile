# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG NUGET_TOKEN
WORKDIR /src

COPY ./src .
COPY NuGet.Config .
 
RUN dotnet restore "Template.WebApi/Template.WebApi.csproj" --configfile "NuGet.Config"
COPY . .
WORKDIR "/src/Template.WebApi"

RUN dotnet build "Template.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/app/datadog/linux-musl-x64/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/app/datadog
ENV LD_PRELOAD=/app/datadog/linux-musl-x64/Datadog.Linux.ApiWrapper.x64.so

# Run the createLogPath script on Linux to ensure the automatic instrumentation logs are generated without permission issues
RUN ["sh", "/app/datadog/createLogPath.sh"]

ENTRYPOINT ["dotnet", "Template.WebApi.dll"]