import * as cdk from '@aws-cdk/core';
import { SnsConstruct } from './constructs/sns-construct';
import { DynamodbConstruct } from './constructs/dynamodb-construct';
import { LambdaConstruct } from './constructs/lambda-construct';

export class BillingAlertAppStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // The code that defines your stack goes here
    var snsTopic = new SnsConstruct(this, "billing-alert-sns");
    var dynamodbConstruct = new DynamodbConstruct(this, "billing-alert-dynamodb");

    new LambdaConstruct(this, "billing-alert-lambda", {
      billingAlertStoreTableArn: dynamodbConstruct.table.tableArn,
      billingAlertStoreTableName: dynamodbConstruct.table.tableName,
      snsTopicArn: snsTopic.topic.topicArn
    });
  }
}
