import cdk = require('@aws-cdk/core');
import { CfnDeliveryStream } from '@aws-cdk/aws-kinesisfirehose';
import * as s3 from '@aws-cdk/aws-s3';
import { RemovalPolicy } from '@aws-cdk/core';

export class KinesisFirehoseConstruct extends cdk.Construct{
    public deliveryStream: CfnDeliveryStream;

    constructor(scope: cdk.Construct, id: string){
        super(scope, id);

        // var destinationS3Bucket = new s3.Bucket(this, id, {
        //     bucketName: 'toll-transactions-stream',            
        //     removalPolicy: RemovalPolicy.DESTROY
        // });

        var deliveryStreamName = 'toll-transactions';
        this.deliveryStream = new CfnDeliveryStream(this, id, {
            deliveryStreamName: deliveryStreamName,
            deliveryStreamType: 'DirectPut'
        });
    }
}