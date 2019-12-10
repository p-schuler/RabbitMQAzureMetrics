# RabbitMQ Azure Application Insights Metrics Collector
Collects metrics from RabbitMq and publishes them into an App Insight instance on Azure. The metric collector is pulling metrics from a RabbitMQ instance, parses them and publishes them using different custom dimensions. 

The code is accesses the [RabbitMQ management HTTP API](https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.2/priv/www/api/index.html). Not all metrics that this API exposes are currently collected. See [Collected Metrics](#collected-metrics)

# Supported Versions
This code was tested against management version of RabbitMQ 3.8.2. Other versions might work unless RabbitMQ changes the API format of the management metrics API.

# Prerequisits
1. [RabbitMQ](#rabbitmq) instance with the [Managment Plugin](https://www.rabbitmq.com/management.html)
2. Application Insights
3. Docker (you can host the publisher anywhere, docker is what this setup provides)

# Setup
## RabbitMQ
1. docker run -d --hostname localhost --name local-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
2. validate you can acess the management portal by pointing your browser to http://localhost:15672 (username: guest, password: guest)

## Application Insights
1. Open your azure portal
2. Create a Resource -> Application Insights
3. Select Subscription, Resource Group, a Region and chose a name -> Review + Create
5. Once the Application Insights is created, open it and copy the Instrumentation Key from the Overview Tab
6. Select Usage and estimated Cost
7. Click on "Custom metrics Preview" -> Check "Enable alerting on custom metric dimensions" -> Save

## Metrics collector
1. docker run -d --link local-rabbit --env AppInsightsKey={your app insights key} --name metrics-collector pschuler.azurecr.io/rabbitmqmetricspublisher:latest
2. 

# Collected Metrics
Currently those metrics are being collected
## Churn Rate Metrics
RabbitMQ Path | Description
------------ | -------------
channel_closed | Total channels closed
channel_created | Content in the second column
connection_closed | Total channels created
connection_created | Total connections closed
queue_created | Total queues created
queue_declared | Total queues declared
queue_deleted" | Total queues deleted

## Exchange Metrics
RabbitMQ Path | Description
------------ | -------------
message_stats.publish_in | Messages published in
message_stats.publish_in_details.rate | Rate of messages published in
message_stats.publish_out | Messages published out
message_stats.publish_out_details.rate | Rate of messages published out

## Queue Metrics
RabbitMQ Path | Description
------------ | -------------
consumers | Consumer: Count
consumer_utilisation | Consumer: Utilisation
messages | Total number of messages
messages_details.rate | Rate: Total number of messages
messages_ready | Messages ready for delivery
messages_ready_details.rate | Rate: Messages ready for delivery
messages_unacknowledged | Number of unacknowledged messages
messages_unacknowledged_details.rate | Rate: Number of unacknowledged messages
message_stats.ack | Number of messages in ack mode
message_stats.ack_details.rate | Rate: Number of messages in ack mode
message_stats.deliver_get | Messages delivered recently (of all modes)
message_stats.deliver_get_details.rate | Rate: Messages delivered recently (of all modes)
message_stats.get_no_ack | Messages delivered in no-ack mode in response to basic.get
message_stats.get_no_ack_details.rate | Rate: Messages delivered in no-ack mode in response to basic.get
message_stats.publish | Messages published recently
message_stats.publish_details.rate | Rate: Messages published recently
message_stats.redeliver | Count of subset of messages in deliver_get which had the redelivered flag set.
message_stats.redeliver_details.rate | Rate: Count of subset of messages in deliver_get which had the redelivered flag set.




