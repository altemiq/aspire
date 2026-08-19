// -----------------------------------------------------------------------
// <copyright file="MiniStackServices.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The <c>MiniStack</c> services.
/// </summary>
public sealed class MiniStackServices : IEnumerable<MiniStackService>
{
    /// <summary>
    /// GetAccountInformation (with AccountState), GetContactInformation, ListRegions opt-in matrix.
    /// </summary>
    public static readonly MiniStackService Account = new(nameof(Account), "account");

    /// <summary>
    /// Request, import, describe certificates; DNS validation records; SANs; tags.
    /// </summary>
    public static readonly MiniStackService CertificateManager = new(nameof(CertificateManager), "acm");

    /// <summary>
    /// Vaults, plans, recovery points, tags — control plane stub.
    /// </summary>
    public static readonly MiniStackService Backup = new(nameof(Backup), "backup");

    /// <summary>
    /// Compute environments, job queues, job definitions (auto-revisioning), SubmitJob — Pro-only on LocalStack.
    /// </summary>
    public static readonly MiniStackService Batch = new(nameof(Batch), "batch");

    /// <summary>
    /// HTTP APIs, Lambda proxy, path params, execute-api data plane.
    /// </summary>
    public static readonly MiniStackService ApiGateway = new(nameof(ApiGateway), "apigateway");

    /// <summary>
    /// Applications, environments, profiles, deployments, hosted versions, configuration sessions data plane.
    /// </summary>
    public static readonly MiniStackService AppConfig = new(nameof(AppConfig), "appconfig");

    /// <summary>
    /// <see cref="AppConfig"/> data.
    /// </summary>
    public static readonly MiniStackService AppConfigData = new(nameof(AppConfigData), "appconfigdata");

    /// <summary>
    /// Serverless GraphQL architecture mocking local DataSources and complex Resolvers.
    /// </summary>
    public static readonly MiniStackService AppSync = new(nameof(AppSync), "appsync");

    /// <summary>
    /// Realtime WebSocket + HTTP publish on /event, aws-appsync-event-ws subprotocol, connection-scoped authorization, strict mode via APPSYNC_EVENTS_ENFORCE_AUTH=1.
    /// </summary>
    public static readonly MiniStackService AppSyncEvents = new(nameof(AppSyncEvents), "appsyncevents");

    /// <summary>
    /// Real SQL via DuckDB (optional), data catalogs, prepared statements.
    /// </summary>
    public static readonly MiniStackService Athena = new(nameof(Athena), "athena");

    /// <summary>
    /// ASGs, launch configs, scaling policies, lifecycle hooks, scheduled actions, tags.
    /// </summary>
    public static readonly MiniStackService AutoScaling = new(nameof(AutoScaling), "autoscaling");

    /// <summary>
    /// Stack lifecycle, change sets, 68 resource types, YAML/JSON templates, intrinsic functions, rollback, cross-stack exports.
    /// </summary>
    public static readonly MiniStackService CloudFormation = new(nameof(CloudFormation), "cloudformation");

    /// <summary>
    /// High-throughput edge location edge server emulation targeting origins natively.
    /// </summary>
    public static readonly MiniStackService CloudFront = new(nameof(CloudFront), "cloudfront");

    /// <summary>
    /// Create / Describe / Delete stores, key CRUD via the cloudfront-keyvaluestore data plane (/key-value-stores/).
    /// </summary>
    public static readonly MiniStackService CloudFrontKeyValueStore = new(nameof(CloudFrontKeyValueStore), "cloudfront-keyvaluestore");

    /// <summary>
    /// Projects, builds, start/stop, batch operations, metadata stored in-memory.
    /// </summary>
    public static readonly MiniStackService CodeBuild = new(nameof(CodeBuild), "codebuild");

    /// <summary>
    /// User pools, auth flows, TOTP MFA, identity pools, federated credentials.
    /// </summary>
    public static readonly MiniStackService CognitoIdentity = new(nameof(CognitoIdentity), "cognito-identity");

    /// <summary>
    /// User pools, auth flows, TOTP MFA, identity pools, federated credentials.
    /// </summary>
    public static readonly MiniStackService CognitoIdp = new(nameof(CognitoIdp), "cognito-idp");

    /// <summary>
    /// Tables, CRUD, query, scan, transactions, TTL, GSI.
    /// </summary>
    public static readonly MiniStackService DynamoDB = new(nameof(DynamoDB), "dynamodb");

    /// <summary>
    /// Stream records emitted by DynamoDB writes; GetShardIterator / GetRecords honoring StreamViewType.
    /// </summary>
    public static readonly MiniStackService DynamoDBStreams = new(nameof(DynamoDBStreams), "dynamodbstreams");

    /// <summary>
    /// Instances, VPCs, subnets, security groups, route tables, ENIs, elastic IPs, NAT gateways, NACLs, flow logs, VPC peering, DHCP options, egress-only IGWs.
    /// </summary>
    public static readonly MiniStackService ElasticComputeCloud = new(nameof(ElasticComputeCloud), "ec2");

    /// <summary>
    /// In-memory container registry providing Docker V2 manifests and full lifecycle rules.
    /// </summary>
    public static readonly MiniStackService ElasticContainerRegistry = new(nameof(ElasticContainerRegistry), "ecr");

    /// <summary>
    /// RunTask starts real containers, capacity providers.
    /// </summary>
    public static readonly MiniStackService ElasticContainerService = new(nameof(ElasticContainerService), "ecs");

    /// <summary>
    /// <see cref="ElasticContainerService"/> metadata.
    /// </summary>
    public static readonly MiniStackService ElasticContainerServiceMetadata = new(nameof(ElasticContainerServiceMetadata), "ecs-metadata");

    /// <summary>
    /// Clusters, node groups, Fargate profiles, addons — Pro-only on LocalStack.
    /// </summary>
    public static readonly MiniStackService ElasticKubernetesService = new(nameof(ElasticKubernetesService), "eks");

    /// <summary>
    /// Real Redis/Memcached containers, users, user groups.
    /// </summary>
    public static readonly MiniStackService ElastiCache = new(nameof(ElastiCache), "elasticache");

    /// <summary>
    /// Elastic file system.
    /// </summary>
    public static readonly MiniStackService ElasticFileSystem = new(nameof(ElasticFileSystem), "elasticfilesystem");

    /// <summary>
    /// Load balancers, target groups, listeners, rules, Lambda targets + live data-plane routing — Pro-only on LocalStack.
    /// </summary>
    public static readonly MiniStackService ElasticLoadBalancing = new(nameof(ElasticLoadBalancing), "elasticloadbalancing");

    /// <summary>
    /// Elastic map reduce.
    /// </summary>
    public static readonly MiniStackService ElasticMapReduce = new(nameof(ElasticMapReduce), "elasticmapreduce");

    /// <summary>
    /// Buses, rules, targets, Lambda dispatch, archives, permissions.
    /// </summary>
    public static readonly MiniStackService EventBridge = new(nameof(EventBridge), "events");

    /// <summary>
    /// Delivery streams, PutRecord/PutRecordBatch, S3 delivery, encryption, tags.
    /// </summary>
    public static readonly MiniStackService Firehose = new(nameof(Firehose), "firehose");

    /// <summary>
    /// Catalog, crawlers, jobs, triggers, workflows.
    /// </summary>
    public static readonly MiniStackService Glue = new(nameof(Glue), "glue");

    /// <summary>
    /// Managed Workflows for Apache Airflow — real apache/airflow:&lt;version&gt; containers (2.x and 3.x), DAG sync from S3, InvokeRestApi proxy.
    /// </summary>
    public static readonly MiniStackService AirFlow = new(nameof(AirFlow), "airflow");

    /// <summary>
    /// Users, roles, policies, groups, instance profiles, OIDC.
    /// </summary>
    public static readonly MiniStackService IdentityAccessManagement = new(nameof(IdentityAccessManagement), "iam");

    /// <summary>
    /// EC2 Instance Metadata Service v1 + v2 — SDK credential chains resolve via AWS_EC2_METADATA_SERVICE_ENDPOINT.
    /// </summary>
    public static readonly MiniStackService InstanceMetadataService = new(nameof(InstanceMetadataService), "imds");

    /// <summary>
    /// Things / certificates / policies, MQTT 3.1.1 over WebSocket on the gateway port, HTTP iot-data Publish, Local CA, persistent sessions + QoS 1 + LWT.
    /// </summary>
    public static readonly MiniStackService IoT = new(nameof(IoT), "iot");

    /// <summary>
    /// <see cref="IoT"/> data.
    /// </summary>
    public static readonly MiniStackService IoTData = new(nameof(IoTData), "iot-data");

    /// <summary>
    /// Streams, split/merge shards, consumers, encryption, monitoring.
    /// </summary>
    public static readonly MiniStackService Kinesis = new(nameof(Kinesis), "kinesis");

    /// <summary>
    /// RSA &amp; symmetric envelope encryption for internal cryptographic signatures.
    /// </summary>
    public static readonly MiniStackService KeyManagementService = new(nameof(KeyManagementService), "kms");

    /// <summary>
    /// Real Python execution, warm workers, SQS event source mapping, Layers.
    /// </summary>
    public static readonly MiniStackService Lambda = new(nameof(Lambda), "lambda");

    /// <summary>
    /// Groups, streams, retention, subscription filters, metric filters, Insights.
    /// </summary>
    public static readonly MiniStackService Logs = new(nameof(Logs), "logs");

    /// <summary>
    /// Media connect.
    /// </summary>
    public static readonly MiniStackService MediaConnect = new(nameof(MediaConnect), "mediaconnect");

    /// <summary>
    /// Domains, versions, change progress, tags — optional real opensearchproject/opensearch cluster + Dashboards sidecar via OPENSEARCH_DATAPLANE=1.
    /// </summary>
    public static readonly MiniStackService OpenSearch = new(nameof(OpenSearch), "opensearch");

    /// <summary>
    /// DescribeOrganization, ListRoots, accounts, OUs with the new Path field.
    /// </summary>
    public static readonly MiniStackService Organizations = new(nameof(Organizations), "organizations");

    /// <summary>
    /// Monitoring.
    /// </summary>
    public static readonly MiniStackService Monitoring = new(nameof(Monitoring), "monitoring");

    /// <summary>
    /// Real Postgres/MySQL containers.
    /// </summary>
    public static readonly MiniStackService RelationalDatabaseService = new(nameof(RelationalDatabaseService), "rds");

    /// <summary>
    /// ExecuteStatement, BatchExecute, transactions — routes SQL to real RDS containers.
    /// </summary>
    public static readonly MiniStackService RelationalDatabaseServiceData = new(nameof(RelationalDatabaseServiceData), "rds-data");

    /// <summary>
    /// GetResources, GetTagKeys/Values, TagResources across S3, Lambda, SQS, SNS, DDB, KMS, ECR, ECS, EFS, AppSync, scheduler, more.
    /// </summary>
    public static readonly MiniStackService ResourceGroups = new(nameof(ResourceGroups), "resource-groups");

    /// <summary>
    /// Hosted zones, record sets (CREATE/UPSERT/DELETE), health checks, tags, alias records.
    /// </summary>
    public static readonly MiniStackService Route53 = new(nameof(Route53), "route53");

    /// <summary>
    /// Buckets, objects, versioning, encryption, lifecycle, CORS, Object Lock, replication.
    /// </summary>
    public static readonly MiniStackService SimpleStorageService = new(nameof(SimpleStorageService), "s3");

    /// <summary>
    /// File systems, mount targets, access points, policies — new April 2026 S3 service.
    /// </summary>
    public static readonly MiniStackService SimpleStorageServiceFiles = new(nameof(SimpleStorageServiceFiles), "s3-files");

    /// <summary>
    /// Schedules, schedule groups, rate/cron expressions, target dispatch.
    /// </summary>
    public static readonly MiniStackService Scheduler = new(nameof(Scheduler), "scheduler");

    /// <summary>
    /// CRUD, versioning, rotation, resource policies.
    /// </summary>
    public static readonly MiniStackService SecretsManager = new(nameof(SecretsManager), "secretsmanager");

    /// <summary>
    /// HTTP/DNS namespaces, services, instance registration, discovery, Route53 integration.
    /// </summary>
    public static readonly MiniStackService CloudMap = new(nameof(CloudMap), "servicediscovery");

    /// <summary>
    /// Send email/raw/templated, identities, configuration sets.
    /// </summary>
    public static readonly MiniStackService SimpleEmailService = new(nameof(SimpleEmailService), "ses");

    /// <summary>
    /// Topics, subscriptions, fanout to SQS, batch publish.
    /// </summary>
    public static readonly MiniStackService SimpleNotificationService = new(nameof(SimpleNotificationService), "sns");

    /// <summary>
    /// Queues, FIFO, DLQ, batch, visibility.
    /// </summary>
    public static readonly MiniStackService SimpleQueueService = new(nameof(SimpleQueueService), "sqs");

    /// <summary>
    /// String, SecureString, paths, labels, tags.
    /// </summary>
    public static readonly MiniStackService SystemsManager = new(nameof(SystemsManager), "ssm");

    /// <summary>
    /// Full ASL engine, sync execution, task tokens, all state types.
    /// </summary>
    public static readonly MiniStackService StepFunctions = new(nameof(StepFunctions), "states");

    /// <summary>
    /// CallerIdentity, AssumeRole, GetSessionToken.
    /// </summary>
    public static readonly MiniStackService SecurityTokenService = new(nameof(SecurityTokenService), "sts");

    /// <summary>
    /// GetResources, GetTagKeys/Values, TagResources across S3, Lambda, SQS, SNS, DDB, KMS, ECR, ECS, EFS, AppSync, scheduler, more.
    /// </summary>
    public static readonly MiniStackService Tagging = new(nameof(Tagging), "tagging");

    /// <summary>
    /// SFTP servers, users, public-key auth, tags.
    /// </summary>
    public static readonly MiniStackService Transfer = new(nameof(Transfer), "transfer");

    /// <summary>
    /// v1 + Regional stub for legacy Terraform / CFN — empty List*, valid GetChangeToken, WAFNonexistentItemException.
    /// </summary>
    public static readonly MiniStackService WebApplicationFirewall = new(nameof(WebApplicationFirewall), "waf");

    /// <summary>
    /// v1 + Regional stub for legacy Terraform / CFN — empty List*, valid GetChangeToken, WAFNonexistentItemException.
    /// </summary>
    public static readonly MiniStackService WebApplicationFirewallRegional = new(nameof(WebApplicationFirewallRegional), "waf-regional");

    /// <summary>
    /// WebACLs, IP sets, rule groups, resource associations, LockToken enforcement — Pro-only on LocalStack.
    /// </summary>
    public static readonly MiniStackService WebApplicationFirewallV2 = new(nameof(WebApplicationFirewallV2), "wafv2");

    /// <summary>
    /// Trail CRUD (stub) — sufficient for IaC stacks that declare trails alongside other resources.
    /// </summary>
    public static readonly MiniStackService CloudTrail = new(nameof(CloudTrail), "cloudtrail");

    /// <summary>
    /// CUR report definitions (stub) — control plane only, no real billing data emitted.
    /// </summary>
    public static readonly MiniStackService CostAndUsageReports = new(nameof(CostAndUsageReports), "cur");

    /// <summary>
    /// 14 ops — Enable, Disable, ListFindings with filtering/sorting/pagination, coverage and aggregation queries, filter and tag CRUD; deterministic stub vulnerability findings for ECR images, Lambda functions, and EC2 instances.
    /// </summary>
    public static readonly MiniStackService Inspector2 = new(nameof(Inspector2), "inspector2");

    /// <summary>
    /// Table buckets, namespaces, Iceberg-format tables; control plane covers Create/List/Get/Delete for buckets, namespaces, and tables plus GetTableMetadataLocation / UpdateTableMetadataLocation; embedded Iceberg REST catalog at /iceberg so Spark jobs commit Iceberg tables without an external catalog.
    /// </summary>
    public static readonly MiniStackService SimpleStorageServiceTables = new(nameof(SimpleStorageServiceTables), "s3tables");

    private readonly IList<MiniStackService> services = [];

    /// <summary>
    /// Initialises a new instance of the <see cref="MiniStackServices"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    internal MiniStackServices(params ReadOnlySpan<MiniStackService> services)
    {
        foreach (var service in services)
        {
            this.services.Add(service);
        }
    }

    /// <summary>
    /// The implicit converter.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns>The services.</returns>
    public static implicit operator MiniStackServices(MiniStackService service) => new(service);

    /// <summary>
    /// The and operator.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="service">The service.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static MiniStackServices operator &(MiniStackServices services, MiniStackService service)
    {
        services.services.Remove(service);
        return services;
    }

    /// <summary>
    /// The or operator.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="service">The service.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static MiniStackServices operator |(MiniStackServices services, MiniStackService service)
    {
        services.services.Add(service);
        return services;
    }

    /// <summary>
    /// The or operator.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="services">The services.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static MiniStackServices operator |(MiniStackService service, MiniStackServices services)
    {
        services.services.Insert(0, service);
        return services;
    }

    /// <inheritdoc />
    public IEnumerator<MiniStackService> GetEnumerator() => this.services.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc />
    public override string ToString() => string.Join(" | ", this.services);
}