// -----------------------------------------------------------------------
// <copyright file="LocalStackServices.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The <c>LocalStack</c> services.
/// </summary>
public static class LocalStackServices
{
    /// <summary>
    /// The <c>LocalStack</c> services.
    /// </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1135:Declare enum member with zero value (when enum has FlagsAttribute)", Justification = "The zero value is used as the default.")]
    public enum Community : long
    {
        /// <summary>
        /// AWS Certificate Manager.
        /// </summary>
        [System.ComponentModel.Description("ACM")]
        CertificateManager = 1L << 0,

        /// <summary>
        /// API Gateway.
        /// </summary>
        ApiGateway = 1L << 1,

        /// <summary>
        /// CloudFormation.
        /// </summary>
        CloudFormation = 1L << 2,

        /// <summary>
        /// CloudWatch.
        /// </summary>
        CloudWatch = 1L << 3,

        /// <summary>
        /// Config.
        /// </summary>
        Config = 1L << 4,

        /// <summary>
        /// DynamoDB.
        /// </summary>
        DynamoDB = 1L << 5,

        /// <summary>
        /// DynamoDB Streams.
        /// </summary>
        DynamoDBStreams = 1L << 6,

        /// <summary>
        /// Elastic Compute Cloud.
        /// </summary>
        [System.ComponentModel.Description("EC2")]
        ElasticComputeCloud = 1L << 7,

        /// <summary>
        /// Elastic Search.
        /// </summary>
        [System.ComponentModel.Description("ES")]
        ElasticSearch = 1L << 8,

        /// <summary>
        /// Events.
        /// </summary>
        Events = 1L << 9,

        /// <summary>
        /// Firehose.
        /// </summary>
        Firehose = 1L << 10,

        /// <summary>
        /// Identity Access Management.
        /// </summary>
        [System.ComponentModel.Description("IAM")]
        IdentityAccessManagement = 1L << 11,

        /// <summary>
        /// Kinesis.
        /// </summary>
        Kinesis = 1L << 12,

        /// <summary>
        /// Key Management Service.
        /// </summary>
        [System.ComponentModel.Description("KMS")]
        KeyManagementService = 1L << 13,

        /// <summary>
        /// Lambda.
        /// </summary>
        Lambda = 1L << 14,

        /// <summary>
        /// Logs.
        /// </summary>
        Logs = 1L << 15,

        /// <summary>
        /// OpenSearch.
        /// </summary>
        OpenSearch = 1L << 16,

        /// <summary>
        /// Redshift.
        /// </summary>
        Redshift = 1L << 17,

        /// <summary>
        /// Resource Groups.
        /// </summary>
        [System.ComponentModel.Description("resource-groups")]
        ResourceGroups = 1L << 18,

        /// <summary>
        /// Resource Group Stagging API.
        /// </summary>
        ResourceGroupStaggingApi = 1L << 19,

        /// <summary>
        /// Route 53.
        /// </summary>
        Route53 = 1L << 20,

        /// <summary>
        /// Route 53 Resolver.
        /// </summary>
        Route53Resolver = 1L << 21,

        /// <summary>
        /// Simple Storage Service.
        /// </summary>
        [System.ComponentModel.Description("S3")]
        SimpleStorageService = 1L << 22,

        /// <summary>
        /// Simple Storage Service Control.
        /// </summary>
        [System.ComponentModel.Description("S3Control")]
        SimpleStorageServiceControl = 1L << 23,

        /// <summary>
        /// Scheduler.
        /// </summary>
        Scheduler = 1L << 24,

        /// <summary>
        /// Secrets Manager.
        /// </summary>
        SecretsManager = 1L << 25,

        /// <summary>
        /// Simple Email Service.
        /// </summary>
        [System.ComponentModel.Description("SES")]
        SimpleEmailService = 1L << 26,

        /// <summary>
        /// Simple Notification Service.
        /// </summary>
        [System.ComponentModel.Description("SNS")]
        SimpleNotificationService = 1L << 27,

        /// <summary>
        /// Simple Queue Service.
        /// </summary>
        [System.ComponentModel.Description("SQS")]
        SimpleQueueService = 1L << 28,

        /// <summary>
        /// Systems Manager.
        /// </summary>
        [System.ComponentModel.Description("SSM")]
        SystemsManager = 1L << 29,

        /// <summary>
        /// Step Functions.
        /// </summary>
        StepFunctions = 1L << 30,

        /// <summary>
        /// Security Token Service.
        /// </summary>
        [System.ComponentModel.Description("STS")]
        SecurityTokenService = 1L << 31,

        /// <summary>
        /// Support.
        /// </summary>
        Support = 1L << 32,

        /// <summary>
        /// Simple Workflow Service.
        /// </summary>
        [System.ComponentModel.Description("SWF")]
        SimpleWorkflowService = 1L << 33,

        /// <summary>
        /// Transcribe.
        /// </summary>
        Transcribe = 1L << 34,
    }
}