import * as cdk from '@aws-cdk/core';
import * as s3 from '@aws-cdk/aws-s3';
import { SnsConstruct } from './constructs/sns-construct';
import { LambdaConstruct } from './constructs/lambda-construct';

interface AppProps{
  billingAlertStoreTableArn:string;
  billingAlertStoreTableName: string;
  s3Bucket: s3.IBucket;
  lambdaPackageName: string;
}

export class AppStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, appProps: AppProps, props?: cdk.StackProps) {
    super(scope, id, props);

    // The code that defines your stack goes here
    var snsTopic = new SnsConstruct(this, "billing-alert-sns");
    
    new LambdaConstruct(this, "billing-alert-lambda", {
      billingAlertStoreTableArn: appProps.billingAlertStoreTableArn,
      billingAlertStoreTableName: appProps.billingAlertStoreTableName,
      snsTopicArn: snsTopic.topic.topicArn,
      lambdaS3Bucket: appProps.s3Bucket,
      lambdaPackageName: appProps.lambdaPackageName
    });
  }
}
