import cdk = require('@aws-cdk/core');
import { CfnDeliveryStream } from '@aws-cdk/aws-kinesisfirehose';
import * as s3 from '@aws-cdk/aws-s3';
import { RemovalPolicy } from '@aws-cdk/core';
import { ServicePrincipal, Role, PolicyStatement } from '@aws-cdk/aws-iam';
import * as log from '@aws-cdk/aws-logs';

export class KinesisFirehoseConstruct extends cdk.Construct{
    public deliveryStream: CfnDeliveryStream;

    constructor(scope: cdk.Construct, id: string){
        super(scope, id);

        var deliveryStreamName = 'toll-transactions';

        var bucketName = 'toll-transactions-stream';
        var destinationS3Bucket = new s3.Bucket(this, 'delievry-stream-bucket', {
            bucketName: bucketName,            
            removalPolicy: RemovalPolicy.DESTROY
        });

        var logGroup = new log.LogGroup(this, 'log-group', {
            logGroupName: `${deliveryStreamName}-log`,
            removalPolicy: RemovalPolicy.DESTROY,
            retention: log.RetentionDays.FIVE_DAYS
        })

        var deliveryStreamRole = new Role(this, 'delivery-stream-role', {
            assumedBy: new ServicePrincipal('firehose.amazonaws.com'),
            description: `${deliveryStreamName} kinesis firehose role`,
            roleName: `${deliveryStreamName}-role`
        });

        deliveryStreamRole.addToPolicy(this.createBucketAccessPolicy(bucketName));
        deliveryStreamRole.addToPolicy(this.createLogEventAccessPolicy(logGroup));

        this.deliveryStream = new CfnDeliveryStream(this, deliveryStreamName, {
            deliveryStreamName: deliveryStreamName,
            deliveryStreamType: 'DirectPut',
            s3DestinationConfiguration: {
                bucketArn: destinationS3Bucket.bucketArn,
                creationStack: [id],
                roleArn: deliveryStreamRole.roleArn,
            }
        });
    }

    private createBucketAccessPolicy(bucketName: string){
        return new PolicyStatement({
            actions: [
                "s3:AbortMultipartUpload",
                "s3:GetBucketLocation",
                "s3:GetObject",
                "s3:ListBucket",
                "s3:ListBucketMultipartUploads",
                "s3:PutObject"
            ],
            resources: [
                `arn:aws:s3:::${bucketName}`,
                `arn:aws:s3:::${bucketName}/*`
            ]
        })
    }

    private createLogEventAccessPolicy(logGroup: log.LogGroup){
        return new PolicyStatement({
            actions: [
                "logs:PutLogEvents"
            ],
            resources: [logGroup.logGroupArn]
        })
    }
}