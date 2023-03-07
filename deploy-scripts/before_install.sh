#!/bin/bash
rm -rf /var/www/paymentsapi
rm -rf /etc/systemd/system/paymentsapi.service
sudo service codedeploy-agent stop
rm -rf /opt/codedeploy-agent/deployment-root/*
udo service codedeploy-agent start
