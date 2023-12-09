using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using EfficientSystemsWithSnsSqs;
using System.Text.Json;

string accessKey = "YOUR_ACCESS_KEY";
string secretKey = "YOUR_SECRET_KEY";
string snsTopicArn = "YOUR_SNS_TOPIC_ARN";
string sqsQueueUrl = "YOUR_SQS_QUEUE_URL";

var snsClient = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, RegionEndpoint.EUCentral1);
var sqsClient = new AmazonSQSClient(accessKey, secretKey, RegionEndpoint.EUCentral1);

// Subscribe the SQS queue to the SNS topic
var subscribeRequest = new SubscribeRequest
{
    TopicArn = snsTopicArn,
    Protocol = "sqs",
    Endpoint = sqsQueueUrl
};

// Publish a message to SNS topic
var snsRequest = new PublishRequest
{
    TopicArn = snsTopicArn,
    Message = "Hello from AWS SNS!"
};

var snsResponse = await snsClient.PublishAsync(snsRequest);
Console.WriteLine($"Message published to SNS with MessageId: {snsResponse.MessageId}");

// Consume the message from SQS queue
var receiveMessageRequest = new ReceiveMessageRequest
{
    QueueUrl = sqsQueueUrl,
    MaxNumberOfMessages = 1,
    WaitTimeSeconds = 20 // Long polling to wait for messages
};

var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);

if (receiveMessageResponse.Messages.Count > 0)
{
    var sqsMessage = receiveMessageResponse.Messages[0];
    var snsMessage = JsonSerializer.Deserialize<SnsMessage>(sqsMessage.Body);

    Console.WriteLine($"Received message from SQS: {snsMessage.Message}");

    // Delete the message from the queue
    var deleteMessageRequest = new DeleteMessageRequest
    {
        QueueUrl = sqsQueueUrl,
        ReceiptHandle = sqsMessage.ReceiptHandle
    };

    await sqsClient.DeleteMessageAsync(deleteMessageRequest);
    Console.WriteLine("Message deleted from SQS queue.");
}
else
{
    Console.WriteLine("No messages found in SQS queue.");
}