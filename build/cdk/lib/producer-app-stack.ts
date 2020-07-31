import * as cdk from '@aws-cdk/core';
import { KinesisFirehoseConstruct } from './constructs/kinesis-firehose-construct';
import { CfnDeliveryStream } from '@aws-cdk/aws-kinesisfirehose';

export class ProducerStack extends cdk.Stack{
    public deliveryStream: CfnDeliveryStream;
    
    constructor(scope: cdk.Construct, id:string, props?: cdk.StackProps){
        super(scope, id, props);

        var kinesisFirehoseConstruct = new KinesisFirehoseConstruct(this, 'billing-alert-deliverystream')
        this.deliveryStream = kinesisFirehoseConstruct.deliveryStream;
    }
}