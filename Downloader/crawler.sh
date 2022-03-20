#!/bin/bash

if [ ! $UID -eq 0 ]
then
  echo "$UID: Required Root authorization."
  exit 1
fi

MARKET_ID=
if [ $# -gt 0 ]
then
  MARKET_ID=$1
fi


echo 'Start'

while [ 1 ]
do
  echo 'Restart tor service'
  systemctl restart tor.service

  echo "Execute Download $MARKET_ID"
  sudo --user=arch \
    dotnet run --project Stq.Downloader.Stock/Stq.Downloader.Stock.csproj -- dataset.db $MARKET_ID

  if [ ! -f 'invalid_company.json' ]
  then
    break
  fi
done

echo 'Finished!'
