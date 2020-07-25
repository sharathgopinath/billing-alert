import * as cdk from '@aws-cdk/core';
import * as s3 from '@aws-cdk/aws-s3';
import { S3BucketConstruct } from './constructs/s3-bucket-construct';

interface DepAssetsStackProps{
    S3BucketName: string;
}

export class DepAssetsStack extends cdk.Stack{
    public bucket: s3.IBucket;

    constructor(scope: cdk.Construct, id:string, stackProps: DepAssetsStackProps, props?: cdk.StackProps){
        super(scope, id, props);

        var s3BucketContruct = new S3BucketConstruct(this, "billing-alert-s3", {s3BucketName:stackProps.S3BucketName});
        this.bucket = s3BucketContruct.bucket;
    }
}