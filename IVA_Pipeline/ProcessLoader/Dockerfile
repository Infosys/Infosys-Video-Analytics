#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ProcessLoader/ProcessLoader.csproj", "ProcessLoader/"]
COPY ["FrameGrabber/FrameGrabber.csproj", "FrameGrabber/"]
COPY ["BusinessComponent/BusinessComponent.csproj", "BusinessComponent/"]
COPY ["ServiceClientLibrary/ServiceClientLibrary.csproj", "ServiceClientLibrary/"]
COPY ["ObjectDetector.Contracts/ObjectDetector.Contracts.csproj", "ObjectDetector.Contracts/"]
COPY ["TaskRoute/TaskRoute.csproj", "TaskRoute/"]
COPY ["BusinessEntity/BusinessEntity.csproj", "BusinessEntity/"]
COPY ["DataEntity/DataEntity.csproj", "DataEntity/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["IDataAccess/IDataAccess.csproj", "IDataAccess/"]
COPY ["ProcessScheduler/ProcessScheduler.csproj", "ProcessScheduler/"]
COPY ["ProcessScheduler.Framework/ProcessScheduler.Framework.csproj", "ProcessScheduler.Framework/"]
COPY ["Processes/Processes.csproj", "Processes/"]
COPY ["TCPChannelCommunication/TCPChannelCommunication.csproj", "TCPChannelCommunication/"]
COPY ["MaskPrediction/Prediction.csproj", "MaskPrediction/"]
RUN dotnet restore "ProcessLoader/ProcessLoader.csproj"
COPY . .
WORKDIR "/src/ProcessLoader"
RUN dotnet build "ProcessLoader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProcessLoader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProcessLoader.dll"]