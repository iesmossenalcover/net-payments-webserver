#!/bin/bash
rm -rf /var/www/*
rm -rf /etc/systemd/system/paymentsapi.service
cp ./deploy-scripts/paymentsapi.service /etc/systemd/system/