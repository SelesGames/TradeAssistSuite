FROM microsoft/dotnet-framework

ADD publish/ /

# Creates a directory for the Host
#WORKDIR tempstore

# Listen on port 1942.
EXPOSE 1942

ENTRYPOINT TradeAssist.Realtime.exe