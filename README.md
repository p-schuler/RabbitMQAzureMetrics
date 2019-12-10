# RabbitMqAzureMetrics
Collects metrics from RabbitMq and publishes them into an App Insight instance on Azure. The metric collector is pulling metrics from a RabbitMQ instance, parses them and publishes them using different custom dimensions. 



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




