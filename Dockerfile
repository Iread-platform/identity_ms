FROM mcr.microsoft.com/dotnet/sdk:5.0

MAINTAINER Yazan Kassam, yazankassam.codavia@gmail.com

WORKDIR /app

ENV ASPNETCORE_URLS="http://[+]:5015"

ENV ASPNETCORE_ENVIRONMENT=Development

ENV ConnectionStrings:DefaultConnection="Server=mysql_identity_ms;Database=identity_ms_db;Uid=codavia;Pwd=cod@v!@; convert zero datetime=True"

ENV ConsulConfig:Host="http://consul:8500"

ENV ConsulConfig:MyHost="identity_ms"

EXPOSE 5015

COPY ./publish .

ENTRYPOINT ["dotnet","iread_identity_ms.dll"]


