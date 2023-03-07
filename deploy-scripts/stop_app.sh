sudo service codedeploy-agent stop
rm -rf /opt/codedeploy-agent/deployment-root/*
sudo service codedeploy-agent start
systemctl stop paymentsapi.service
systemctl disable paymentsapi.service
