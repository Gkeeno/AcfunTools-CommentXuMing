#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AcfunTools.CommentXuMing.Web/AcfunTools.CommentXuMing.Web.csproj", "AcfunTools.CommentXuMing.Web/"]
RUN dotnet restore "AcfunTools.CommentXuMing.Web/AcfunTools.CommentXuMing.Web.csproj"
COPY . .
WORKDIR "/src/AcfunTools.CommentXuMing.Web"
RUN dotnet build "AcfunTools.CommentXuMing.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcfunTools.CommentXuMing.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AcfunTools.CommentXuMing.Web.dll"]