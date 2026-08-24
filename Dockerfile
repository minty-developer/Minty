# 1. 빌드 스테이지
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# 하위 폴더(Mini) 안의 csproj 파일을 복사하도록 경로 수정
COPY Mini/*.csproj ./Mini/
RUN dotnet restore Mini/Mini.csproj

# 나머지 소스 코드 전체 복사 및 빌드
COPY . ./
RUN dotnet publish Mini/Mini.csproj -c Release -o out

# 2. 실행 스테이지
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# 빌드 결과물 복사
COPY --from=build-env /app/out .

# 실행 파일 지정 (Mini.dll)
ENTRYPOINT ["dotnet", "Mini.dll"]