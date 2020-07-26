import cdk = require('@aws-cdk/core');
import * as sns from '@aws-cdk/aws-sns';

export class SnsConstruct extends cdk.Construct{
    public topic:sns.Topic;

    constructor(scope: cdk.Construct, id: string) {
        super(scope, id);

        var topicName = 'toll-amount-threshold-breached';
        this.topic = new sns.Topic(this, topicName, {
            topicName: topicName
        });
    }
}