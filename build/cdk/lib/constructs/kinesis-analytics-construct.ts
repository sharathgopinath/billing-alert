import cdk = require('@aws-cdk/core');
import { CfnApplication } from '@aws-cdk/aws-kinesisanalytics';
import analytics from '../../helpers/analytics';
import { ServicePrincipal, Role, PolicyStatement } from '@aws-cdk/aws-iam';

interface KinesisAnalyticsConstructProps{
    firehoseInputArn: string;
    streamOutputArn: string;
}

export class KinesisAnalyticsConstruct extends cdk.Construct{
    constructor(scope: cdk.Construct, id: string, props: KinesisAnalyticsConstructProps){
        super(scope, id);

        var applicationName = 'toll-transactions-analytics';
        var streamNamePrefix = 'TOLL_TRANSACTIONS_STREAM';

        var firehoseAccessRole = new Role(this, 'firehose-input-role', {
            assumedBy: new ServicePrincipal('kinesisanalytics.amazonaws.com'),
            description: `${applicationName} kinesis analytics role`,
            roleName: `${applicationName}-role`
        });
        firehoseAccessRole.addToPolicy(this.createDataStreamAccessPolicy(props.streamOutputArn));
        firehoseAccessRole.addToPolicy(this.createFirehoseAccessPolicy(props.firehoseInputArn));

        var cfnApplication = new CfnApplication(this, applicationName,{
            applicationName: applicationName,
            applicationDescription: 'Sum the total incurred for each customer',
            applicationCode: analytics.getCode(streamNamePrefix),
            inputs: [{
                kinesisFirehoseInput: {
                    creationStack: [id],
                    resourceArn: props.firehoseInputArn,
                    roleArn: firehoseAccessRole.roleArn
                },
                creationStack: [id],
                inputSchema: {
                    creationStack: [id],
                    recordColumns: [
                        {
                            creationStack:[id],
                            name: 'customerId',
                            sqlType: 'integer',
                            mapping: 'CUSTOMERID'
                        },
                        {
                            creationStack:[id],
                            name: 'tollId',
                            sqlType: 'integer',
                            mapping: 'TOLLID'
                        },
                        {
                            creationStack:[id],
                            name: 'tollAmount',
                            sqlType: 'integer',
                            mapping: 'TOLLAMOUNT'
                        }
                    ],
                    recordFormat: {
                        creationStack: [id],
                        recordFormatType: 'JSON'
                    }
                },
                namePrefix: streamNamePrefix
            }]
        });
    }

    private createDataStreamAccessPolicy(streamOutputArn: string){
        return new PolicyStatement({
            actions: [
                "kinesis:DescribeStream",
                "kinesis:GetShardIterator",
                "kinesis:GetRecords",
                "kinesis:PutRecord",
                "kinesis:PutRecords"
            ],
            resources: [
                streamOutputArn
            ]
        });
    }

    private createFirehoseAccessPolicy(firehoseArn: string){
        return new PolicyStatement({
            actions: [
                "firehose:DescribeDeliveryStream",
                "firehose:Get*"
            ],
            resources: [
                firehoseArn
            ]
        });
    }
}