import * as cdk from '@aws-cdk/core';
import { DynamodbConstruct } from './constructs/dynamodb-construct';

export class PersistenceStack extends cdk.Stack{
    public dynamoDb: DynamodbConstruct;
    constructor(scope: cdk.Construct, id:string, props?: cdk.StackProps){
        super(scope, id, props);

        this.dynamoDb = new DynamodbConstruct(this, "billing-alert-dynamodb");
    }
}