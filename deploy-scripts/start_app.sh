#!/bin/bash
chmod +x /var/www/paymentsapi/net-payments-webserver
systemctl enable paymentsapi.service
systemctl start paymentsapi.service