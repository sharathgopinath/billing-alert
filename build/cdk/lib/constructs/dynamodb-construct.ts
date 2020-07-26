import cdk = require('@aws-cdk/core');
import * as dynamodb from '@aws-cdk/aws-dynamodb';

export class DynamodbConstruct extends cdk.Construct{
    public table:dynamodb.Table;

    constructor(scope: cdk.Construct, id: string){
        super(scope, id);

        var tableName = 'billing-alert'
        this.table = new dynamodb.Table(this, tableName, {
            partitionKey: {name: 'customerid', type: dynamodb.AttributeType.NUMBER},
            billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
            tableName: tableName
        });
    }
}