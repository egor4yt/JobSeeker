﻿namespace JobSeeker.Deduplication.Shared;

public class ConfigurationKeys
{
    public const string DatabaseConnectionString = "DatabaseConnection";
    public const string MessageQueueConnectionString = "ConnectionStrings:MessageQueue";
    public const string ObjectStorageConnectionString = "ConnectionStrings:ObjectStorage";
    public const string CacheConnectionString = "ConnectionStrings:CacheBroker";
    public const string MessageQueueBroker = "MessageQueueBroker";
    public const string ObjectStorageBroker = "ObjectStorageBroker";
    public const string CacheBroker = "CacheBroker";
}