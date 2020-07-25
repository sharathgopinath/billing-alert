import cdk = require('@aws-cdk/core');
import * as s3 from '@aws-cdk/aws-s3';
import { RemovalPolicy } from '@aws-cdk/core';

interface S3ConstructProps{
    s3BucketName: string;
}

export class S3BucketConstruct extends cdk.Construct{
    public bucket: s3.IBucket;

    constructor(scope: cdk.Construct, id: string, props: S3ConstructProps){
        super(scope, id);

        this.bucket = new s3.Bucket(this, id, {
            bucketName: props.s3BucketName,            
            removalPolicy: RemovalPolicy.DESTROY
        });
    }
}