systemctl stop paymentsapi.service
systemctl disable paymentsapi.service
sudo service codedeploy-agent stop
rm -rf /opt/codedeploy-agent/deployment-root/*
udo service codedeploy-agent start
