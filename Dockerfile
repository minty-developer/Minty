# 1. 빌드 스테이지 (SDK 포함) - 26년 8월 기준 정식 .NET 10.0 SDK 사용
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# 프로젝트 파일(csproj)을 먼저 복사하고 NuGet 패키지 복원
# (프로젝트 파일 이름이 다르면 실제 이름으로 수정해 주세요!)
COPY *.csproj ./
RUN dotnet restore

# 나머지 소스 코드 전체 복사 및 빌드
COPY . ./
RUN dotnet publish -c Release -o out

# 2. 실행 스테이지 (가벼운 Runtime만 포함) - 정식 .NET 10.0 ASP.NET Runtime 사용
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# 빌드 스테이지에서 생성된 결과물만 가져오기
COPY --from=build-env /app/out .

# .env 파일은 Render 환경 변수로 대체하므로 복사하지 않음

# 봇 실행 명령 (프로젝트 DLL 파일 이름으로 꼭 수정해 주세요!)
# 예: 프로젝트 이름이 MyBot이면 MyBot.dll
ENTRYPOINT ["dotnet", "Minty.dll"]