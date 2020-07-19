import cdk = require('@aws-cdk/core');
import * as lambda from '@aws-cdk/aws-lambda';
import * as logs from '@aws-cdk/aws-logs';
import * as iam from '@aws-cdk/aws-iam';

interface lambdaProps{
    snsTopicArn: string;
    billingAlertStoreTableArn: string;
    billingAlertStoreTableName: string;
}

export class LambdaConstruct extends cdk.Construct{
    public fn: lambda.Function;
    constructor(scope: cdk.Construct, id: string, props: lambdaProps){
        super(scope, id);
        
        var functionName = 'billing-alert';
        this.fn = new lambda.Function(this, functionName, {
            runtime: lambda.Runtime.DOTNET_CORE_3_1,
            logRetention: logs.RetentionDays.FIVE_DAYS,
            handler: 'BillingAlert::BillingAlert.Function::Execute',
            code: lambda.Code.fromAsset("../../src/Consumer/BillingAlert"),
            environment: {
                SnsSettings__TopicArn: props.snsTopicArn,
                BillingAlertStore__TableName: props.billingAlertStoreTableName
            }
        });
        this.fn.addToRolePolicy(this.createSnsAccessPolicy(props.snsTopicArn));
        this.fn.addToRolePolicy(this.createDynamodbAccessPolicy(props.billingAlertStoreTableArn));
    }

    private createSnsAccessPolicy(snsArn: string){
        return new iam.PolicyStatement({
            actions: [
                'SNS:Publish',
                'SNS:GetTopicAttributes'
            ],
            resources: [
                snsArn
            ]
        });
    }

    private createDynamodbAccessPolicy(tableArn: string){
        return new iam.PolicyStatement({
            actions: [
                "dynamodb:BatchGet*",
                "dynamodb:DescribeStream",
                "dynamodb:DescribeTable",
                "dynamodb:Get*",
                "dynamodb:Query",
                "dynamodb:Scan",
                "dynamodb:BatchWrite*",
                "dynamodb:CreateTable",
                "dynamodb:Delete*",
                "dynamodb:Update*",
                "dynamodb:PutItem"
            ],
            resources: [
                tableArn
            ]
        });
    }
}